using System;
using System.Linq;

using Bet.AspNetCore.LetsEncrypt;
using Bet.AspNetCore.LetsEncrypt.Internal;
using Bet.Extensions.LetsEncrypt.Certificates;
using Bet.Extensions.LetsEncrypt.Order.Stores;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class LetsEncryptBuilderExtensions
    {
        public static ILetsEncryptBuilder AddHttpChallengeResponse(this ILetsEncryptBuilder builder)
        {
            builder.Services.AddSingleton<HttpChallenge>();

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
               sp => new StartupCertificateLoader(builder.Name, sp),
               ServiceLifetime.Singleton));

            builder.Services
                   .AddSingleton<IStartupFilter, HttpChallengeStartupFilter>();

            builder.Services
                   .AddOptions<HttpChallengeResponseOptions>(string.Empty) // TODO: figure out how to tie it to builder.Name
                   .Configure<IServiceProvider>((options, sp) =>
                   {
                       options.ChallengeStore = sp.GetServices<IAcmeChallengeStore>().First(x => x is InMemoryChallengeStore);
                   });

            builder.Services.AddScheduler(x => x.AddJob<AcmeRenewalJob, AcmeRenewalJobOptions>("LetsEncrypt"));

            return builder;
        }
    }
}
