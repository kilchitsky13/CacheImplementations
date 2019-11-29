using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using CacheExample.Enums;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace CacheExample.Configuration
{
    public static class ConnectionMultiplexerConfigs
    {
        public static RedisCacheHealthStatus Health { get; private set; }

        public static long LastUnhealthTime { get; private set; }

        public static int AbsoluteExpirationRelativeToNow = 5;
        private static int _healthCheckTimeDelay = 30;

        public static Lazy<ConfigurationOptions> CreateConfigs(IConfigurationRoot configuration)
        {
            if (int.TryParse(configuration["RedisCache:HealthCheckTimeDelay"], out var healthCheckTime))
            {
                _healthCheckTimeDelay = healthCheckTime;
            }

            var configOptions = CreateLazyConfigurationOptions(configuration);
            configOptions.Value.AllowAdmin = true;

            if (int.TryParse(configuration["RedisCache:Options:AbsoluteExpirationRelativeToNow"], out var timeParsed))
            {
                AbsoluteExpirationRelativeToNow = timeParsed;
            }

            return configOptions;
        }

        public static void SetUnHealthy()
        {
            Health = RedisCacheHealthStatus.Unhealth;
            Task.Delay(TimeSpan.FromSeconds(_healthCheckTimeDelay))
                        .ContinueWith(t =>
                        {
                            Health = RedisCacheHealthStatus.Non;
                        });
        }

        public static void SetHealthy()
        {
            if (Health != RedisCacheHealthStatus.Health)
            {
                LastUnhealthTime = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
                Health = RedisCacheHealthStatus.Health;
            }
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
                ConnectTimeout = connectTimeout,
                SyncTimeout = syncTimeout,
                ConnectRetry = 5,
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
