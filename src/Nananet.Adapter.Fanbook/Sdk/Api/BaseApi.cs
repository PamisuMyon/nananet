using System.Net;
using System.Text;
using Nananet.Adapter.Fanbook.Models;
using Nananet.Adapter.Fanbook.Sdk.Models;
using Nananet.Core.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Nananet.Adapter.Fanbook.Api;

public class BaseApi
{
    // 先简单实现

    public const string ApiHost = "https://web.fanbook.cn";
    public const string ApiBaseUrl = $"{ApiHost}/api/a1";

    private JsonSerializerSettings _jsonSerializerSettings = new()
    {
        ContractResolver = new DefaultContractResolver()
        {
            NamingStrategy = new SnakeCaseNamingStrategy()
        }
    };
    
    public ClientConfig Config { get; private set; }
    public string TempId { get; private set; }
    public string Xsp { get; private set; }
    public string Token => Config.Token;
    public string? ClientId { get; internal set; }
    
    public BaseApi(ClientConfig config)
    {
        Config = config;
        TempId = Guid.NewGuid().ToString();
        var xspJson = $@"{{""platform"":""web"",""version"":""{Config.AppVersion}"",""device_id"":""{Config.DeviceId}"",""build_number"":""{Config.BuildNumber}""}}";
        Xsp = Convert.ToBase64String(Encoding.UTF8.GetBytes(xspJson));
    }

    public async Task PostAsync(string url, string body, string? referer = null)
    {
        try
        {
            url = $"{ApiBaseUrl}{url}";
            var handler = new HttpClientHandler();
            handler.CookieContainer = new CookieContainer();
            AddCookies(handler.CookieContainer);
            using var client = new HttpClient(handler);
            // using var client = new HttpClient();
            client.DefaultRequestVersion = new Version(2, 0);
            var content = new StringContent(body, Encoding.UTF8, "application/json");
            AddCommonHeaders(client, content, body);
            if (referer != null)
                client.DefaultRequestHeaders.Add("referer", referer);
            var response = await client.PostAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Response Body:");
                Console.WriteLine(responseBody);
            }
            else
            {
                Console.WriteLine($"Failed to make POST request. Status code: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Logger.L.Error(ex);
        }
    }
    
    private void AddCommonHeaders(HttpClient client, HttpContent content, string body)
    {
        if (ClientId == null)
        {
            Logger.L.Error("ClientId is null");
            return;
        }
        
        var nonce = Guid.NewGuid().ToString();
        var signatureParams = new SignatureParams(Config.AppKey, Token, nonce, body);
        
        client.DefaultRequestHeaders.Add("sec-ch-ua", "\"Microsoft Edge\";v=\"123\", \"Not:A-Brand\";v=\"8\", \"Chromium\";v=\"123\"");
        client.DefaultRequestHeaders.Add("dnt", "1");
        client.DefaultRequestHeaders.Add("language", "zh-CN");
        client.DefaultRequestHeaders.Add("nonce", signatureParams.Nonce);
        client.DefaultRequestHeaders.Add("authorization", Token);
        client.DefaultRequestHeaders.Add("signature", signatureParams.GenerateSignature());
        client.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");
        client.DefaultRequestHeaders.Add("x-super-properties", Xsp);
        client.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
        client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36 Edg/123.0.0.0");
        client.DefaultRequestHeaders.Add("timestamp", signatureParams.Timestamp.ToString());
        client.DefaultRequestHeaders.Add("client-id", ClientId);
        client.DefaultRequestHeaders.Add("platform", "web");
        client.DefaultRequestHeaders.Add("appkey", Config.AppKey);
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
        
        container.Add(new Cookie("did", Config.DeviceId, path, domain));
        foreach (var it in Config.DummyCookies)
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