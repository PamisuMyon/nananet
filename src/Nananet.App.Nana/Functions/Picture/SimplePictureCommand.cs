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

    protected delegate Task<string?> DownloadFunc();

    protected abstract DownloadFunc[] DownloadFuncs { get; }

    protected abstract PicCommandHints Hints { get; }

    protected Spam _temporarySpam = new Spam(0, 1, 5000);

    public override Task<CommandTestInfo> Test(Message input, CommandTestOptions options)
    {
        if (input is not TextMessage text) return Task.FromResult(NoConfidence);
        if (Regexes.Any(r => r.IsMatch(text.Content))) return Task.FromResult(FullConfidence);
        return Task.FromResult(NoConfidence);
    }

    public override async Task<CommandResult> Execute(IBot bot, Message input, CommandTestInfo testInfo)
    {
        if (!_temporarySpam.Check(input.AuthorId).Pass) return Executed;

        _temporarySpam.Record(input.AuthorId);
        var hintMsgId = await bot.ReplyTextMessage(input, Hints.DownloadingHint);

        var filePath = await DownloadFuncs.RandomElem().Invoke();
        string? error = null;
        if (filePath != null)
        {
            var imgMsgId = await bot.ReplyPictureFileMessage(input, filePath);
            FileUtil.DeleteUnreliably(filePath);
            if (imgMsgId == null)
                error = Hints.DownloadErrorHint;
            else
                await ActionLog.Log(Name, input, filePath);
        }
        else
        {
            error = Hints.DownloadErrorHint;
        }

        await Task.Delay(1000);
        if (hintMsgId != null)
        {
            var b = await bot.DeleteMessage(hintMsgId);
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

    private DownloadFunc[] _downloadFuncs =
    {
        DownloadRandomCat,
        DownloadTheCatApi
    };

    protected override DownloadFunc[] DownloadFuncs => _downloadFuncs;

    public static async Task<string?> DownloadTheCatApi()
    {
        var client = new RestClient("https://api.thecatapi.com/v1/images/search")
        {
            Timeout = _timeout
        };
        var request = new RestRequest();
        Logger.L.Debug("Requesting the cat api...");
        try
        {
            var response = await client.ExecuteGetAsync(request);
            var ja = JArray.Parse(response.Content!);
            var url = ja[0]["url"]!.ToString();
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

    public static async Task<string?> DownloadRandomCat()
    {
        var client = new RestClient("https://aws.random.cat/meow")
        {
            Timeout = _timeout,
        };
        var request = new RestRequest();
        Logger.L.Debug("Requesting random cat...");
        try
        {
            var response = await client.ExecuteGetAsync(request);
            var jo = JObject.Parse(response.Content!);
            var url = jo["file"]!.ToString();
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

    private DownloadFunc[] _downloadFuncs =
    {
        DownloadShibe,
    };

    protected override DownloadFunc[] DownloadFuncs => _downloadFuncs;
    
    public static async Task<string?> DownloadShibe()
    {
        var client = new RestClient("https://shibe.online/api/shibes?count=1&urls=true&httpsUrls=true")
        {
            Timeout = _timeout
        };
        var request = new RestRequest();
        Logger.L.Debug("Requesting shibe...");
        try
        {
            var response = await client.ExecuteGetAsync(request);
            var ja = JArray.Parse(response.Content!);
            var url = ja[0].ToString();
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
