using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bet.AspNetCore.LetsEncrypt.Abstractions;
using Bet.Extensions.Hosting;
using Bet.Extensions.Hosting.Abstractions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bet.AspNetCore.LetsEncrypt.Internal
{
    /// <summary>
    /// This class provides with ability to check if the existing certificates in the specified storage are valid, if not
    /// the order request is issued.
    /// </summary>
    internal class CertificateRenewalService : TimedHostedService
    {
        private readonly ILetsEncryptService _letsEncryptService;

        public CertificateRenewalService(
            ILetsEncryptService letsEncryptService,
            IOptionsMonitor<TimedHostedServiceOptions> options,
            IEnumerable<ITimedHostedLifeCycleHook> lifeCycleHooks,
            ILogger<ITimedHostedService> logger) : base(options, lifeCycleHooks, logger)
        {
            TaskToExecuteAsync = (token) => RequestCertificate(token);
            _letsEncryptService = letsEncryptService ?? throw new System.ArgumentNullException(nameof(letsEncryptService));
        }


        public async Task RequestCertificate(CancellationToken cancellationToken)
        {
            await _letsEncryptService.AuthenticateWithNewAccount(cancellationToken);

            var cert = await _letsEncryptService.AcquireNewCertificateForHosts(cancellationToken);
        }
    }
}
