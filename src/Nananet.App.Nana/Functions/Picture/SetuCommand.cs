using System.Text.RegularExpressions;
using Nananet.App.Nana.Models;
using Nananet.Core;
using Nananet.Core.Commands;
using Nananet.Core.Models;
using Nananet.Core.Utils;

namespace Nananet.App.Nana.Functions.Picture;

public class SetuCommand : Command
{
    
    public class SetuConfig
    {
        public string[] Tags { get; set; }
        public string[] ExcludedTags { get; set; }
        public string[][] FallbackTags { get; set; }
        public string ClientId { get; set; }
    }
    
    public override string Name => "recruit";

    protected int _timeout = 8000;
    protected Regex[] _regexes =
    {
        new ("来(点|电|份|张)(涩|瑟|色|美|帅)?图(片|图)? *　*(.*)"),
        new ("來(點|電|份|張)(澀|瑟|色|美|帥)?圖(片|圖)? *　*(.*)"),
    };
    private PicCommandHints _hints = new()
    {
        DownloadingHint = "正在检索猫猫数据库，请博士耐心等待...",
        DownloadErrorHint = "图片被猫猫吞噬了，请博士稍后再试。",
        SendErrorHint = "图片发送过程中发生致命错误，您的开水壶已被炸毁。"
    };
    
    protected Spam _temporarySpam = new(0, 1, 5000);
    protected string _imageProxy = "i.pixiv.cat";
    protected PxKore _pxKore;

    
    public override async Task Init(IBot bot)
    {
        await base.Init(bot);
        _pxKore = new PxKore();

        if (bot.Config.Extra != null)
        {
            if (bot.Config.Extra.ContainsKey("setuProxy"))
                _imageProxy = bot.Config.Extra["setuProxy"].ToString()!;
            else
                _imageProxy = "i.pixiv.cat";
        }
    }
    
    public override Task<CommandTestInfo> Test(Message input, CommandTestOptions options)
    {
        if (!input.HasContent()) return Task.FromResult(NoConfidence);
        foreach (var regex in _regexes)
        {
            if (regex.IsMatch(input.Content))
            {
                var m = regex.Match(input.Content);
                return Task.FromResult(new CommandTestInfo
                {
                    Confidence = 1,
                    Data = m
                });
            }
        }
        return Task.FromResult(NoConfidence);
    }

    public override async Task<CommandResult> Execute(IBot bot, Message input, CommandTestInfo testInfo)
    {
        if (testInfo.Data is not Match m) return Failed;
        if (!_temporarySpam.Check(input.AuthorId).Pass) return Executed;

        _temporarySpam.Record(input.AuthorId);

        var config = bot.Config.GetChannel(input.ChannelId);
        SetuConfig? setuConfig = null;
        if (config != null
            && config.Wildcards != null
            && config.Wildcards.ContainsKey("setu"))
        {
            setuConfig = JsonUtil.FromJson<SetuConfig>(config.Wildcards["setu"].ToString()!);
        }

        var options = new PxKore.IllustOptions();
        if (setuConfig != null)
        {
            options.ClientId = setuConfig.ClientId;
            options.Tags = setuConfig.Tags;
            options.ExcludedTags = setuConfig.ExcludedTags;
            options.FallbackTags = setuConfig.FallbackTags.SelectMany(i => i).ToArray();
        }
        options.ClientId ??= input.IsPersonal ? input.AuthorId : "public";
        options.ShouldRecord = true;
        options.IsRandomSample = false;
        options.AppendTotalSampleInfo = true;

        if (!string.IsNullOrEmpty(m.Groups[4].Value))
        {
            var keywords = m.Groups[4].Value.Replace('　', ' ').Trim().Split(' ');
            options.Tags = keywords;
        }
        if ((options.Tags == null || options.Tags.Length == 0)
            && setuConfig != null)
        {
            options.Tags = setuConfig.Tags;
            options.IsRandomSample = true;
        }

        if (setuConfig != null)
        {
            options.ExcludedTags = setuConfig.ExcludedTags;
            if (setuConfig.FallbackTags.Length > 0)
                options.FallbackTags = setuConfig.FallbackTags.RandomElem();
        }
        
        var hintMsgId = await bot.ReplyTextMessage(input, _hints.DownloadingHint);
        
        // Request pxkore server
        var illust = await _pxKore.RequestIllust(options);
        string? error = null;
        if (illust != null)
        {
            // Use server cache if it already exists
            var serverImage = await ServerImage.GetOne(illust.Value.FileName);
            if (serverImage != null)
            {
                Logger.L.Debug($"Use server image cache: {serverImage.Url}");
                var imgMsgId = await bot.ReplyServerFileMessage(input, serverImage.Url, FileType.Image);
                if (string.IsNullOrEmpty(imgMsgId))
                    error = _hints.SendErrorHint;
                else
                    await ActionLog.Log(Name, input, serverImage.Url);
            }
            else
            {
                // TODO
            }
        }

        await bot.ReplyTextMessage(input, Sentence.GetOne("underDevelopment"));
        return Executed;
    }
    
}