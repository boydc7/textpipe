using System;
using System.Collections.Generic;

namespace TextWrangler.Extensions
{
    public static class TypeExtensions
    {
        private static readonly Dictionary<string, Type> _typeNameMap = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        public static bool TryGetSystemType(this string typeName, out Type type)
        {
            try
            {
                type = GetSystemType(typeName);

                return true;
            }
            catch
            {
                type = null;

                return false;
            }
        }

        public static Type GetSystemType(this string typeName)
        {
            if (!_typeNameMap.ContainsKey(typeName))
            {
                _typeNameMap.Add(typeName, Type.GetType(typeName));
            }

            return _typeNameMap[typeName];
        }
    }
}
