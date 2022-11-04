using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Nado.Core.Utils;

public static class JsonUtil
{
    public static JsonSerializerSettings Settings = new ()
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver()
    };

    public static T? FromJson<T>(string json)
    {
        return JsonConvert.DeserializeObject<T>(json, Settings);
    }

    public static string ToJson(object? obj)
    {
        return JsonConvert.SerializeObject(obj, Settings);
    }
    
}