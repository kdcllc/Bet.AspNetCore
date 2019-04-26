using System;
using System.Text;
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

        /// <summary>
        /// Converts string to Base64
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToBase64String(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            var bytes = Encoding.UTF8.GetBytes(value);
            return Convert.ToBase64String(bytes);
        }
    }
}
