using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace CacheExample.Configuration
{
    public static class ConnectionMultiplexerConfigs
    {
        public static int AbsoluteExpirationRelativeToNow = 5;

        public static Lazy<ConfigurationOptions> CreateConfigs(IConfigurationRoot configuration)
        {
            var configOptions = CreateLazyConfigurationOptions(configuration);
            configOptions.Value.AllowAdmin = true;

            if (int.TryParse(configuration["RedisCache:Options:AbsoluteExpirationRelativeToNow"], out var timeParsed))
            {
                AbsoluteExpirationRelativeToNow = timeParsed;
            }

            return configOptions;
        }

        #region Private

        private static Lazy<ConfigurationOptions> CreateLazyConfigurationOptions(IConfigurationRoot configuration)
        {
            var clientName = configuration["RedisCache:Options:InstanceName"];
            var endPoint = configuration.GetConnectionString("RedisCache");

            var connectTimeout = 100000;
            if (int.TryParse(configuration["RedisCache:Options:ConnectTimeout"], out var connectTimeoutParsed))
            {
                connectTimeout = connectTimeoutParsed;
            }

            var syncTimeout = 100000;
            if (int.TryParse(configuration["RedisCache:Options:SyncTimeout"], out var syncTimeoutParsed))
            {
                syncTimeout = syncTimeoutParsed;
            }

            var configOptions = new Lazy<ConfigurationOptions>(new ConfigurationOptions
            {
                ClientName = string.IsNullOrEmpty(clientName) ? Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName) : clientName,
                ConnectRetry = 5,
                ConnectTimeout = connectTimeout,
                SyncTimeout = syncTimeout,
                ReconnectRetryPolicy = new ExponentialRetry(100, 500),
                AbortOnConnectFail = false,
                Password = configuration["RedisCache:Options:Password"]
            });

            configOptions.Value.EndPoints.Add(string.IsNullOrEmpty(endPoint) ? "localhost:6379" : endPoint);
            return configOptions;
        }

        #endregion
    }
}
