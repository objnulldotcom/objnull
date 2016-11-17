using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace MVCWeb.Redis.Base
{
    public static class MyRedisDBFactory
    {
        private static ConnectionMultiplexer GetConnection()
        {
            ConfigurationOptions option = new ConfigurationOptions();
            option.EndPoints.Add("127.0.0.1:6399");
            option.Password = "@simple0";
            option.ConnectTimeout = 10 * 1000;
            option.SyncTimeout = 10 * 1000;
            return ConnectionMultiplexer.Connect(option);
        }

        public static IServer GetServer()
        {
            ConnectionMultiplexer connection = GetConnection();
            return connection.GetServer(connection.GetEndPoints()[0]);
        }

        public static IDatabase GetDB()
        {
            return GetConnection().GetDatabase();
        }
    }
}
