using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.ML.DataLoaders;
using Bet.Extensions.ML.DataLoaders.ModelLoaders;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;

using Xunit;

namespace Bet.Extensions.UnitTest.ML
{
    public class StorageProvidersTests
    {
        [Fact]
        public async Task InMemoryStorage_Successful_Save_And_Load()
        {
            var services = new ServiceCollection();

            services.AddTransient<InMemoryStorage, InMemoryStorage>();

            var provider = services.BuildServiceProvider();

            var storage = provider.GetRequiredService<InMemoryStorage>();

            using var cts = new CancellationTokenSource();
            var modelName = "testModel";
            var modelText = "This is test for the model memory stream";

            using var stream = new MemoryStream();
            var modelBytes = Encoding.UTF8.GetBytes(modelText);
            stream.Write(modelBytes, 0, modelBytes.Length);
            stream.Position = 0;

            await storage.SaveAsync(modelName, stream, cts.Token);

            using var savedStream = await storage.LoadAsync(modelName, cts.Token);
            using var reader = new StreamReader(savedStream);
            var text = reader.ReadToEnd();
            Assert.Equal(modelText, text);
        }

        [Fact]
        public async Task InMemoryModelLoader_Successful_OnChange()
        {
            // arrange
            var services = new ServiceCollection();

            services.AddSingleton<InMemoryStorage, InMemoryStorage>();

            services.AddTransient<InMemoryModelLoader, InMemoryModelLoader>();

            var provider = services.BuildServiceProvider();

            var loader = provider.GetRequiredService<InMemoryModelLoader>();

            loader.Setup(new ModelLoderFileOptions
            {
                ModelName = "testModel"
            });

            using var cts = new CancellationTokenSource();
            var modelText = "This is test for the model memory stream";

            var changed = false;

            // register the change token
            var changeToken = ChangeToken.OnChange(
                () => loader.GetReloadToken(),
                () => changed = true);

            // save
            using (var stream = new MemoryStream())
            {
                var modelBytes = Encoding.UTF8.GetBytes(modelText);
                stream.Write(modelBytes, 0, modelBytes.Length);
                stream.Position = 0;

                await loader.SaveAsync(stream, cts.Token);
            }

            Assert.True(changed);
        }
    }
}
