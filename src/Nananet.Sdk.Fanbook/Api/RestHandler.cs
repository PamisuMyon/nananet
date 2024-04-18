using System.Net;
using System.Text;
using Nananet.Core.Utils;
using Nananet.Sdk.Fanbook.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Nananet.Sdk.Fanbook.Api;

public class RestHandler
{
    // 先简单实现

    public const string ApiHost = "https://web.fanbook.cn";
    public const string ApiBaseUrl = $"{ApiHost}/api/a1/api";

    private JsonSerializerSettings _jsonSerializerSettings = new()
    {
        ContractResolver = new DefaultContractResolver()
        {
            NamingStrategy = new SnakeCaseNamingStrategy()
        }
    };

    public ClientRuntimeData RuntimeData { get; private set; }
    
    public RestHandler(ClientRuntimeData runtimeData)
    {
        RuntimeData = runtimeData;
    }

    public async Task<T?> PostAsync<T>(string url, string body)
    {
        if (RuntimeData.CurrentGuildId == null || RuntimeData.CurrentChannelId == null)
        {
            Logger.L.Error("Please set browser context first.");
            return default;
        }
        try
        {
            url = $"{ApiBaseUrl}/{url}";
            var handler = new HttpClientHandler();
            handler.CookieContainer = new CookieContainer();
            AddCookies(handler.CookieContainer);
            using var client = new HttpClient(handler);
            // using var client = new HttpClient();
            client.DefaultRequestVersion = new Version(2, 0);
            var content = new StringContent(body, Encoding.UTF8, "application/json");
            AddCommonHeaders(client, body);
            client.DefaultRequestHeaders.Add("referer", $"{ApiHost}/channels/{RuntimeData.CurrentGuildId}/{RuntimeData.CurrentChannelId}");
            var response = await client.PostAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Response Body:");
                Console.WriteLine(responseBody);
                if (!string.IsNullOrEmpty(responseBody))
                {
                    var result = FromJson<T>(responseBody);
                    return result;
                }
            }
            else
            {
                Logger.L.Error($"Failed to make POST request. Status code: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Logger.L.Error($"POST Request Error: {ex.Message}");
            Logger.L.Error(ex.StackTrace);
        }

        return default;
    }
    
    private void AddCommonHeaders(HttpClient client, string body)
    {
        if (RuntimeData.ClientId == null)
        {
            Logger.L.Error("ClientId is null");
            return;
        }
        
        var nonce = Guid.NewGuid().ToString();
        var signatureParams = new Signature(RuntimeData.Config.AppKey, RuntimeData.Config.Token, nonce, body);
        
        client.DefaultRequestHeaders.Add("sec-ch-ua", "\"Microsoft Edge\";v=\"123\", \"Not:A-Brand\";v=\"8\", \"Chromium\";v=\"123\"");
        client.DefaultRequestHeaders.Add("dnt", "1");
        client.DefaultRequestHeaders.Add("language", "zh-CN");
        client.DefaultRequestHeaders.Add("nonce", signatureParams.Nonce);
        client.DefaultRequestHeaders.Add("authorization", RuntimeData.Config.Token);
        client.DefaultRequestHeaders.Add("signature", signatureParams.GenerateSignature());
        client.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");
        client.DefaultRequestHeaders.Add("x-super-properties", RuntimeData.Xsp);
        client.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
        client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36 Edg/123.0.0.0");
        client.DefaultRequestHeaders.Add("timestamp", signatureParams.Timestamp.ToString());
        client.DefaultRequestHeaders.Add("client-id", RuntimeData.ClientId);
        client.DefaultRequestHeaders.Add("platform", "web");
        client.DefaultRequestHeaders.Add("appkey", RuntimeData.Config.AppKey);
        client.DefaultRequestHeaders.Add("origin", ApiHost);
        client.DefaultRequestHeaders.Add("sec-fetch-site", "same-origin");
        client.DefaultRequestHeaders.Add("sec-fetch-mode", "cors");
        client.DefaultRequestHeaders.Add("sec-fetch-dest", "empty");
        client.DefaultRequestHeaders.Add("accept-encoding", "gzip, deflate, br, zstd");
        client.DefaultRequestHeaders.Add("accept-language", "zh-CN,zh;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6");
        client.DefaultRequestHeaders.Add("accept", "application/json, text/plain, */*");
    }

    private void AddCookies(CookieContainer container)
    {
        const string path = "/";
        const string domain = "web.fanbook.cn";
        
        container.Add(new Cookie("did", RuntimeData.Config.DeviceId, path, domain));
        foreach (var it in RuntimeData.Config.DummyCookies)
        {
            container.Add(new Cookie(it.Key, it.Value, path, domain));
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
    
    public long Nonce()
    {
        long nonce = 0;
        for (var i = 0; i < 18; i++)
        {
            nonce = nonce * 10 + Random.Shared.Next(0, 10);
        }
        return nonce;
    }
    
}