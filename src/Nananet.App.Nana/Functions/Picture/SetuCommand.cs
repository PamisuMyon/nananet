using System.Text.RegularExpressions;
using Nananet.App.Nana.Commons;
using Nananet.App.Nana.Models;
using Nananet.Core;
using Nananet.Core.Commands;
using Nananet.Core.Models;
using Nananet.Core.Utils;

namespace Nananet.App.Nana.Functions.Picture;

public class SetuCommand : Command
{
    [Obsolete]
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
        new("来(点|电|份|张)(涩|瑟|色|美|帅)?图(片|图)? *　*(.*)"),
        new("來(點|電|份|張)(澀|瑟|色|美|帥)?圖(片|圖)? *　*(.*)"),
    };
    private PicCommandHints _hints = new()
    {
        DownloadingHint = "正在检索猫猫数据库，请博士耐心等待...",
        DownloadErrorHint = "图片被猫猫吞噬了，请博士稍后再试。",
        SendErrorHint = "图片被企鹅吞噬了，请博士稍后再试。"
    };

    protected readonly Spam _temporarySpam = new(0, 1, 5000);
    protected string _pixivProxy = "i.pixiv.re";
    protected PxKore _pxKore = new();


    public override async Task Init(IBot bot)
    {
        await base.Init(bot);
        bot.Config.TryGetExtraValue("pixivProxy", out _pixivProxy, "i.pixiv.re");
        bot.Config.TryGetExtraValue("storagePath", out string? storagePath);
        _pxKore = new PxKore(storagePath);
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

        // 不再读取配置，以下功能将会变动
        // 默认标签、排除标签、缺省标签 -> 摸了
        // 客户端id -> 服务器/频道/群组id or 私聊用户id
        // var config = bot.Config.GetChannel(input.ChannelId);
        // SetuConfig? setuConfig = null;
        // if (config != null
        //     && config.Wildcards != null
        //     && config.Wildcards.ContainsKey("setu"))
        // {
        //     setuConfig = JsonUtil.FromJson<SetuConfig>(config.Wildcards["setu"].ToString()!);
        // }
        //
        var options = new PxKore.IllustOptions();
        // if (setuConfig != null)
        // {
        //     options.ClientId = setuConfig.ClientId;
        //     options.Tags = setuConfig.Tags;
        //     options.ExcludedTags = setuConfig.ExcludedTags;
        //     options.FallbackTags = setuConfig.FallbackTags.SelectMany(i => i).ToArray();
        // }
        options.Proxy = _pixivProxy;
        // options.ClientId ??= input.IsPersonal ? input.AuthorId : input.GuildId;
        // options.ShouldRecord = true;
        options.IsRandomSample = false;
        options.AppendTotalSampleInfo = true;
        options.ReturnTotalSample = true;


        if (!string.IsNullOrEmpty(m.Groups[4].Value))
        {
            var keywords = m.Groups[4].Value.Replace('　', ' ').Trim().Split(' ');
            options.Tags = keywords;
        }
        // 无标签则随机采样 有标签则尝试走最新未看逻辑（根据客户端id检索该id未看过的图片）
        // if (options.Tags == null || options.Tags.Length == 0
        //     && setuConfig != null)
        // {
        //     options.Tags = setuConfig.Tags;
        //     options.IsRandomSample = true;
        // }

        // if (setuConfig != null)
        // {
        //     options.ExcludedTags = setuConfig.ExcludedTags;
        //     if (setuConfig.FallbackTags.Length > 0)
        //         options.FallbackTags = setuConfig.FallbackTags.RandomElem();
        // }

        var hintMsgId = await bot.ReplyTextMessage(input, _hints.DownloadingHint);

        // 请求PxKore服务
        var illust = await _pxKore.RequestIllust(options);
        string? error = null;
        if (illust != null)
        {
            // 服务器图片缓存 某些平台无法拿到服务器图片url
            // var serverImage = await ServerImage.GetOne(bot.AppSettings.Platform, illust.Value.FileName);
            // if (serverImage != null)
            // {
            //     Logger.L.Info($"Use server image cache: {serverImage.Url}");
            //     var imgMsgId = await bot.ReplyServerFileMessage(input, serverImage.Url, FileType.Image);
            //     if (string.IsNullOrEmpty(imgMsgId))
            //         error = _hints.SendErrorHint;
            //     else
            //         await ActionLog.Log(Name, bot, input, serverImage.Url);
            // }
            // else
            // await bot.ReplyServerFileMessage(input, illust.Value.Url);

            // 下载图片后上传
            var path = await _pxKore.Download(illust.Value.Url, illust.Value.FileName);
            if (path != null)
            {
                string? imgMsgId;
                // 对于文字和图片可同时存在的平台，图片与作品信息合为一条消息发送
                if (bot.AppSettings.Platform == Constants.PlatfromQQGuild) // hard-code
                    imgMsgId = await bot.SendMessage(new OutgoingMessage
                    {
                        Content = illust.Value.Info,
                        FileUri = path,
                        FileMode = OutgoingMessage.SendFileMode.Local,
                        ReferenceId = input.MessageId,
                        TargetId = input.ChannelId
                    });
                else
                    imgMsgId = await bot.ReplyLocalFileMessage(input, path, FileType.Image);

                if (imgMsgId.NullOrEmpty())
                    error = _hints.SendErrorHint;
                else
                    await ActionLog.Log(Name, bot, input, illust.Value.Url);
            }
            else
                error = _hints.DownloadErrorHint;

            // 发送作品信息
            if (error.NullOrEmpty()
                && illust.Value.Info.NotNullNorEmpty()
                && bot.AppSettings.Platform != Constants.PlatfromQQGuild) // hard-code
            {
                await Task.Delay(100);
                await bot.SendTextMessage(input.ChannelId, illust.Value.Info, input.IsPersonal, input.MessageId);
                await ActionLog.Log(Name, bot, input, illust.Value.Info);
            }
        }

        await bot.DeleteMessage(input.ChannelId, hintMsgId!);
        if (error.NotNullNorEmpty())
        {
            await Task.Delay(100);
            await bot.ReplyTextMessage(input, error!);
            await ActionLog.Log(Name, bot, input, error);
        }

        return Executed;
    }
}