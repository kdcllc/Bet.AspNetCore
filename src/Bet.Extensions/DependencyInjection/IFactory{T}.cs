namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// The Factory class to create TService type.
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    public interface IFactory<TService> where TService : class
    {
        /// <summary>
        /// To Create TService instance.
        /// </summary>
        /// <returns></returns>
        TService Create();
    }
}
