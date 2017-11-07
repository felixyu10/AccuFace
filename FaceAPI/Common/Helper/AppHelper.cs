using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json;
using FaceAPI.Web.Common;

namespace FaceAPI.Web.Common.Helper
{


    public static class AppHelper
    {
        /// <summary>
        /// DebugMode
        /// </summary>
        public static bool IsDebugMode
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// TestMode
        /// </summary>
        public static bool IsTestMode
        {
            get
            {
                var request = HttpContext.Current;
                if (request.Request.Url.Host.Contains("test"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Get WebConfig Setting
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetWebConfigSetting(string key)
        {
            return ConfigurationManager.AppSettings.Get(key);
        }

        /// <summary>
        /// Gets the deserialized json object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonStr">The json STR.</param>
        /// <returns></returns>
        public static T GetDeserializedJsonToObject<T>(string jsonStr)
        {
            if (string.IsNullOrEmpty(jsonStr))
                return default(T);

            if (typeof(T) == typeof(String))
            {
                try
                {
                    return jsonStr.ToGetValueOrDefault(default(T));
                }
                catch
                {
                    return default(T);
                }
            }

            try
            {
                return JsonConvert.DeserializeObject<T>(jsonStr);
            }
            catch
            {
                return default(T);
            }
        }

        public static T GetDeserializedDynamicToObject<T>(object dObj)
        {
            var json = dObj.ToGetJsonString(ReferenceLoopHandling.Ignore);
            return GetDeserializedJsonToObject<T>(json);
        }

        /// <summary>
        /// Gets the deserialized json automatic dynamic.
        /// </summary>
        /// <param name="jsonStr">The json string.</param>
        /// <returns></returns>
        public static dynamic GetDeserializedJsonToDynamic(string jsonStr)
        {
            return JsonConvert.DeserializeObject<ExpandoObject>(jsonStr);
        }

        public static string GetSerializeObjectToJson(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        /// <summary>
        /// Encrypts the sh a256.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="enc">The enc.</param>
        /// <returns></returns>
        public static string EncryptSHA256(string text, Encoding enc)
        {
            var buffer = enc.GetBytes(text);
            var ha = new SHA256CryptoServiceProvider();
            return BitConverter.ToString(ha.ComputeHash(buffer)).Replace("-", "");
        }

        public static Dictionary<String, String> ParseXml(XmlDocument xmlDoc)
        {
            return xmlDoc.SelectNodes("//*")
                         .Cast<XmlNode>()
                         .ToDictionary<XmlNode, String, String>
                           (xmlNodeTemp => xmlNodeTemp.Name,
                            xmlNodeTemp => xmlNodeTemp.InnerText);
        }

    }
}
