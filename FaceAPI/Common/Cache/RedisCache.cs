using System;
using System.Collections.Generic;
using System.Linq;
using FaceAPI.Web.Common.Enum;
using StackExchange.Redis;
using FaceAPI.Web.Common.Helper;

namespace FaceAPI.Web.Common.Cache
{
    public class RedisCache
    {
        //預設10分鐘
        private static readonly int liveTime = 10;
        private static class RedisCacheHolder
        {
            // internal static readonly RedisCache Instance = new RedisCache();
            internal static readonly Dictionary<int, RedisCache> DicInstance = new Dictionary<int, RedisCache>();

            static RedisCacheHolder()
            {
                for (int i = 0; i <= 15; i++)
                {
                    DicInstance.Add(i, new RedisCache(i));
                }
            }
        }
        private static Dictionary<int, RedisCache> DicInstance { get { return RedisCacheHolder.DicInstance; } }

        #region Public static function
        public static void SetCache(string key, object obj, CacheInSideBlockEnum block, TimeSpan? timeSpan = null)
        {
            if (timeSpan == null)
                timeSpan = new TimeSpan(0, liveTime, 0);
            GetRedisDB(block).SetCacheValue(key, obj, timeSpan.Value);
        }
        public static void SetCache(CacheNameEnum key, List<KeyValuePair<string, object>> keyValues, TimeSpan? timeSpan = null)
        {
            var block = GetCacheBlockEnum(key);
            if (timeSpan == null)
                timeSpan = new TimeSpan(0, liveTime, 0);
            GetRedisDB(block).SetCacheValue(keyValues, timeSpan.Value);
        }
        public static void SetCache(CacheNameEnum key, string identity, object obj, TimeSpan? timeSpan = null)
        {
            var block = GetCacheBlockEnum(key);
            var k = GetCacheKey(key, identity);
            if (timeSpan == null)
                timeSpan = new TimeSpan(0, liveTime, 0);
            GetRedisDB(block).SetCacheValue(k, obj, timeSpan.Value);
        }
        public static KeyValuePair<string, object> MakeCacheKeyValue(CacheNameEnum key, string identity, object obj)
        {
            var k = GetCacheKey(key, identity);
            KeyValuePair<string, object> keyValue = new KeyValuePair<string, object>(k, obj);

            return keyValue;
        }

        public static T GetCache<T>(CacheNameEnum key, string identity, TimeSpan? timeSpan = null, Func<T> func = null)
        {
            var k = GetCacheKey(key, identity);
            var block = GetCacheBlockEnum(key);
            return GetCache<T>(k, block, timeSpan, func);
        }
        public static T GetCache<T>(string key, CacheInSideBlockEnum block, TimeSpan? timeSpan = null, Func<T> func = null)
        {
            var redisDB = GetRedisDB(block);
            var obj = redisDB.GetCacheValue<T>(key);

            if (obj != null)
                return obj;

            if (func != null)
            {
                obj = func();
                if (timeSpan == null)
                    timeSpan = new TimeSpan(0, liveTime, 0);
                redisDB.SetCacheValue(key, obj, timeSpan.Value);
            }

            return obj;
        }
        public static IList<T> GetCache<T>(CacheNameEnum key, List<string> identitys)
        {
            var block = GetCacheBlockEnum(key);
            List<string> identityList = new List<string>();
            foreach (var identity in identitys)
            {
                var k = GetCacheKey(key, identity);
                identityList.Add(k);
            }
            string[] keys = identityList.ToArray();
            return GetRedisDB(block).GetCacheValue<T>(keys);

        }
        public static void RemoveCache(string key, CacheInSideBlockEnum block)
        {
            GetRedisDB(block).RemoveCacheValue(key);
        }
        public static void RemoveCache(CacheNameEnum key, string identity)
        {
            var block = GetCacheBlockEnum(key);
            var k = GetCacheKey(key, identity);
            GetRedisDB(block).RemoveCacheValue(k);
        }
        /// <summary>
        /// 移除 包含指定關鍵字 cache
        /// </summary>
        /// <param name="keyword"></param>
        public static void RemoveCacheByContainKey(CacheNameEnum key, string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
                return;
            var block = GetCacheBlockEnum(key);
            GetRedisDB(block).RemoveCacheByContainKey(keyword);
        }
        public static void RemoveCacheByContainKey(CacheInSideBlockEnum block, string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
                return;

            GetRedisDB(block).RemoveCacheByContainKey(keyword);
        }
        public static CacheInSideBlockEnum GetCacheBlockEnum(CacheNameEnum key)
        {
            var keyName = System.Enum.GetName(typeof(CacheNameEnum), key);
            var keyBlock = (int)System.Enum.Parse(typeof(CacheNameBlockMappingEnum), keyName);
            var block = CacheInSideBlockEnum.Other;
            System.Enum.TryParse<CacheInSideBlockEnum>(keyBlock.ToString(), out block);
            return block;
        }
        public static string GetCacheKey(CacheNameEnum key, string identity)
        {
            return
                string.Format("{0}_{1}", key, identity)
                    .Replace(@":", "")
                    .Replace("{", "")
                    .Replace("}", "")
                    .Replace(",", "")
                    .Replace(@"""", "");
        }
        /// <summary>
        /// String Increment(long)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static long StringIncrement(CacheInSideBlockEnum block, string key, long value)
        {
            return GetRedisDB(block).StringIncrement(key, value);
        }
        /// <summary>
        /// String Decrement(long)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static long StringDecrement(CacheInSideBlockEnum block, string key, long value)
        {
            return GetRedisDB(block).StringDecrement(key, value);
        }
        public static bool LockTake(CacheInSideBlockEnum block, string key, string value, TimeSpan timeSpan)
        {
            return GetRedisDB(block).LockTake(key, value, timeSpan);
        }

        public static bool LockRelease(CacheInSideBlockEnum block, string key, string value)
        {
            return GetRedisDB(block).LockRelease(key, value);
        }
        public static bool SetKeyExpire(CacheInSideBlockEnum block, string key, TimeSpan timeSpan)
        {
            return GetRedisDB(block).SetKeyExpire(key, timeSpan);
        }
        #endregion

        #region 核心
        private static RedisCache GetRedisDB(CacheInSideBlockEnum block)
        {
            var blockValue = (int)block;
            var redisCache = DicInstance[blockValue];
            return redisCache;
        }
        //private ConnectionMultiplexer connection;
        private IDatabase cache;
        private IServer server;
        private CacheInSideBlockEnum cacheBlock;
        private RedisCache(int block)
        {
            Initialize(block);
        }


        private static Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        {
            string connectionStr = string.Empty;

            //if (AppHelper.IsDebugMode)
            //    connectionStr = AppHelper.GetMySetting("cache").Element("redisCacheConnectionTest").Value;
            //else
                connectionStr = "rcAccuCacheTest.redis.cache.windows.net,abortConnect=false,ssl=false,password=G+5meNwBusqRsvX1SD0kA/Ncy9DabETc693poki+twk=,syncTimeout=2000";

            return ConnectionMultiplexer.Connect(connectionStr);
        });

        public static ConnectionMultiplexer connection
        {
            get
            {
                return lazyConnection.Value;
            }
        }


        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="block"></param>
        /// <param name="i"></param>
        private void Initialize(int block, int i = 0)
        {
            try
            {
                cache = connection.GetDatabase(block);
                var endPoint = connection.GetEndPoints().First();
                server = connection.GetServer(endPoint);
                this.cacheBlock = (CacheInSideBlockEnum)block;
            }
            catch (System.Exception ex)
            {
                //連線失敗Retry 10次, 間隔10ms
                if (i >= 10)
                {
                    throw ex;
                }
                i++;
                System.Threading.Thread.Sleep(10);
                Initialize(block, i);
            }
        }
        private T GetCacheValue<T>(string key)
        {
            try
            {
                var value = cache.StringGet(key);
                return AppHelper.GetDeserializedJsonToObject<T>(value.ToString());
            }
            catch
            {
                return default(T);
            }
        }
        private RedisKey[] GetRedisKeysByStringArray(string[] keys)
        {
            var redisKeys = keys.Select(key => (RedisKey)key).ToArray();
            return redisKeys;
        }
        private List<T> GetCacheValue<T>(string[] keys)
        {
            try
            {
                RedisKey[] redisKeys = GetRedisKeysByStringArray(keys);
                var values = cache.StringGet(redisKeys);
                List<T> valueList = values.Select(value => AppHelper.GetDeserializedJsonToObject<T>(value.ToString())).ToList();
                return valueList;
            }
            catch
            {
                return default(List<T>);
            }
        }
        /// <summary>
        /// Sets the cache value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="obj">The object.</param>
        private void SetCacheValue(string key, object obj)
        {
            var value = obj.ToGetJsonString(Newtonsoft.Json.ReferenceLoopHandling.Ignore);
            cache.StringSet(key, value);
        }
        private void SetCacheValue(string key, object obj, TimeSpan timeSpan)
        {
            var value = obj.ToGetJsonString(Newtonsoft.Json.ReferenceLoopHandling.Ignore);
            cache.StringSet(key, value, timeSpan);
        }
        private void SetCacheValue(List<KeyValuePair<string, object>> keyValues, TimeSpan timeSpan)
        {
            var values = keyValues.Select(keyValue => new KeyValuePair<RedisKey, RedisValue>(keyValue.Key, keyValue.Value.ToGetJsonString(Newtonsoft.Json.ReferenceLoopHandling.Ignore))).ToArray();
            cache.StringSet(values);
            foreach (var keyValue in keyValues)
            {
                cache.KeyExpire(keyValue.Key, timeSpan);
            }
        }
        private void RemoveCacheValue(string key)
        {
            cache.KeyDelete(key);
        }
        private void RemoveCacheValues(List<string> keys)
        {

            cache.KeyDelete(keys.Select(k => (RedisKey)k).ToArray());
        }
        private void RemoveCacheByContainKey(string keyword)
        {
            string pattern = string.Format("*{0}*", keyword);
            List<string> removeKeys = server.Keys((int)cacheBlock, pattern: pattern).Select(k => k.ToString()).ToList();
            this.RemoveCacheValues(removeKeys);
        }
        /// <summary>
        /// String Increment(long)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private long StringIncrement(string key, long value)
        {
            return cache.StringIncrement(key, value);
        }
        /// <summary>
        /// String Decrement(long)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private long StringDecrement(string key, long value)
        {
            return cache.StringDecrement(key, value);
        }
        private bool LockTake(string key, string value, TimeSpan timeSpan)
        {
            var result = cache.LockTake(key, value, timeSpan);
            return result;
        }

        private bool LockRelease(string key, string value)
        {
            var result = cache.LockRelease(key, value);
            return result;
        }
        /// <summary>
        /// Set TTL
        /// </summary>
        /// <param name="key"></param>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        private bool SetKeyExpire(string key, TimeSpan timeSpan)
        {
            return cache.KeyExpire(key, timeSpan);
        }
        #endregion
    }
}