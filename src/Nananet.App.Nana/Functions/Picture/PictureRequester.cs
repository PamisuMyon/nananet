using System.Text.RegularExpressions;
using Nananet.Core.Utils;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Nananet.App.Nana.Functions.Picture;

public class PictureRequester
{
    protected virtual string Url { get; set; }

    public delegate string ParseResponse(string content);

    protected ParseResponse _getFileUrlFromResponse;
    
    // hard-code 允许的文件类型
    protected readonly Regex _fileTypeRegex = new("\\.(jpg|jpeg|png|gif|webp|bmp)$", RegexOptions.IgnoreCase);

    public PictureRequester() {}
    
    public PictureRequester(string url, ParseResponse getFileUrlFromResponse)
    {
        Url = url;
        _getFileUrlFromResponse = getFileUrlFromResponse;
    }

    public async Task<string?> Execute(int timeout, bool downloadFile, int retryTimes = 3)
    {
        var options = new RestClientOptions(Url)
        {
            MaxTimeout = timeout,
        };
        var client = new RestClient(options);

        var request = new RestRequest();
        Logger.L.Debug($"Requesting: {Url}");
        do
        {
            try
            {
                var response = await client.ExecuteGetAsync(request);
                if (response.Content == null) return null;
                var url = _getFileUrlFromResponse(response.Content);
                if (!_fileTypeRegex.IsMatch(url))
                {
                    Logger.L.Error("Picture Requester file type not allowed, retrying...");
                    continue;
                }
                if (!downloadFile)
                    return url;

                var fileName = url.Split("/")[^1];
                var dir = FileUtil.PathFromBase("cache/images");

                Logger.L.Debug($"Downloading file: {url}");
                await NetUtil.DownloadFile(url, dir, fileName);
                var path = Path.Combine(dir, fileName);
                Logger.L.Debug($"File downloaded: {path}");
                return path;
            }
            catch (Exception e)
            {
                Logger.L.Error($"Picture Requester Error: {e.Message}");
                Logger.L.Error(e.StackTrace);
                retryTimes--;
                await Task.Delay(100);
                Logger.L.Error($"Picture Requester retrying({retryTimes})");
            }
        } while (retryTimes >= 0);
        return null;
    }
}

public class NekosBestRequester : PictureRequester
{
    private string[] categories  = { "neko", "kitsune", "waifu" };
    protected override string Url => $"https://nekos.best/api/v2/{categories.RandomElem()}";

    public NekosBestRequester()
    {
        _getFileUrlFromResponse = content =>
        {
            var jo = JObject.Parse(content);
            var result = jo["results"]![0];
            return result!["url"]!.ToString();
        };
    }
}

public static class PictureRequesterStore
{
    public static readonly PictureRequester TheCatApi = new("https://api.thecatapi.com/v1/images/search", content =>
    {
        var ja = JArray.Parse(content);
        return ja[0]["url"]!.ToString();
    });

    public static readonly PictureRequester AwsRandomCat = new("https://aws.random.cat/meow", content =>
    {
        var jo = JObject.Parse(content);
        return jo["file"]!.ToString();
    });

    public static readonly PictureRequester Shibe = new("https://shibe.online/api/shibes?count=1&urls=true&httpsUrls=true", content =>
    {
        var ja = JArray.Parse(content);
        return ja[0].ToString();
    });
    
    public static readonly PictureRequester RandomDuck = new("https://random-d.uk/api/v2/random", content =>
    {
        var jo = JObject.Parse(content);
        return jo["url"]!.ToString();
    });
    
    public static readonly PictureRequester RandomFox = new("https://randomfox.ca/floof", content =>
    {
        var jo = JObject.Parse(content);
        return jo["image"]!.ToString();
    });
    
    public static readonly PictureRequester RandomCatBoy = new("https://api.catboys.com/img", content =>
    {
        var jo = JObject.Parse(content);
        return jo["url"]!.ToString();
    });
    
    public static readonly PictureRequester WaifuPics = new("https://api.waifu.pics/sfw/waifu", content =>
    {
        var jo = JObject.Parse(content);
        return jo["url"]!.ToString();
    });

    public static readonly NekosBestRequester NekosBest = new();
    
    
}