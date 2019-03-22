using System.IO;
using System.Runtime.Serialization.Json;

namespace Bet.Extensions
{
    public static class JsonExtensions
    {
        public static T Deserialize<T>(byte[] byteArray)
        {
            using (var memoryStream = new MemoryStream(byteArray))
            {
                var ser = new DataContractJsonSerializer(typeof(T));
                return (T)ser.ReadObject(memoryStream);
            }
        }
    }
}
