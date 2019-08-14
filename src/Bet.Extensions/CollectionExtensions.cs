using System.Linq;
using System.Text;

namespace System.Collections.Generic
{
    public static class CollectionExtensions
    {
        /// <summary>
        /// Enables to process <see cref="IEnumerable{T}"/> as batch.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int size)
        {
            using (var iter = source.GetEnumerator())
            {
                while (iter.MoveNext())
                {
                    var chunk = new T[size];
                    chunk[0] = iter.Current;
                    for (var i = 1; i < size && iter.MoveNext(); i++)
                    {
                        chunk[i] = iter.Current;
                    }

                    yield return chunk;
                }
            }
        }

        /// <summary>
        /// Takes <see cref="IEnumerable{T}"/> and returns a string with specified separator.
        /// </summary>
        /// <param name="strings"></param>
        /// <param name="seperator"></param>
        /// <param name="head"></param>
        /// <param name="tail"></param>
        /// <returns></returns>
        public static string Flatten(this IEnumerable<string> strings, string seperator, string head = "", string tail = "")
        {
            // If the collection is null, or if it contains zero elements,
            // then return an empty string.
            if (strings?.Any() != true)
            {
                return string.Empty;
            }

            // Build the flattened string
            var flattenedString = new StringBuilder();

            flattenedString.Append(head);
            foreach (var s in strings)
            {
                flattenedString.AppendFormat("{0}{1}", s, seperator); // Add each element with the given separator.
            }

            flattenedString.Remove(flattenedString.Length - seperator.Length, seperator.Length); // Remove the last separator
            flattenedString.Append(tail);

            // Return the flattened string
            return flattenedString.ToString();
        }
    }
}
