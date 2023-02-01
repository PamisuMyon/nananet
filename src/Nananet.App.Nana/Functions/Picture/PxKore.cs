using System.Text;
using Nananet.App.Nana.Models;
using Nananet.Core.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Nananet.App.Nana.Functions.Picture;

public class PxKore
{
    public class IllustOptions
    {
        public string[]? Tags { get; set; }
        public string[]? ExcludedTags { get; set; }
        public bool? Fallback { get; set; }
        public string[]? FallbackTags { get; set; }
        public string? Proxy { get; set; }
        public int? RetryTimes { get; set; }
        public string? ClientId { get; set; }
        public bool ShouldRecord { get; set; }
        public string? MatchMode { get; set; }
        public bool ReturnTotalSample { get; set; }

        [JsonIgnore]
        public bool IsRandomSample { get; set; }
        [JsonIgnore]
        public bool AppendTotalSampleInfo { get; set; }
    }
    
    public struct IllustResult
    {
        public int TotalSample { get; set; }
        public bool Fallback { get; set; }
        public JArray Data { get; set; }
        [JsonIgnore]
        public string Url { get; set; }
        [JsonIgnore]
        public string FileName { get; set; }
        [JsonIgnore]
        public string Info { get; set; }
    }
    
    private int _defaultRetryTimes = 5;
    private RestClient _client;
    private string? _storagePath;

    public PxKore(string? storagePath = null)
    {
        _client = new RestClient("http://127.0.0.1:7007");
        _storagePath = storagePath?? FileUtil.PathFromBase("../storage/illusts");
    }

    public async Task<IllustResult?> RequestIllust(IllustOptions? options)
    {
        var opt = new IllustOptions
        {
            Fallback = true,
            ShouldRecord = false,
            IsRandomSample = true,
            AppendTotalSampleInfo = false,
            RetryTimes = _defaultRetryTimes,
            MatchMode = "tags",
            ReturnTotalSample = true,
        };
        if (options != null)
            opt.MergeWith(options);

        var blocks = await MiscConfig.FindByName<Dictionary<string, string[]>>("blockedPicIds");
        string[]? blockList = null;
        if (blocks != null && blocks.ContainsKey("ids"))
            blockList = blocks["ids"];
        var result = await DoRequestIllust(opt, blockList);
        if (result != null)
        {
            var r = result.Value;
            var data = r.Data[0];
            var info = new StringBuilder();
            if (data["title"] != null)
                info.Append(data["title"]);
            info.Append($"  by {data["author_name"]}");
            info.Append($"  ID {data["id"]}");
            if (opt.AppendTotalSampleInfo)
            {
                if (!opt.IsRandomSample
                    && r.TotalSample > 0
                    && !r.Fallback)
                    info.Append($"    -🖼{r.TotalSample}-");
                else
                    info.Append("    -🎲-");
            }

            r.Info = info.ToString();
            return r;
        }

        return null;
    }

    private async Task<IllustResult?> DoRequestIllust(IllustOptions o, string[]? blockList = null)
    {
        if (o.RetryTimes <= 0) return null;

        var request = new RestRequest("api/v1/illust");
        request.AddJsonBody(JsonUtil.ToJson(o));
        try
        {
            Logger.L.Info("Requesting pxkore...");
            var response = await _client.ExecutePostAsync(request);
            var result = JsonUtil.FromJson<IllustResult>(response.Content!);
            if (result.Data != null && result.Data.Count > 0)
            {
                var data = result.Data[0];
                var id = data["id"]!.ToString();
                // Is illust in block list
                if (blockList != null && blockList.Contains(id))
                {
                    Logger.L.Info($"Blocked id detected, retry: {id}");
                    if (o.Tags != null)
                        o.Tags = o.Tags.Where(it => it != id).ToArray();
                    o.RetryTimes -= 1;
                    return await DoRequestIllust(o);
                }

                result.Url = data["urls"]!["regular"]!.ToString();
                result.FileName = result.Url.Split('/')[^1];
                return result;
            }
        }
        catch (Exception e)
        {
            Logger.L.Error("PxKore DoRequestIllust Error");
            Logger.L.Error(e.Message);
            Logger.L.Error(e.StackTrace);
        }

        return null;
    }

    public async Task<string?> Download(string url, string fileName)
    {
        if (_storagePath != null)
        {
            var storageFile = Path.Combine(_storagePath, fileName);
            if (File.Exists(storageFile))
            {
                Logger.L.Info($"Use file from storage: {storageFile}");
                return storageFile;
            }
        }

        var cacheDir = FileUtil.PathFromBase("cache/illusts");
        try
        {
            Logger.L.Info($"Downloading file: {url}");
            await NetUtil.DownloadFile(url, cacheDir, fileName);
            var path = Path.Combine(cacheDir, fileName);
            Logger.L.Info($"File downloaded: {path}" );
            return path;
        }
        catch (Exception e)
        {
            Logger.L.Error(e.Message);
        }

        return null;
    }
    
}