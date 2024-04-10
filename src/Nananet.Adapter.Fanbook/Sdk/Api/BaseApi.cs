using System.Text;
using Nananet.Adapter.Fanbook.Models;
using Nananet.Core.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RestSharp;

namespace Nananet.Adapter.Fanbook.Api;

public class BaseApi
{
    // 先简单实现

    // TODO 可能会变
    public const string Version = "2.0.0";
    public const int BuildNumber = 851;
    public const string AppKey = "ww0qqc0UHMsFtDUhGbh0";
    public const string ApiHost = "https://web.fanbook.cn";
    public const string ApiBaseUrl = $"{ApiHost}/api/a1";

    private RestClient _client = new(ApiBaseUrl);
    private JsonSerializerSettings _jsonSerializerSettings = new()
    {
        ContractResolver = new DefaultContractResolver()
        {
            NamingStrategy = new SnakeCaseNamingStrategy()
        }
    };

    public string TempId { get; private set; }
    public string DeviceId { get; private set; }
    public string Xsp { get; private set; }
    public string Token { get; private set; }
    public string? ClientId { get; internal set; }
    
    public BaseApi(string token)
    {
        Token = token;
        TempId = Guid.NewGuid().ToString();
        // DeviceId = "cb4b0d82-4c23-4f3d-ac38-14001a36a486";
        DeviceId = "b367a86d-39b9-818c-356d-d1092fa043d6";
        var xspJson = $@"{{""platform"":""web"",""version"":""{Version}"",""device_id"":""{DeviceId}"",""build_number"":""{BuildNumber}""}}";
        Xsp = Convert.ToBase64String(Encoding.UTF8.GetBytes(xspJson));
    }

    public async Task PostAsync(string url, string body, string? referer = null)
    {
        try
        {
            var request = new RestRequest(url, Method.Post);
            Logger.L.Debug($"Body: {body}");
            request.AddJsonBody(body);
            AddCommonHeaders(request, body);
            
            request.AddOrUpdateHeader("content-type", "application/json");
            request.AddOrUpdateHeader("accept", "application/json, text/plain, */*");
            if (referer != null)
                request.AddOrUpdateHeader("referer", referer);
            var response = await _client.ExecuteAsync(request);
            if (response.IsSuccessful)
            {
                if (response.Content != null)
                {
                    Logger.L.Debug($"PostAsync result: {response.Content}");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.L.Error(ex);
        }
    }
    
    private void AddCommonHeaders(RestRequest request, string body)
    {
        if (ClientId == null)
        {
            Logger.L.Error("ClientId is null");
            return;
        }
        
        var nonce = Guid.NewGuid().ToString();
        var signatureParams = new SignatureParams(AppKey, Token, nonce, body);
        
        request.AddOrUpdateHeader("sec-ch-ua", "\"Microsoft Edge\";v=\"123\", \"Not:A-Brand\";v=\"8\", \"Chromium\";v=\"123\"");
        request.AddOrUpdateHeader("dnt", "1");
        request.AddOrUpdateHeader("language", "zh-CN");
        request.AddOrUpdateHeader("nonce", signatureParams.Nonce);
        request.AddOrUpdateHeader("authorization", Token);
        request.AddOrUpdateHeader("signature", signatureParams.GenerateSignature());
        request.AddOrUpdateHeader("sec-ch-ua-platform", "\"Windows\"");
        request.AddOrUpdateHeader("x-super-properties", Xsp);
        request.AddOrUpdateHeader("sec-ch-ua-mobile", "?0");
        request.AddOrUpdateHeader("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36 Edg/123.0.0.0");
        request.AddOrUpdateHeader("timestamp", signatureParams.Timestamp);
        request.AddOrUpdateHeader("client-id", ClientId);
        request.AddOrUpdateHeader("platform", "web");
        request.AddOrUpdateHeader("appkey", AppKey);
        request.AddOrUpdateHeader("origin", ApiHost);
        request.AddOrUpdateHeader("sec-fetch-site", "same-origin");
        request.AddOrUpdateHeader("sec-fetch-mode", "cors");
        request.AddOrUpdateHeader("sec-fetch-dest", "empty");
        request.AddOrUpdateHeader("accept-encoding", "gzip, deflate, br, zstd");
        request.AddOrUpdateHeader("accept-language", "zh-CN,zh;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6");
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