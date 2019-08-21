namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// The Factory Class that provides instance based on the Key.
    /// </summary>
    /// <typeparam name="TKey">The type of the key to be used within the Create method.</typeparam>
    /// <typeparam name="TService">The interface type to be used to create an instance for.</typeparam>
    public interface IKeyFactory<TKey, TService> where TService : class
    {
        /// <summary>
        /// To Create an instance of the TService.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        TService Create(TKey key);
    }
}
