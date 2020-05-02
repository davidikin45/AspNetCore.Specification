using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AspNetCore.Specification
{
    internal static class TypeExtensions
    {
        public static bool IsCollection(this Type type)
        {
            return type.GetInterfaces().Where(x => x.GetTypeInfo().IsGenericType).Any(x => x.GetGenericTypeDefinition() == typeof(ICollection<>) && !x.GetGenericArguments().Contains(typeof(Byte)));
        }

        public static Type[] GetGenericArguments(this Type type, string propName)
        {
            if (HasProperty(type, propName))
            {
                return type.GetProperties().First(p => p.Name.ToUpper() == propName.ToUpper()).PropertyType.GenericTypeArguments;
            }
            return null;
        }

        public static bool HasProperty(this Type type, string propName)
        {
            return type.GetProperties().Any(p => p.Name.ToUpper() == propName.ToUpper());
        }

    }
}
