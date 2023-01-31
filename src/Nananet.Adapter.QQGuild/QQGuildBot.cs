using System.Text.RegularExpressions;
using Nananet.Core;
using Nananet.Core.Commands;
using Nananet.Core.Models;
using Nananet.Core.Storage;
using Nananet.Core.Utils;
using QQChannelFramework.Api;
using QQChannelFramework.Expansions.Bot;
using QQGuildMessage = QQChannelFramework.Models.MessageModels.Message;

namespace Nananet.Adapter.QQGuild;

public class QQGuildBot : IBot
{
    protected IStorage _storage;
    public IStorage Storage => _storage;
    public BotConfig Config => _storage.Config;
    protected AppSettings _appSettings;
    public AppSettings AppSettings => _appSettings;
    protected List<Command> _commands;
    protected Command.CommandPickFunc _pickFunc = Command.PickO1;
    protected Regex _mentionRegex;
    protected Regex _commandRegex = new("^[\\.。/]", RegexOptions.Multiline);
    protected Defender _defender;
    protected User _me;
    protected bool _isInitialized = false;

    protected QQChannelApi _qChannelApi;


    public QQGuildBot(InitOptions? options = default)
    {
        _storage = options?.Storage ?? new FileStorage();
        _commands = options?.Commands ?? new List<Command>();
    }

    public async Task Launch()
    {
        await Storage.Init();

        var appSettings = await Storage.GetAppSettings();
        if (appSettings == null)
        {
            Logger.L.Error("Options not available, launch failed.");
            return;
        }

        _appSettings = appSettings;

        // 声明鉴权信息
        var accessInfo = new OpenApiAccessInfo
        {
            BotAppId = _appSettings.AppId,
            BotToken = _appSettings.Token,
            BotSecret = _appSettings.Secret
        };

        _qChannelApi = new QQChannelApi(accessInfo);
        // 指定Api请求使用Bot身份
        _qChannelApi.UseBotIdentity();
        // 指定Api通道模式为沙盒模式 (测试时使用)
        if (_appSettings.IsDebug)
            _qChannelApi.UseSandBoxMode();

        var channelBot = new ChannelBot(_qChannelApi);
        // 指定是否为私域机器人 注册消息事件
        if (_appSettings.IsPrivate)
        {
            channelBot.UsePrivateBot();
            channelBot.RegisterUserMessageEvent();
        }
        else
        {
            channelBot.RegisterAtMessageEvent();
        }
        // channelBot.RegisterDirectMessageEvent();

        channelBot.OnConnected += () => { Logger.L.Info("Bot Connected."); };
        channelBot.AuthenticationSuccess += async () =>
        {
            Logger.L.Info("Authentication Succeeded.");
            if (_isInitialized) return;
            
            var user = await _qChannelApi.GetUserApi().GetCurrentUserAsync();
            _me = Converter.FromUser(user);
            
            // 初始化@正则
            _mentionRegex = new Regex($"[@＠]{_me.NickName}");
            Logger.L.Info($"Mention Regex: {_mentionRegex}");
            
            await Refresh();
            _isInitialized = true;
        };
        channelBot.OnError += ex => { Logger.L.Error($"Bot Error -> {ex.Message}"); };
        channelBot.ReceivedUserMessage += OnMessageReceived;
        channelBot.ReceivedAtMessage += OnMessageReceived;
        channelBot.ReceivedDirectMessage += OnMessageReceived;

        await channelBot.OnlineAsync();
        await Task.Delay(Timeout.Infinite);
    }

    public virtual async Task Refresh()
    {
        await Storage.RefreshBotConfig();
        await Storage.RefreshBlockList();
        _defender = new Defender(_storage, Config.Defender.Interval, Config.Defender.Threshold);
        foreach (var command in _commands)
        {
            await command.Init(this);
        }
    }

    protected void OnMessageReceived(QQGuildMessage input)
    {
        if (input.Author.IsBot) return;

        var msg = Converter.FromMessage(input);
        Logger.L.Info($"OnMessageReceived: {input}");
        Task.Run(async () => await ProcessMessage(msg));
    }

    protected async Task ProcessMessage(Message input)
    {
        if (_defender.IsBlocked(input.AuthorId))
        {
            Logger.L.Info($"Message form blocked user: {input.AuthorId}");
            return;
        }
        
        var isTriggered = false;
        var isChannelCommand = false;
        var isReplyMe = IsReplyMe(input);
        
        if (input.IsPersonal)
        {
            isTriggered = true;
            Logger.L.Info($"Direct message received: {input}");
        }
        else
        {
            // if (!Config.HasChannel(input.ChannelId)) return;
            Logger.L.Info($"Channel message received: {input}");

            if (isReplyMe)
            {
                isTriggered = true;
            }
            else if (input.HasContent())
            {
                // 使用@功能提到或手动输入@
                if (input.HasMentioned(_me.Id)
                    || _mentionRegex.IsMatch(input.OriginalContent))
                    isTriggered = true;
                // 使用指令
                else if (_commandRegex.IsMatch(input.Content))
                    isChannelCommand = true;
            }
        }
        
        if (!isTriggered && !isChannelCommand) return;
        input.Content = input.Content.Trim();
        if (input.Content.StartsWith("/"))
            input.Content = input.Content.Remove(0, 1);
        
        try
        {
            var testInfo = await _pickFunc(_commands, input, new CommandTestOptions
            {
                IsCommand = isChannelCommand
            });
            if (testInfo == null) return;

            var command = _commands.GetElemSafe(testInfo.Value.CommandIndex);
            if (command == null) return;
            Logger.L.Info($"Command executing: {command.Name}");
            var result = await command.Execute(this, input, testInfo.Value);
            if (result.Success)
            {
                _defender.Record(new User
                {
                    Id = input.AuthorId,
                    NickName = input.Author.NickName
                });
            }
            else
            {
                Logger.L.Error($"{command.Name} Failed.");
            }
        }
        catch (Exception e)
        {
            Logger.L.Error($"Command Error: {e.Message}");
            Logger.L.Error(e.StackTrace);
        }
        
    }
    
    public bool IsReplyMe(Message input)
    {
        if (!input.IsPersonal && input.Reference != null)
        {
            return input.Reference.AuthorId == _appSettings.BotId;
        }
        return false;
    }

    public async Task<string?> SendTextMessage(string targetId, string content, bool isPersonal, string? referenceId = null)
    {
        Logger.L.Info($"Sending text message to {targetId}: \n {content}");
        try
        {
            var result = await _qChannelApi.GetMessageApi().SendTextMessageAsync(targetId, content, referenceId ?? "");
            return result.Id;
        }
        catch (Exception e)
        {
            Logger.L.Error($"Sending text error: {e.Message}");
            Logger.L.Error(e.StackTrace);
        }
        return null;
    }

    public Task<string?> ReplyTextMessage(Message to, string content)
    {
        if (!to.IsPersonal)
        {
            // content = $"<@!{to.Author.Id}>" + content;   // @用户会产生提示消息
            content = $"@{to.Member.NickName} " + content;   // 避免打扰使用假@
        }
        return SendTextMessage(to.ChannelId, content, to.IsPersonal, to.MessageId);
    }

    public Task<string?> SendLocalFileMessage(string targetId, string filePath, bool isPersonal, string? referenceId = null, FileType fileType = FileType.File)
    {
        throw new NotImplementedException();
    }

    public Task<string?> ReplyLocalFileMessage(Message to, string filePath, FileType fileType = FileType.File)
    {
        throw new NotImplementedException();
    }

    public async Task<string?> SendServerFileMessage(string targetId, string url, bool isPersonal, string? referenceId = null, FileType fileType = FileType.File)
    {
        Logger.L.Info($"Sending image url message to {targetId}: \n {url}");
        // hard-code 重试次数
        var retryTimes = 7;
        do
        {
            try
            {
                var msg = await _qChannelApi.GetMessageApi().SendImageMessageAsync(targetId, url, referenceId);
                return msg.Id;
            }
            catch (Exception e)
            {
                Logger.L.Error($"Sending image url error: {e.Message}");
                Logger.L.Error(e.StackTrace);
                retryTimes--;
                Logger.L.Error($"Sending image url failed, retrying({retryTimes})");
                await Task.Delay(500);
            }
        } while (retryTimes >= 0);
        return null;
    }

    public Task<string?> ReplyServerFileMessage(Message to, string url, FileType fileType = FileType.File)
    {
        return SendServerFileMessage(to.ChannelId, url, to.IsPersonal, to.MessageId);
    }

    public async Task<bool> DeleteMessage(string? targetId, string messageId)
    {
        // 公域机器人暂不支持撤回消息
        // https://bot.q.qq.com/wiki/develop/api/openapi/message/delete_message.html
        if (!_appSettings.IsPrivate) return false;
        Logger.L.Info($"Deleting message: {messageId}");
        await _qChannelApi.GetMessageApi().RetractMessageAsync(targetId, messageId, true);
        return true;
    }
    
}