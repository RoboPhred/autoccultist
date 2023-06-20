namespace AutoccultistNS
{
    using System;

    public static class TypeExtensions
    {
        public static object Cast(Type type, object obj)
        {
            // This way might be better, or it might be far worse...
            // https://github.com/dotnet/runtime/issues/19683
            var objType = obj.GetType();

            if (type.IsAssignableFrom(objType))
            {
                return obj;
            }

            var implicitConversion = type.GetMethod("op_Explicit", new[] { objType }) ?? type.GetMethod("op_Implicit", new[] { objType });
            if (implicitConversion != null)
            {
                return implicitConversion.Invoke(null, new[] { obj });
            }

            return Convert.ChangeType(obj, type);
        }
    }
}
