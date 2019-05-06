using Bet.Extensions.AzureStorage;
using Bet.Extensions.AzureStorage.Options;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Builder
{
    public static class AspNetCoreBuilderExtensions
    {
        /// <summary>
        /// Uses Azure Storage Blob Container for serving `Static File`.
        /// </summary>
        /// <typeparam name="TOptions">The type of the configuration object to be used to register the Azure Storage Blob container.</typeparam>
        /// <param name="app">The <see cref="IApplicationBuilder"/> instance.</param>
        /// <returns></returns>
        public static IApplicationBuilder UseAzureStorageForStaticFiles<TOptions>(
            this IApplicationBuilder app)
            where TOptions : StorageFileProviderOptions
        {
            var storageOptions = app.ApplicationServices.GetRequiredService<IOptionsMonitor<StorageAccountOptions>>();
            var options = app.ApplicationServices.GetRequiredService<IOptionsMonitor<TOptions>>().CurrentValue;

            var azureFileProvider = new StorageFileProvider(
                storageOptions.Get(options.AzureStorageConfiguration),
                options.ContainerName);

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
