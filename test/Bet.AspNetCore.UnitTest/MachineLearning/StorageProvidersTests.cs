using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.ML.ModelBuilder;

using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace Bet.AspNetCore.UnitTest.MachineLearning
{
    public class StorageProvidersTests
    {
        [Fact]
        public async Task Succesful_Save_And_Load()
        {
            var services = new ServiceCollection();

            services.AddTransient<IModelStorageProvider, InMemoryModelStorageProvider>();

            var provider = services.BuildServiceProvider();

            var storage = provider.GetRequiredService<IModelStorageProvider>();

            using (var cts = new CancellationTokenSource())
            {
                var modelName = "testModel";
                var modelText = "This is test for the model memory stream";

                using (var stream = new MemoryStream())
                {
                    var modelBytes = Encoding.UTF8.GetBytes(modelText);
                    stream.Write(modelBytes, 0, modelBytes.Length);
                    stream.Position = 0;

                    await storage.SaveModelAsync(modelName, stream, cts.Token);
                }

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
    }
}
