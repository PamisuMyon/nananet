using Nananet.Core.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RestSharp;

namespace Nananet.Adapter.Fanbook.Api;

public class RestHandler
{
    // 先简单实现

    public const string Host = "https://a1.fanbook.cn";

    private RestClient _client;
    private JsonSerializerSettings _jsonSerializerSettings = new()
    {
        ContractResolver = new DefaultContractResolver()
        {
            NamingStrategy = new SnakeCaseNamingStrategy()
        }
    };

    public string Token { get; private set; }
    public string BaseUrl { get; private set; }
    
    public RestHandler(string token)
    {
        Token = token;
        BaseUrl = $"{Host}/api/bot/{token}";
        _client = new RestClient(BaseUrl);
    }

    public async Task GetAsync(string uri)
    {
        try
        {
            var request = new RestRequest(uri, Method.Get);
            var response = await _client.ExecuteAsync(request);
            if (response.IsSuccessful)
            {
                if (response.Content != null)
                {
                    Logger.L.Debug($"GetAsync result: {response.Content}");
                }
            }
            else
            {
                Logger.L.Error($"GetAsync failed: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Logger.L.Error(ex);
        }
    }

    public async Task PostAsync(string uri, string body)
    {
        try
        {
            var request = new RestRequest(uri, Method.Post);
            Logger.L.Debug($"Body: {body}");
            request.AddJsonBody(body);
            request.AddOrUpdateHeader("content-type", "application/json");
            var response = await _client.ExecuteAsync(request);
            if (response.IsSuccessful)
            {
                if (response.Content != null)
                {
                    Logger.L.Debug($"PostAsync result: {response.Content}");
                }
            }
            else
            {
                Logger.L.Error($"PostAsync failed: {response.StatusCode}, {response.StatusDescription}");
            }
        }
        catch (Exception ex)
        {
            Logger.L.Error(ex);
        }
    }
    
    public string ToJson(object obj)
    {
        return JsonConvert.SerializeObject(obj, Formatting.None, _jsonSerializerSettings);
    }

    public T? FromJson<T>(string json)
    {
        return JsonConvert.DeserializeObject<T>(json, _jsonSerializerSettings);
    }

    
}