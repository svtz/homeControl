using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace homeControl.ClientServerShared
{
    internal sealed class TypeSerializer
    {
        private static readonly IDictionary<string, Type> _typesCache =
            typeof(TypeSerializer)
                .GetTypeInfo()
                .Assembly
                .GetExportedTypes()
                .ToDictionary(t => t.FullName);

        public static string Serialize<T>()
        {
            return Serialize(typeof(T));
        }

        public static string Serialize(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return type.FullName;
        }

        public static Type Deserialize(string typeName)
        {
            if (typeName == null)
                throw new ArgumentNullException(nameof(typeName));

            Type type;
            if (!_typesCache.TryGetValue(typeName, out type))
            {
                type = _typesCache[typeName] = Type.GetType(typeName, true);
            }

            return type;
        }
    }
}