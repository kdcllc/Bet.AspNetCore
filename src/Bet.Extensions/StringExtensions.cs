using System.Text;
using System.Text.RegularExpressions;

namespace System
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
            return Regex.Replace(input, "[^A-Za-z]", string.Empty);
        }

        /// <summary>
        /// Converts string to Base64.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToBase64String(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var bytes = Encoding.UTF8.GetBytes(value);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Converts Base64 string back to string.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="encoder"></param>
        /// <returns></returns>
        public static string FromBase64String(this string value, Encoding? encoder = null)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var bytes = Convert.FromBase64String(value);

            if (encoder != null)
            {
                return encoder.GetString(bytes);
            }

            return Encoding.ASCII.GetString(bytes);
        }

        /// <summary>
        /// Converts string to bytes.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static byte[] ToBytes(this string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return Array.Empty<byte>();
            }

            return Encoding.ASCII.GetBytes(s);
        }
    }
}
