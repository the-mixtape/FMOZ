using Newtonsoft.Json;

namespace OutbreakZCore.Shared.Utils
{
    public static class Converter
    {
        public static T FromJson<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static string ToJson<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }    
}
