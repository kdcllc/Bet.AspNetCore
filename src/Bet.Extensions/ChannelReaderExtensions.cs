using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace System.Threading.Channels
{
    public static class ChannelReaderExtensions
    {
        public static ChannelReader<T> Merge<T>(params ChannelReader<T>[] inputs)
        {
            var output = Channel.CreateUnbounded<T>();

            Task.Run(async () =>
            {
                async Task Redirect(ChannelReader<T> input)
                {
                    while (await input.WaitToReadAsync())
                    {
                        var item = await input.ReadAsync();

                        await output.Writer.WriteAsync(item);
                    }
                }

                await Task.WhenAll(inputs.Select(i => Redirect(i)).ToArray());
                output.Writer.Complete();
            });

            return output;
        }

        public static IList<ChannelReader<T>> Split<T>(ChannelReader<T> ch, int n)
        {
            var outputs = new Channel<T>[n];

            for (var i = 0; i < n; i++)
            {
                outputs[i] = Channel.CreateUnbounded<T>();
            }

            Task.Run(async () =>
            {
                var index = 0;
                while (await ch.WaitToReadAsync())
                {
                    var item = await ch.ReadAsync();
                    await outputs[index].Writer.WriteAsync(item);
                    index = (index + 1) % n;
                }

                foreach (var ch in outputs)
                {
                    ch.Writer.Complete();
                }
            });

            return outputs.Select(ch => ch.Reader).ToArray();
        }
    }
}
