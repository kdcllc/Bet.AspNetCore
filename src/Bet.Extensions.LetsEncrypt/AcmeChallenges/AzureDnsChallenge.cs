using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Bet.Extensions.LetsEncrypt.Order;

using DnsClient;

using Microsoft.Azure.Management.Dns.Fluent;
using Microsoft.Azure.Management.Dns.Fluent.Models;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Rest;

namespace Bet.Extensions.LetsEncrypt.AcmeChallenges
{
    public class AzureDnsChallenge
    {
        private const string DnsChallengeNameFormat = "_acme-challenge.{0}";
        private const string WildcardRegex = "^\\*\\.";

        private readonly LookupClient _lookupClient;
        private readonly ILogger<AzureDnsChallenge> _logger;

        private IOptionsMonitor<AzureAuthenticationOptions> _optionsMonitor;

        public AzureDnsChallenge(
            LookupClient lookupClient,
            IOptionsMonitor<AzureAuthenticationOptions> optionsMonitor,
            ILogger<AzureDnsChallenge> logger)
        {
            _lookupClient = lookupClient ?? throw new ArgumentNullException(nameof(lookupClient));
            _optionsMonitor = optionsMonitor;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task DnsAuthorizationAsync(string named, string instanceId, AcmeChallengeResponse challenge)
        {
            var options = _optionsMonitor.Get(named);

            using var dnsManagementClient = await CreateDnsManagementClientAsync(options);

            var zone = (await dnsManagementClient.Zones.ListAsync()).FirstOrDefault(x => challenge.Domain.EndsWith(x?.Name!));
            var resourceId = ParseResourceId(zone.Id);

            var fullDomainName = GetChallengeDnsName(challenge.Domain);

            var domain = fullDomainName.Replace("." + zone.Name, string.Empty);

            _logger.LogInformation("[Azure Dns Update][Started] Domain Name: {domainName}", challenge.Domain);

            RecordSetInner? recordSet;
            try
            {
                recordSet = await dnsManagementClient.RecordSets.GetAsync(resourceId["resourceGroups"], zone.Name, domain, RecordType.TXT);
            }
            catch
            {
                recordSet = null;
            }

            if (recordSet != null)
            {
                if (recordSet.Metadata == null || !recordSet.Metadata.TryGetValue("InstanceId", out var dnsInstanceId) || dnsInstanceId != instanceId)
                {
                    recordSet.Metadata = new Dictionary<string, string>
                    {
                        { "InstanceId", instanceId }
                    };

                    recordSet.TxtRecords.Clear();
                }

                recordSet.TTL = 3600;
                recordSet.TxtRecords.Add(new TxtRecord(new[] { challenge.Token }));
            }
            else
            {
                recordSet = new RecordSetInner()
                {
                    TTL = 3600,
                    Metadata = new Dictionary<string, string>
                    {
                        { "InstanceId", instanceId }
                    },
                    TxtRecords = new[]
                    {
                        new TxtRecord(new[] { challenge.Token })
                    }
                };
            }

            await dnsManagementClient.RecordSets.CreateOrUpdateAsync(resourceId["resourceGroups"], zone.Name, domain, RecordType.TXT, recordSet);

            _logger.LogInformation("[Azure DNS Update][Ended] Domain Name: {domainName}", challenge.Domain);

            await CheckDnsChallengeAsync(challenge, fullDomainName);
        }

        public async Task DnsPreconditionAsync(string named, IEnumerable<string> dnsNames)
        {
            var options = _optionsMonitor.Get(named);

            using var dnsClient = await CreateDnsManagementClientAsync(options);
            var zones = await dnsClient.Zones.ListAsync();
            foreach (var hostName in dnsNames)
            {
                if (!zones.Any(x => hostName.EndsWith(x.Name)))
                {
                    throw new InvalidOperationException($"Azure DNS zone \"{hostName}\" is not found");
                }
            }
        }

        private string GetChallengeDnsName(string domain)
        {
            var dnsName = Regex.Replace(domain, WildcardRegex, string.Empty);
            dnsName = string.Format(DnsChallengeNameFormat, dnsName);

            return dnsName;
        }

        private async Task<DnsManagementClient> CreateDnsManagementClientAsync(AzureAuthenticationOptions options)
        {
            var sw = ValueStopwatch.StartNew();

            _logger.LogInformation("[Azure Management Client][Started]");

            var tokenProvider = new AzureServiceTokenProvider();
            var accessToken = await tokenProvider.GetAccessTokenAsync("https://management.azure.com/", options.SubscriptionId);

            var tokenCredentials = new TokenCredentials(accessToken);
            var azureCredentials = new AzureCredentials(tokenCredentials, tokenCredentials, options.TenantId, AzureEnvironment.AzureGlobalCloud);
            var restClient = RestClient
                .Configure()
                .WithEnvironment(AzureEnvironment.AzureGlobalCloud)
                .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                .WithCredentials(azureCredentials)
                .Build();

            var dnsClient = new DnsManagementClient(restClient)
            {
                SubscriptionId = options.SubscriptionId,
            };

            _logger.LogInformation("[Azure Management Client][Ended] Elapsed: {elapsed}sec", sw.GetElapsedTime().TotalSeconds);
            return dnsClient;
        }

        private async Task CheckDnsChallengeAsync(AcmeChallengeResponse challengeResult, string fullDomain)
        {
            var retries = 0;
            var delay = TimeSpan.FromSeconds(10);

            var txtRecords = new List<DnsClient.Protocol.TxtRecord>();

            while (true)
            {
                _logger.LogInformation("[Azure DNS][Update Check] Domain Name: {domainName}", challengeResult.Domain);

                var queryResult = await _lookupClient.QueryAsync(fullDomain, QueryType.TXT);
                var records = queryResult.Answers
                    .OfType<DnsClient.Protocol.TxtRecord>()
                    .ToArray();

                txtRecords.AddRange(records);

                if (txtRecords.Count > 0
                    && txtRecords.Any(x => x.Text.Contains(challengeResult.Token)))
                {
                    break;
                }

                if (retries > 10)
                {
                    break;
                }

                await Task.Delay(delay);

                retries++;
            }

            if (txtRecords.Count == 0)
            {
                throw new LetsEncryptException($"{challengeResult.Domain} did not resolve.");
            }

            if (!txtRecords.Any(x => x.Text.Contains(challengeResult.Token)))
            {
                throw new LetsEncryptException($"{challengeResult.Domain} value is not correct.");
            }
        }

        private IDictionary<string, string> ParseResourceId(string resourceId)
        {
            var values = resourceId.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            return new Dictionary<string, string>
            {
                { "subscriptions", values[1] },
                { "resourceGroups", values[3] },
                { "providers", values[5] }
            };
        }
    }
}
