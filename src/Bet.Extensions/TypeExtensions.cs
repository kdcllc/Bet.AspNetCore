using System;
using System.Linq;

namespace System
{
    public static class TypeExtensions
    {
        public static bool IsSimpleType(this Type type)
        {
            return
        type.IsPrimitive
        || new Type[]
        {
            typeof(Enum),
            typeof(string),
            typeof(decimal),
            typeof(DateTime),
            typeof(DateTimeOffset),
            typeof(TimeSpan),
            typeof(Guid)
        }.Contains(type)
        || Convert.GetTypeCode(type) != TypeCode.Object
        || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) && IsSimpleType(type.GetGenericArguments()[0]));
        }
    }
}
