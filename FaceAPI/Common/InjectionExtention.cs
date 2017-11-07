using System;
using System.Collections.Generic;
using System.Dynamic;
using FaceAPI.Web.Common.Injection;
using Omu.ValueInjecter;

namespace FaceAPI.Web.Common
{
    public static class InjectionExtention
    {

        /// <summary>
        /// new 一個 T物件。將 obj 的屬性都塞進T後  返回T物件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T InstanceInjectObject<T>(this object obj) where T : class
        {
            if (obj == null)
                return default(T);

            T newInstance = Activator.CreateInstance<T>();
            return (T)StaticValueInjecter.InjectFrom(newInstance, obj);
        }




        public static object InjectFrom(this object obj, params object[] objects)
        {
            return StaticValueInjecter.InjectFrom(obj, objects);
        }

        public static object ToDictionaryToObject(this Dictionary<string, object> dic, Type t)
        {
            var obj = Activator.CreateInstance(t);
            obj.InjectFrom<InjectionForDictionaryToObject>(dic);
            return obj;
        }
        
        public static object InjectFrom<T>(this object target, params object[] source) where T : IValueInjection, new()
        {            
           return StaticValueInjecter.InjectFrom<T>(target,source);
        }
    }
}