using System;
using System.Collections.Generic;
using System.Linq;

using Bet.AspNetCore.LetsEncrypt.Internal;
using Bet.Extensions.LetsEncrypt.Account;
using Bet.Extensions.LetsEncrypt.Certificates;
using Bet.Extensions.LetsEncrypt.Certificates.Stores;
using Bet.Extensions.LetsEncrypt.Order.Stores;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class LetsEncryptBuilderExtensions
    {
        public static ILetsEncryptBuilder AddHttpChallengeResponse(this ILetsEncryptBuilder builder)
        {
            builder.Services
                    .AddOptions<KestrelServerOptions>()
                    .Configure<KestrelCertificateSelector>((options, certificateSelector) =>
                    {
                        options.ConfigureHttpsDefaults(o => o.ServerCertificateSelector = certificateSelector.Select);
                    });

            builder.Services
                   .AddSingleton<DevelopmentCertificate>()
                   .AddSingleton(sp =>
                   {
                       var validator = sp.GetRequiredService<ICertificateValidator>();
                       return new KestrelCertificateSelector(builder.Name, validator);
                   });

            builder.Services.Add(ServiceDescriptor.Describe(
               typeof(IHostedService),
               sp =>
               {
                   var accountOptions = sp.GetRequiredService<IOptionsMonitor<AcmeAccountOptions>>().Get(builder.Name);
                   var certificateOptions = sp.GetRequiredService<IOptionsMonitor<CertificateOptions>>().Get(builder.Name);

                   var developmentCertificate = sp.GetRequiredService<DevelopmentCertificate>();
                   var stores = sp.GetRequiredService<IEnumerable<ICertificateStore>>();
                   var certificateSelector = sp.GetRequiredService<KestrelCertificateSelector>();

                   return new StartupCertificateLoader(
                       Options.Options.Create(accountOptions),
                       Options.Options.Create(certificateOptions),
                       developmentCertificate,
                       stores,
                       certificateSelector);
               },
               ServiceLifetime.Singleton));

            builder.Services
                   .AddSingleton<IStartupFilter, HttpChallengeStartupFilter>();
                   //.AddTransient<HttpChallengeResponseMiddleware>();

            builder.Services
                   .AddOptions<HttpChallengeResponseOptions>(string.Empty)
                   .Configure<IServiceProvider>((options, sp) =>
                   {
                       options.ChallengeStore = sp.GetServices<IAcmeChallengeStore>().First(x => x is InMemoryChallengeStore);
                   });

            return builder;
        }
    }
}
