using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
    public static class ByteExtensions
    {
        public static string ConvertToString(this byte[] bytes)
        {
            return bytes == null || bytes.Length == 0 ? string.Empty : Encoding.ASCII.GetString(bytes);
        }

        public static string ConvertToString(this byte[] bytes, int index, int count)
        {
            return bytes == null || bytes.Length == 0 ? string.Empty : Encoding.ASCII.GetString(bytes, index, count);
        }

        public static string ConvertToString(this IList<byte> lstBts)
        {
            return lstBts == null || lstBts.Count == 0 ? string.Empty : lstBts.ToArray().ConvertToString();
        }
    }
}
