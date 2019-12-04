using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CacheExample.Factories;
using CacheExample.Interfaces;
using CacheExample.Managers;
using CacheExample.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;

namespace CacheExample
{
    public class Program
    {
        public static IServiceProvider ServiceProvider { get; set; }
        static void Main(string[] args)
        {
            var config = GetIConfigurationRoot();

            Log.Logger = new LoggerConfiguration()
                .WriteTo
                .Console(LogEventLevel.Information)
                .CreateLogger();

            var tests = new CacheTests();
            var memoryCache = new MemoryCache(new MemoryCacheOptions());


            Task.WhenAll( 
                Task.Run(() => tests.ReadWriteIntoDistributedCache(config)),
                Task.Run(() => tests.ReadWriteIntoCache(config)),
                Task.Run(() => tests.ReadWriteIntoInMemoryCache(memoryCache, config))
                );
            
            Console.ReadKey();
        }

        #region Private

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            var directoryPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return new ConfigurationBuilder()
                .SetBasePath(directoryPath)
                .AddJsonFile("appsettings.json", optional: true)
                .Build();
        }

        #endregion
    }

    public class CacheTests
    {
        #region AppCache

        public void ReadWriteIntoCache(IConfigurationRoot configuration)
        {
            ICacheManager<string, UserForCaching> cache = new ApplicationCacheManager<UserForCaching>();
            RunTest(cache);
        }

        #endregion

        #region InMemoryCache

        public void ReadWriteIntoInMemoryCache(IMemoryCache memoryCache, IConfigurationRoot configuration)
        {
            ICacheManager<string, UserForCaching> cache = new InMemoryCacheManager<UserForCaching>(memoryCache, configuration);
            RunTest(cache);
        }

        #endregion

        #region DistributedCache

        public void ReadWriteIntoDistributedCache(IConfigurationRoot configuration)
        {
            ICacheManager<string, UserForCaching> cache = new RedisCacheManager<UserForCaching>(configuration);
            RunTest(cache);
        }

        #endregion
        
        #region Private

        public async void RunTest(ICacheManager<string, UserForCaching> cacheService)
        {
            var prefix = cacheService.GetType().Name;

            var model = CachingModelsFactory.CreateFakeUser();
            model.Id = $"{prefix}.{model.Id}";

            await Task.Run(() => ReadWriteIntoFromCache(cacheService, model));
        }

        private void ReadWriteIntoFromCache(ICacheManager<string, UserForCaching> cacheService, UserForCaching model)
        {
            //Writing
            Log.Information("Writing model with id:{0}", model.Id);
            var sw = Stopwatch.StartNew();
            var addResult = cacheService.TryAdd(model.Id, model);
            sw.Stop();

            if (!addResult.IsSuccess)
            {
                Log.Error(addResult.ErrorMessage);
                return;
            }
            Log.Information("Model with id: {0}, wrote. Time taken: {1} ms", model.Id, sw.Elapsed.TotalMilliseconds);


            //Reading
            Log.Information("Reading model with id:{0}", model.Id);

            sw = Stopwatch.StartNew();
            var getResult = cacheService.TryGet(model.Id);
            sw.Stop();

            if (!getResult.IsSuccess)
            {
                Log.Error(getResult.ErrorMessage);
                return;
            }
            Log.Information("Got model with id: {0}. Got time taken: {1} ms", model.Id, sw.Elapsed.TotalMilliseconds);


            //Reriding
            Log.Information("Waiting...");
            Thread.Sleep(TimeSpan.FromSeconds(10));
            sw = Stopwatch.StartNew();
            getResult = cacheService.TryGet(model.Id);
            sw.Stop();

            if (getResult.IsSuccess)
            {
                Log.Information("Got model with id: {0}. Time taken: {1} ms", model.Id, sw.Elapsed.TotalMilliseconds);
            }
            else
            {
                Log.Information("Entry with KEY: {0}, EXPIRED. Time taken: {1} ms", model.Id, sw.Elapsed.TotalMilliseconds);
                return;
            }


            //Removing

            Log.Information("Removing...");
            sw = Stopwatch.StartNew();
            var removeResult = cacheService.TryRemove(model.Id);
            sw.Stop();
            
            Log.Information("Remove model with id: {0}. Time taken: {1} ms", model.Id, sw.Elapsed.TotalMilliseconds);
        }
        #endregion

    }
}
