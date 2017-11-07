using System;
using System.Dynamic;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using FaceAPI.Web.Common.Injection;

namespace FaceAPI.Web.Common
{
    public static class ExtensionMethods
    {

        /// <summary>
        /// [擴充方法] 判斷List是否為null或Count == 0
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsEmptyOrNull<T>(this IEnumerable<T> source)
        {
            if (source == null)
                return true; // or throw an exception

            if (source.Any())
                return false;

            return true;
        }



        /// <summary>
        /// [擴充方法] 判斷List是否不為空 (不為null ＆＆　筆數大於０)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsNotEmpty<T>(this IEnumerable<T> source)
        {
            if (source == null)
                return false; // or throw an exception

            return source.Any();
        }








        public static string ToGetJsonString(this object obj)
        {
            if (obj == null)
                return null;

            return JsonConvert.SerializeObject(obj);
        }

        public static string ToGetJsonString(this object obj, ReferenceLoopHandling loopHanding)
        {
            if (obj == null)
                return null;

            return JsonConvert.SerializeObject(obj, new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = loopHanding
                });
        }

        /// <summary>
        /// stream to byte array
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static byte[] ToGetByteArray(this Stream stream)
        {
            var bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            stream.Seek(0, SeekOrigin.Begin);
            return bytes;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strChinese"></param>
        /// <returns></returns>
        public static bool IsChinese(this string strChinese)
        {
            var bresult = true;
            var dRange = 0;
            var dstringmax = Convert.ToInt32("9fff", 16);
            var dstringmin = Convert.ToInt32("4e00", 16);

            for (var i = 0; i < strChinese.Length; i++)
            {
                dRange = Convert.ToInt32(Convert.ToChar(strChinese.Substring(i, 1)));
                if (dRange >= dstringmin && dRange < dstringmax)
                {
                    bresult = true;
                    break;
                }
                else
                {
                    bresult = false;
                }
            }

            return bresult;
        }

        public static ExpandoObject ToExpando(this object obj)
        {
            var ed = new ExpandoObject();
            ed.InjectFrom<InjectionForObjectToExpando>(obj);
            return ed;
        }

        /// <summary>
        /// 如果 property 為 null，property 就刪除
        /// </summary>
        public static ExpandoObject ToExpandoAPI(this object obj)
        {
            var ed = new ExpandoObject();
            ed.InjectFrom<InjectionForObjectToExpandoAPI>(obj);
            return ed;
        }

        public static bool IsNumber(this string numberString)
        {
            var number = 0;

            return Int32.TryParse(numberString, out number);
        }

        public static string ToRemovePointZero<T>(this T obj)
        {
            if (typeof(T) == typeof(double))
                return obj.ToGetValueOrDefault(0d).ToString("0.########");
            if (typeof(T) == typeof(decimal))
                return obj.ToGetValueOrDefault(0m).ToString("0.########");

            throw new System.Exception("use decimal and double type only.");
        }

        /// <summary>
        ///  截掉指定的字數 在後面補上 "..."
        /// </summary>
        /// <param name="str"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string ToGetShortString(this string str, int subStrlength)
        {
            if (string.IsNullOrEmpty(str) || str.Length <= subStrlength)
            {
                return str;
            }

            var result = str.Substring(0, subStrlength - 3) + "...";

            return result;
        }


        /// <summary>
        /// 截掉指定的字數 在後面補上自訂的符號
        /// </summary>
        /// <param name="s"></param>
        /// <param name="length"></param>
        /// <param name="truncation"></param>
        /// <returns></returns>
        public static string ToGetShortString(this string str, int length, string truncation = "...")
        {
            if (string.IsNullOrEmpty(str))            
                return str;

            return str.Length > length ?
                str.Substring(0, length - truncation.Length) + truncation : str;
        }

    }
}