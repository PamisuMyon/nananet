using System.Text.RegularExpressions;
using Nananet.App.Nana.Models;
using Nananet.Core;
using Nananet.Core.Commands;
using Nananet.Core.Models;
using Nananet.Core.Utils;

namespace Nananet.App.Nana.Functions.Picture;

public struct PicCommandHints
{
    public string DownloadingHint { get; set; }
    public string DownloadErrorHint { get; set; }
    public string SendErrorHint { get; set; }
}

public abstract class PictureCommand : Command
{
    protected static int _timeout => 8000;
    protected abstract Regex[] Regexes { get; }

    protected List<PictureRequester> PictureRequesters { get; } = new();

    protected abstract PicCommandHints Hints { get; }

    protected Spam _temporarySpam = new(0, 1, 5000);

    protected bool _downloadFile = false;

    public override async Task Init(IBot bot)
    {
        await base.Init(bot);
        bot.Config.TryGetExtraValue("downloadFile", out _downloadFile);
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

        var path = await PictureRequesters.RandomElem().Execute(_timeout, _downloadFile);
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
                error = Hints.SendErrorHint;
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

public class KittyCommand : PictureCommand
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
        SendErrorHint = "图片被企鹅吞噬了，请博士稍后再试。"
    };

    protected override PicCommandHints Hints => _hints;

    public override async Task Init(IBot bot)
    {
        await base.Init(bot);
        if (PictureRequesters.Count != 0) return;
        PictureRequesters.Add(PictureRequesterStore.TheCatApi);
        PictureRequesters.Add(PictureRequesterStore.AwsRandomCat);
    }
}

public class DogeCommand : PictureCommand
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
        SendErrorHint = "图片被企鹅吞噬了，请博士稍后再试。"
    };

    protected override PicCommandHints Hints => _hints;

    public override async Task Init(IBot bot)
    {
        await base.Init(bot);
        if (PictureRequesters.Count != 0) return;
        PictureRequesters.Add(PictureRequesterStore.Shibe);
    }
    
}

public class QuackCommand : PictureCommand
{
    public override string Name => "doge";

    private Regex[] _regexes =
    {
        new("来(点|电|份|张)鸭(鸭|图)"),
        new("來(點|電|份|張)鴨(鴨|圖)")
    };

    protected override Regex[] Regexes => _regexes;

    private PicCommandHints _hints = new()
    {
        DownloadingHint = "正在检索鸭鸭数据库，请博士耐心等待...",
        DownloadErrorHint = "图片被鸭鸭吞噬了，请博士稍后再试。",
        SendErrorHint = "图片被企鹅吞噬了，请博士稍后再试。"
    };

    protected override PicCommandHints Hints => _hints;

    public override async Task Init(IBot bot)
    {
        await base.Init(bot);
        if (PictureRequesters.Count != 0) return;
        PictureRequesters.Add(PictureRequesterStore.RandomDuck);
    }
    
}

public class FoxCommand : PictureCommand
{
    public override string Name => "doge";

    private Regex[] _regexes =
    {
        new("来(点|电|份|张)小?狐狸?(狐|图)"),
        new("來(點|電|份|張)小?狐狸?(狐|圖)")
    };

    protected override Regex[] Regexes => _regexes;

    private PicCommandHints _hints = new()
    {
        DownloadingHint = "正在检索小狐狸数据库，请博士耐心等待...",
        DownloadErrorHint = "图片被小狐狸吞噬了，请博士稍后再试。",
        SendErrorHint = "图片被企鹅吞噬了，请博士稍后再试。"
    };

    protected override PicCommandHints Hints => _hints;

    public override async Task Init(IBot bot)
    {
        await base.Init(bot);
        if (PictureRequesters.Count != 0) return;
        PictureRequesters.Add(PictureRequesterStore.RandomFox);
    }
    
}

public class CatBoyCommand : PictureCommand
{
    public override string Name => "catboy";

    private Regex[] _regexes =
    {
        new("来(点|电|份|张)猫猫?男孩?子?图?"),
        new("來(點|電|份|張)貓貓?男孩?子?圖?")
    };

    protected override Regex[] Regexes => _regexes;

    private PicCommandHints _hints = new()
    {
        DownloadingHint = "正在检索猫猫数据库，请博士耐心等待...",
        DownloadErrorHint = "图片被猫猫吞噬了，请博士稍后再试。",
        SendErrorHint = "图片被企鹅吞噬了，请博士稍后再试。"
    };

    protected override PicCommandHints Hints => _hints;

    public override async Task Init(IBot bot)
    {
        await base.Init(bot);
        if (PictureRequesters.Count != 0) return;
        PictureRequesters.Add(PictureRequesterStore.RandomCatBoy);
    }
    
}
