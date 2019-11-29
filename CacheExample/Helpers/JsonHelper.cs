using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;

namespace CacheExample.Helpers
{
    public class JsonHelper
    {
        public static string Serialize(object msesage)
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            return JsonConvert.SerializeObject(msesage, settings);
        }

        public static T Deserialize<T>(string message)
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            T result = default(T);
            try
            {
                result = JsonConvert.DeserializeObject<T>(message, settings);
            }
            catch (Exception)
            {
                Log.Error($"Error with deserialize message ({message}) to type {typeof(T)}");
            }
            return result;
        }
    }
}
