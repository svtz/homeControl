using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace homeControl.ClientServerShared
{
    internal sealed class TypeSerializer
    {
        private static readonly IDictionary<string, Type> SharedAssemblyTypes =
            typeof(TypeSerializer)
                .GetTypeInfo()
                .Assembly
                .GetExportedTypes()
                .ToDictionary(t => t.FullName);


        public static string Serialize(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var typeKey = type.FullName;

            if (!SharedAssemblyTypes.ContainsKey(typeKey))
                throw new NotSupportedException("Only types from ClientServerShared assembly are supported");

            return typeKey;
        }

        public static Type Deserialize(string typeName)
        {
            if (typeName == null)
                throw new ArgumentNullException(nameof(typeName));

            Type type;
            if (!SharedAssemblyTypes.TryGetValue(typeName, out type))
                throw new ArgumentOutOfRangeException(nameof(typeName));

            return type;
        }
    }
}