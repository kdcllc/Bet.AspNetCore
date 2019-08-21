using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.ML.ModelStorageProviders;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Bet.AspNetCore.UnitTest.MachineLearning
{
    public class StorageProvidersTests
    {
        [Fact]
        public async Task InMemoryModelStorageProvider_Succesful_Save_And_Load()
        {
            var services = new ServiceCollection();

            services.AddTransient<IModelStorageProvider, InMemoryModelStorageProvider>();

            var provider = services.BuildServiceProvider();

            var storage = provider.GetRequiredService<IModelStorageProvider>();

            using (var cts = new CancellationTokenSource())
            {
                var modelName = "testModel";
                var modelText = "This is test for the model memory stream";

                var stream = new MemoryStream();

                var modelBytes = Encoding.UTF8.GetBytes(modelText);
                stream.Write(modelBytes, 0, modelBytes.Length);
                stream.Position = 0;

                await storage.SaveModelAsync(modelName, stream, cts.Token);


                using (var savedStream = await storage.LoadModelAsync(modelName, cts.Token))
                {
                    using (var reader = new StreamReader(savedStream))
                    {
                        var text = reader.ReadToEnd();
                        Assert.Equal(modelText, text);
                    }
                }
            }
        }

        [Fact]
        public async Task InMemoryModelStorageProvider_Succesful_OnChange()
        {
            // arrange
            var services = new ServiceCollection();

            services.AddTransient<IModelStorageProvider, InMemoryModelStorageProvider>();

            var provider = services.BuildServiceProvider();

            var storage = provider.GetRequiredService<IModelStorageProvider>();

            using (var cts = new CancellationTokenSource())
            {
                var modelName = "testModel";
                var modelText = "This is test for the model memory stream";

                var changed = false;

                var changeToken = ChangeToken.OnChange(
                    () => storage.GetReloadToken(),
                    () => changed = true);

                // save
                using (var stream = new MemoryStream())
                {
                    var modelBytes = Encoding.UTF8.GetBytes(modelText);
                    stream.Write(modelBytes, 0, modelBytes.Length);
                    stream.Position = 0;

                    await storage.SaveModelAsync(modelName, stream, cts.Token);
                }

                Assert.True(changed);
            }
        }
    }
}
