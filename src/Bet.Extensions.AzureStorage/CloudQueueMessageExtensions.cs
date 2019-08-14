using Microsoft.Azure.Storage.Queue;

using Newtonsoft.Json;

namespace Bet.Extensions.AzureStorage
{
    public static class CloudQueueMessageExtensions
    {
        /// <summary>
        /// Converts <see cref="CloudQueueMessage"/> to a generic type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <returns></returns>
        public static T Convert<T>(this CloudQueueMessage message)
        {
            return JsonConvert.DeserializeObject<T>(message.AsString);
        }

        /// <summary>
        /// Converts generic type to <see cref="CloudQueueMessage"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static CloudQueueMessage Convert<T>(this T data)
        {
            return new CloudQueueMessage(JsonConvert.SerializeObject(data));
        }
    }
}
