using System;

namespace TextWrangler.Extensions
{
    public static class TypeExtensions
    {
        public static bool TryGetSystemType(this string typeName, out Type type)
        {
            try
            {
                type = Type.GetType(typeName);

                return true;
            }
            catch
            {
                type = null;

                return false;
            }
        }
    }
}
