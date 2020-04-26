using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

using Bet.Extensions.LetsEncrypt.Certificates;

using Microsoft.AspNetCore.Connections;

namespace Bet.AspNetCore.LetsEncrypt.Internal
{
    internal class KestrelCertificateSelector
    {
        private readonly ConcurrentDictionary<string, X509Certificate2>
            _store = new ConcurrentDictionary<string, X509Certificate2>(StringComparer.OrdinalIgnoreCase);

        private readonly ICertificateValidator _certificateValidator;
        private readonly string _named;

        public KestrelCertificateSelector(string named, ICertificateValidator certificateValidator)
        {
            _certificateValidator = certificateValidator ?? throw new ArgumentNullException(nameof(certificateValidator));
            _named = named;
        }

        public ICollection<string> SupportedDomains => _store.Keys;

        public string[] GetCertificatesAboutToExpire()
        {
            var certs = _store.ToArray();
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
                    mustRequest = !_certificateValidator.IsCertificateValid(_named, cert.Value);
                }

                if (mustRequest)
                {
                    result.Add(cert.Key);
                }
            }

            return result.ToArray();
        }

        public void Add(X509Certificate2 certificate)
        {
            var commonName = certificate.GetNameInfo(X509NameType.SimpleName, false);
            AddWithDomainName(commonName, certificate);

            foreach (var subjectAltName in X509CertificateHelpers.GetDnsFromExtensions(certificate))
            {
                AddWithDomainName(subjectAltName, certificate);
            }
        }

        public X509Certificate2? Select(ConnectionContext features, string hostName)
        {
            if (string.IsNullOrEmpty(hostName))
            {
                return null;
            }

            if (!_store.TryGetValue(hostName, out var retVal))
            {
                return null;
            }

            return retVal;
        }

        private void AddWithDomainName(string domainName, X509Certificate2 certificate)
        {
            _store.AddOrUpdate(domainName, certificate, (_, currentCert) =>
            {
                if (_certificateValidator.IsCertificateValid(_named, certificate))
                {
                    return certificate;
                }

                return currentCert;
            });
        }
    }
}
