using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace MVCWeb.Redis.Base
{
    public interface IMyRedisDB
    {
        IServer RedisServer { get; set; }
        IDatabase RedisDB { get; set; }

        void DelKey(string key);
        void StringSet(string key, string value);
        string StringGet(string key);
        void SetAdd<T>(string key, T obj);
        IEnumerable<T> GetSet<T>(string key);
        void SetRemove<T>(string key, T obj);
    }
    
    public class MyRedisDB : IMyRedisDB
    {
        public IServer RedisServer { get; set; }
        public IDatabase RedisDB { get; set; }

        public void DelKey(string key)
        {
            RedisDB.KeyDelete(key);
        }

        public void StringSet(string key, string value)
        {
            RedisDB.StringSet(key, value);
        }

        public string StringGet(string key)
        {
            return RedisDB.StringGet(key);
        }

        public void SetAdd<T>(string key, T obj)
        {
            RedisDB.SetAdd(key, JsonConvert.SerializeObject(obj));
        }

        public IEnumerable<T> GetSet<T>(string key)
        {
            foreach (string obj in RedisDB.SetMembers(key))
            {
                yield return JsonConvert.DeserializeObject<T>(obj);
            }
        }

        public void SetRemove<T>(string key, T obj)
        {
            RedisDB.SetRemove(key, JsonConvert.SerializeObject(obj));
        }
    }
}
