namespace Bet.Extensions.Options
{
    public interface IOptionsFormatter
    {
        /// <summary>
        /// Creates a string to be logged when the options are created from the IOptionsFactory.
        /// The returned value should be a JSON string with all secrets removed.
        /// </summary>
        /// <returns>A JSON string representing the options, with all secrets removed.</returns>
        string Format();
    }
}
