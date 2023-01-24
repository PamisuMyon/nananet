using Nananet.App.Nana.Models;
using Nananet.Core.Utils;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Nananet.App.Nana.Functions.AI;

public class BaiduOcr
{
    
    private const string GeneralApi = "https://aip.baidubce.com/rest/2.0/ocr/v1/general_basic";
    private const int RetryTimes = 3;
    private const int RetryWait = 500;
    private const int MaxQps = 2;
    
    private static int _concurrency;
    public static bool LimitReached { get; set; }

    public static async Task<List<string>?> Execute(string imageUrl)
    {
        if (LimitReached)
            return null;
        while (_concurrency >= MaxQps)
        {
            Logger.L.Info($"Baidu max qps limit reached, concurrency {_concurrency}");
            await Task.Delay(RetryWait);
        }

        _concurrency++;
        var client = new RestClient(GeneralApi);
        var request = new RestRequest("");
        request.AddQueryParameter("access_token", BaiduAuth.AccessToken);
        request.AddParameter("url", imageUrl);

        var shouldRetry = false;
        var retry = RetryTimes + 1;
        List<string> words = new();

        do
        {
            try
            {
                shouldRetry = false;

                var response = await client.ExecutePostAsync(request);
                Logger.L.Debug($"Baidu OCR result: {response.Content}");
                var jo = JObject.Parse(response.Content!);
                var wordsResult = jo["words_result"];
                if (wordsResult != null && wordsResult.HasValues)
                {
                    foreach (var it in wordsResult.Children())
                    {
                        if (it["words"] != null)
                            words.Add(it["words"]!.ToString());
                    }
                }
                else
                {
                    // 错误码 https://ai.baidu.com/ai-doc/OCR/dk3h7y5vr
                    var errorCode = jo["error_code"]!.ToObject<int>();
                    if (errorCode == 17 || errorCode == 19)
                    {
                        // 请求超过限额 不再进行尝试
                        Logger.L.Error("Baidu OCR limit reached.");
                        LimitReached = true;
                    }
                    else if (errorCode == 18)
                    {
                        // QPS超过限额
                        shouldRetry = true;
                        Logger.L.Error("Baidu max qps limit reached.");
                        await Task.Delay(RetryWait * 2);
                    }
                    else
                    {
                        shouldRetry = true;
                        await Task.Delay(RetryWait);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.L.Error($"Baidu OCR Error: {e.Message}");
                Logger.L.Error(e.StackTrace);
            }

            retry--;
        } while (shouldRetry && retry > 0);
        _concurrency--;

        return words;
    }
    
}
