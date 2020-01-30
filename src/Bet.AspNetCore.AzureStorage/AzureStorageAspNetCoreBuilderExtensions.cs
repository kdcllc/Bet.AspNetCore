using Bet.Extensions.AzureStorage;
using Bet.Extensions.AzureStorage.Options;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Builder
{
    public static class AzureStorageAspNetCoreBuilderExtensions
    {
        /// <summary>
        /// Uses Azure Storage Blob container for service "Static Files".
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> instance.</param>
        /// <param name="namedContainer">The name of the named container registry. The same type can have several named options.</param>
        /// <returns></returns>
        public static IApplicationBuilder UseAzureStorageForStaticFiles(this IApplicationBuilder app, string namedContainer = "")
        {
            return app.UseAzureStorageForStaticFiles<StorageFileProviderOptions>(namedContainer);
        }

        /// <summary>
        /// Uses Azure Storage Blob Container for serving `Static File`.
        /// </summary>
        /// <typeparam name="TOptions">The type of the configuration object to be used to register the Azure Storage Blob container.</typeparam>
        /// <param name="app">The <see cref="IApplicationBuilder"/> instance.</param>
        /// <param name="namedContainer">The name of the named container registry. The same type can have several named options.</param>
        /// <returns></returns>
        public static IApplicationBuilder UseAzureStorageForStaticFiles<TOptions>(
            this IApplicationBuilder app,
            string namedContainer = "")
            where TOptions : StorageFileProviderOptions
        {
            var sp = app.ApplicationServices;

            var options = sp.GetRequiredService<IOptionsMonitor<TOptions>>().Get(namedContainer);
            var storageOptions = sp.GetRequiredService<IOptionsMonitor<StorageAccountOptions>>()
                .Get(options.AccountName);

            var azureFileProvider = new StorageFileProvider(storageOptions, options.ContainerName);

            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = azureFileProvider,
                RequestPath = options.RequestPath
            });

            if (options.EnableDirectoryBrowsing)
            {
                app.UseDirectoryBrowser(new DirectoryBrowserOptions()
                {
                    FileProvider = azureFileProvider,
                    RequestPath = options.RequestPath
                });
            }

            return app;
        }
    }
}
