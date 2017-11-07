using System;
using System.ComponentModel;

namespace FaceAPI.Web.Common
{
    public static class ValueExtension
    {
        /// <summary>
        /// To the get value or default.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static object ToGetValueOrDefault(this object obj, Type type)
        {
            var defaultValue = type.IsValueType ? Activator.CreateInstance(type) : null;
            try
            {
                type = Nullable.GetUnderlyingType(type) ?? type;
                return Convert.ChangeType(obj, type);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// To the get value or default.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">The obj.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static T ToGetValueOrDefault<T>(this object obj, T defaultValue)
        {
            Type t = typeof(T);

            if (obj == null)
                return defaultValue;

            if (t == typeof (string))
            {
                try
                {
                    return (T)Convert.ChangeType(obj.ToString(), typeof(T));
                }
                catch
                {
                    return defaultValue;
                }
            }

            //convert string to enum
            if (t.BaseType == typeof(System.Enum))
            {
                try
                {
                    return (T)System.Enum.Parse(t, obj.ToString(), true);
                }
                catch
                {
                    return defaultValue;
                }
            }

            //convert the datetime format of 20100101/2010-01-01
            if (t == typeof(DateTime))
            {
                string oStr = obj.ToGetValueOrDefault("");
                if (oStr.Length == 8)
                {
                    obj = oStr.Insert(6, "/").Insert(4, "/");
                }
                if (oStr.Length == 10)
                {
                    obj = oStr.Replace("-", "/");
                }
            }

            if (t == typeof(Guid))
            {
                try
                {
                    return (T)TypeDescriptor.GetConverter(typeof(T))
                                .ConvertFromInvariantString(obj.ToString());
                }
                catch
                {
                    return defaultValue;
                }
            }

            //return the default value
            return obj.ToConvert(defaultValue);
        }

        private static T ToConvert<T>(this object obj, T defaultValue)
        {
            try
            {
                return (T)Convert.ChangeType(obj, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}