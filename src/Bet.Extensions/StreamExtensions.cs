using System.Threading.Tasks;

namespace System.IO
{
    public static class StreamExtensions
    {
        /// <summary>
        /// Converts <see cref="Stream"/> into array of bytes.
        /// </summary>
        /// <param name="stream">The steam object.</param>
        /// <returns></returns>
        public static async Task<byte[]> ToByteArrayAsync(this Stream stream)
        {
            stream.Position = 0;
            var byteArray = new byte[stream.Length];
            await stream.ReadAsync(byteArray, 0, (int)stream.Length);
            return byteArray;
        }
    }
}
