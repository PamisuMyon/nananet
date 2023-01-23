using System.Text.RegularExpressions;
using Nananet.App.Nana.Models;
using Nananet.Core;
using Nananet.Core.Commands;
using Nananet.Core.Models;
using Nananet.Core.Utils;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Nananet.App.Nana.Functions.Picture;

public struct PicCommandHints
{
    public string DownloadingHint { get; set; }
    public string DownloadErrorHint { get; set; }
    public string SendErrorHint { get; set; }
}

public abstract class SimplePictureCommand : Command
{
    protected static int _timeout => 8000;
    protected abstract Regex[] Regexes { get; }

    protected List<SimplePictureRequester> PictureRequesters { get; } = new();

    protected abstract PicCommandHints Hints { get; }

    protected Spam _temporarySpam = new(0, 1, 5000);

    protected bool _downloadFile = false;

    public override async Task Init(IBot bot)
    {
        await base.Init(bot);
        if (bot.Config.Extra != null)
        {
            if (bot.Config.Extra.ContainsKey(""))
                _downloadFile = (bool)bot.Config.Extra["downloadFile"];
        }
    }
    
    public override Task<CommandTestInfo> Test(Message input, CommandTestOptions options)
    {
        if (!input.HasContent()) return Task.FromResult(NoConfidence);
        if (Regexes.Any(r => r.IsMatch(input.Content))) return Task.FromResult(FullConfidence);
        return Task.FromResult(NoConfidence);
    }

    public override async Task<CommandResult> Execute(IBot bot, Message input, CommandTestInfo testInfo)
    {
        if (!_temporarySpam.Check(input.AuthorId).Pass) return Executed;

        _temporarySpam.Record(input.AuthorId);
        var hintMsgId = await bot.ReplyTextMessage(input, Hints.DownloadingHint);

        var path = await PictureRequesters.RandomElem().Execute();
        string? error = null;
        if (path != null)
        {
            string? imgMsgId;
            if (_downloadFile)
                imgMsgId = await bot.ReplyLocalFileMessage(input, path, FileType.Image);
            else
                imgMsgId = await bot.ReplyServerFileMessage(input, path, FileType.Image);
            FileUtil.DeleteUnreliably(path);
            if (imgMsgId == null)
                error = Hints.DownloadErrorHint;
            else
                await ActionLog.Log(Name, input, path);
        }
        else
        {
            error = Hints.DownloadErrorHint;
        }

        await Task.Delay(1000);
        if (hintMsgId != null)
        {
            var b = await bot.DeleteMessage(input.ChannelId, hintMsgId);
            Logger.L.Debug($"Delete message: {b}  id {hintMsgId}");
        }

        if (error != null)
        {
            await bot.ReplyTextMessage(input, error);
            await ActionLog.Log(Name, input, error);
        }

        _temporarySpam.Reset(input.AuthorId);
        return Executed;
    }
}

public class SimplePictureRequester
{
    private readonly string _url;
    private readonly int _timeout;
    private readonly bool _downloadFile;
    public delegate string ParseResponse(string content);
    private readonly ParseResponse _getFileUrlFromResponse;

    public SimplePictureRequester(string url, int timeout, bool downloadFile, ParseResponse getFileUrlFromResponse)
    {
        _url = url;
        _timeout = timeout;
        _downloadFile = downloadFile;
        _getFileUrlFromResponse = getFileUrlFromResponse;
    }

    public async Task<string?> Execute()
    {
        var options = new RestClientOptions(_url)
        {
            MaxTimeout = _timeout,
        };
        var client = new RestClient(options);
        
        var request = new RestRequest();
        Logger.L.Debug($"Requesting: {_url}");
        try
        {
            var response = await client.ExecuteGetAsync(request);
            if (response.Content == null) return null;
            var url = _getFileUrlFromResponse(response.Content);
            if (!_downloadFile)
                return url;
            
            var fileName = url.Split("/")[^1];
            var dir = FileUtil.PathFromBase("cache/images");
            
            Logger.L.Debug($"Downloading file: {url}");
            await NetUtil.DownloadFile(url, dir, fileName);
            var path = Path.Combine(dir, fileName);
            Logger.L.Debug($"File downloaded: {path}" );
            return path;
        }
        catch (Exception e)
        {
            Logger.L.Error(e.Message);
        }

        return null;
    }
}

public class KittyCommand : SimplePictureCommand
{
    public override string Name => "kitty";

    private Regex[] _regexes =
    {
        new("来(点|电|份|张)猫(猫|图)"),
        new("來(點|電|份|張)貓(貓|圖)")
    };

    protected override Regex[] Regexes => _regexes;

    private PicCommandHints _hints = new()
    {
        DownloadingHint = "正在检索猫猫数据库，请博士耐心等待...",
        DownloadErrorHint = "图片被猫猫吞噬了，请博士稍后再试。",
        SendErrorHint = "图片发送过程中发生致命错误，您的开水壶已被炸毁。"
    };

    protected override PicCommandHints Hints => _hints;

    public override async Task Init(IBot bot)
    {
        await base.Init(bot);
        PictureRequesters.Add(
            new SimplePictureRequester("https://api.thecatapi.com/v1/images/search",
                _timeout, _downloadFile, content =>
                {
                    var ja = JArray.Parse(content);
                    return ja[0]["url"]!.ToString();
                }));
        
        PictureRequesters.Add(
            new SimplePictureRequester("https://aws.random.cat/meow",
                _timeout, _downloadFile, content =>
                {
                    var jo = JObject.Parse(content!);
                    return jo["file"]!.ToString();
                }));
    }
}

public class DogeCommand : SimplePictureCommand
{
    public override string Name => "doge";

    private Regex[] _regexes =
    {
        new("来(点|电|份|张)狗(狗|图)"),
        new("來(點|電|份|張)狗(狗|圖)")
    };

    protected override Regex[] Regexes => _regexes;

    private PicCommandHints _hints = new()
    {
        DownloadingHint = "正在检索狗狗数据库，请博士耐心等待...",
        DownloadErrorHint = "图片被狗狗吞噬了，请博士稍后再试。",
        SendErrorHint = "图片发送过程中发生致命错误，您的开水壶已被炸毁。"
    };

    protected override PicCommandHints Hints => _hints;

    public override async Task Init(IBot bot)
    {
        await base.Init(bot);
        PictureRequesters.Add(
            new SimplePictureRequester("https://shibe.online/api/shibes?count=1&urls=true&httpsUrls=true",
                _timeout, _downloadFile, content =>
                {
                    var ja = JArray.Parse(content!);
                    return ja[0].ToString();
                }));
    }
    
}
