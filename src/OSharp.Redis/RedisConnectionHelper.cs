using System.Collections.Generic;
using System.Threading;
using StackExchange.Redis;

namespace OSharp.Redis
{
    /// <summary>
    /// Redis连接帮助类
    /// </summary>
    public class RedisConnectionHelper
    {
        private static readonly IDictionary<string, ConnectionMultiplexer> ConnectionCache = new Dictionary<string, ConnectionMultiplexer>();
        private static readonly SemaphoreSlim ConnectionLock = new SemaphoreSlim(1, 1);

        /// <summary>
        /// 连接到指定服务器
        /// </summary>
        public static ConnectionMultiplexer Connect(string host)
        {
            ConnectionLock.Wait();
            try
            {
                if (ConnectionCache.TryGetValue(host, out ConnectionMultiplexer connection) && connection.IsConnected)
                {
                    return connection;
                }

                connection = ConnectionMultiplexer.Connect(host);
                ConnectionCache[host] = connection;
                return connection;
            }
            finally
            {
                ConnectionLock.Release();
            }
        }
    }
}