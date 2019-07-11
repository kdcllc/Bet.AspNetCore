using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

using Bet.AspNetCore.LetsEncrypt.Options;

using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Options;

namespace Bet.AspNetCore.LetsEncrypt.Internal
{
    internal class CertificateSelector
    {
        private readonly ConcurrentDictionary<string, X509Certificate2> _certs = new ConcurrentDictionary<string, X509Certificate2>(StringComparer.OrdinalIgnoreCase);

        private LetsEncryptOptions _options;

        public CertificateSelector(IOptionsMonitor<LetsEncryptOptions> options)
        {
            _options = options.CurrentValue;
            options.OnChange(newOptions => _options = newOptions);
        }

        public string[] GetCertificatesAboutToExpire()
        {
            var certs = _certs.ToArray();
            var result = new List<string>();

            foreach (var cert in certs)
            {
                bool mustRequest;
                if (cert.Value == null)
                {
                    mustRequest = true;
                }
                else
                {
                    mustRequest = DateTime.UtcNow.AddDays(_options.DaysBefore) > cert.Value.NotAfter;
                }

                if (mustRequest)
                {
                    result.Add(cert.Key);
                }
            }

            return result.ToArray();
        }

        public void Use(string hostName, X509Certificate2 certificate)
        {
            _certs.AddOrUpdate(hostName, certificate, (_, __) => certificate);
        }

        public X509Certificate2 Select(ConnectionContext features, string hostName)
        {
            if (!_certs.TryGetValue(hostName, out var retVal))
            {
                return null;
            }

            return retVal;
        }
    }
}
