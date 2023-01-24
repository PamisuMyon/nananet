using Nananet.App.Nana.Models;
using Nananet.Core.Utils;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Nananet.App.Nana.Functions.AI;

public class BaiduAuth
{
    public static string? AccessToken { get; set; }

    public static async Task Init()
    {
        var config = await MiscConfig.FindByName<BaiduConfig>("baiduConfig");
        if (config == null)
        {
            Logger.L.Error("baiduConfig not found in collection misc-config.");
            return;
        }
        Logger.L.Info("Baidu cloud authorizing...");
        var client = new RestClient($"https://aip.baidubce.com/oauth/2.0/token?grant_type=client_credentials&client_id={config.ApiKey}&client_secret={config.SecretKey}");

        try
        {
            var response = await client.ExecuteAsync(new RestRequest("", Method.Post));
            if (response.Content != null)
            {
                var jo = JObject.Parse(response.Content);
                AccessToken = jo["access_token"]?.ToString();
                Logger.L.Info("Baidu cloud authorized.");
            }
            else
                Logger.L.Error($"Baidu cloud authorization failed.");
        }
        catch (Exception e)
        {
            Logger.L.Error($"Baidu cloud authorization failed: {e.Message}");
            Logger.L.Error(e.StackTrace);
        }
    }
}

public class BaiduConfig
{
    public string ApiKey;
    public string SecretKey;
}

