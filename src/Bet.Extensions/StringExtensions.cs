using System.Text.RegularExpressions;

namespace Bet.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Removes anything besides [^A-Za-z] this regular expression.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string KeepAllLetters(this string input)
        {
            return Regex.Replace(input, "[^A-Za-z]", "");
        }
    }
}
