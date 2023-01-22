﻿using System.Text.RegularExpressions;
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
    protected List<Command> _commands;
    protected Command.CommandPickFunc _pickFunc = Command.PickO1;
    protected Regex _mentionRegex;
    protected Regex _commandRegex = new("^[\\.。/]", RegexOptions.Multiline);
    protected Defender _defender;
    protected User _me;

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
        channelBot.UsePrivateBot();
        channelBot.RegisterUserMessageEvent();
        // channelBot.RegisterAtMessageEvent();
        // channelBot.RegisterDirectMessageEvent();
        channelBot.OnConnected += () => { Logger.L.Info("Bot Connected."); };
        channelBot.AuthenticationSuccess += async () =>
        {
            Logger.L.Info("Authentication Succeeded.");

            var user = await _qChannelApi.GetUserApi().GetCurrentUserAsync();
            _me = Converter.FromUser(user);
            
            // 初始化@正则
            _mentionRegex = new Regex($"[@＠]{_me.NickName}");
            Logger.L.Info($"Mention Regex: {_mentionRegex}");
            
            await Refresh();
        };
        channelBot.OnError += ex => { Logger.L.Error($"Bot Error -> {ex.Message}"); };
        channelBot.ReceivedUserMessage += OnMessageReceived;
        // channelBot.ReceivedAtMessage += OnMessageReceived;
        // channelBot.ReceivedDirectMessage += OnMessageReceived;

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
        Logger.L.Debug($"OnMessageReceived: {input}");
        Task.Run(async () => await ProcessMessage(msg));
    }

    protected async Task ProcessMessage(Message input)
    {
        if (_defender.IsBlocked(input.AuthorId))
        {
            Logger.L.Debug($"Message form blocked user: {input.AuthorId}");
            return;
        }
        
        var isTriggered = false;
        var isChannelCommand = false;
        var isReplyMe = IsReplyMe(input);
        
        if (input.IsPersonal)
        {
            isTriggered = true;
            Logger.L.Debug($"Direct message received: {input}");
        }
        else
        {
            // if (!Config.HasChannel(input.ChannelId)) return;
            Logger.L.Debug($"Channel message received: {input}");

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
        
        try
        {
            var testInfo = await _pickFunc(_commands, input, new CommandTestOptions
            {
                IsCommand = isChannelCommand
            });
            if (testInfo == null) return;

            var command = _commands.GetElemSafe(testInfo.Value.CommandIndex);
            if (command == null) return;
            Logger.L.Debug($"Command executing: {command.Name}");
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
        var result = await _qChannelApi.GetMessageApi().SendTextMessageAsync(targetId, content, referenceId ?? "");
        return result.Id;
    }

    public Task<string?> ReplyTextMessage(Message to, string content)
    {
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

    public Task<string?> SendServerFileMessage(string targetId, string url, bool isPersonal, string? referenceId = null, FileType fileType = FileType.File)
    {
        throw new NotImplementedException();
    }

    public Task<string?> ReplyServerFileMessage(Message to, string url, FileType fileType = FileType.File)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteMessage(string? targetId, string messageId)
    {
        throw new NotImplementedException();
    }
    
}