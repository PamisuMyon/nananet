﻿using System.Text.RegularExpressions;
using DoDo.Open.Sdk.Models;
using DoDo.Open.Sdk.Models.Bots;
using DoDo.Open.Sdk.Models.Channels;
using DoDo.Open.Sdk.Models.Messages;
using DoDo.Open.Sdk.Models.Personals;
using DoDo.Open.Sdk.Models.Resources;
using DoDo.Open.Sdk.Services;
using Nado.Core.Models;
using Nado.Core.Storage;
using Nado.Core.Utils;
using Nado.Core.Commands;

namespace Nado.Core;

public class NadoBot
{
    protected IStorage _storage;
    public IStorage Storage => _storage;
    public BotConfig Config => _storage.Config;

    protected OpenApiService _openApiService;
    public OpenApiService ApiService => _openApiService;
    protected NadoEventProcessService _eventProcessService;
    protected List<Command> _commands;
    protected GetBotInfoOutput _me;
    protected GetBotInfoOutput Me => _me;
    protected Regex _mentionRegex;
    protected Regex _commandRegex = new Regex("^[\\.。]", RegexOptions.Multiline);
    protected Command.CommandPickFunc _pickFunc = Command.PickO1;

    public NadoBot(InitOptions? options = default)
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

        _openApiService = new OpenApiService(new OpenApiOptions
        {
            BaseApi = appSettings.BaseApi,
            ClientId = appSettings.ClientId,
            Token = appSettings.Token,
        });
        _me = await _openApiService.GetBotInfoAsync(new GetBotInfoInput());
        if (_me == null)
        {
            Logger.L.Error("Get bot info failed.");
            return;
        }
        _mentionRegex = new Regex($"(<@!{_me.DodoId}>|[@＠]{_me.NickName})", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        Logger.L.Debug($"At Regex: {_mentionRegex}");

        await Refresh();
        
        _eventProcessService = new NadoEventProcessService(_openApiService);
        _eventProcessService.OnMessage += OnMessage;
        var openEventService = new OpenEventService(_openApiService, _eventProcessService, new OpenEventOptions
        {
            IsReconnect = true,
            IsAsync = true,
        });
        await openEventService.ReceiveAsync();
    }

    protected virtual async Task Refresh()
    {
        await Storage.RefreshBotConfig();
    }
    
    protected virtual async void OnMessage(Message input)
    {
        if (input.MessageType is not
            (Message.Type.Text or Message.Type.Picture)) return;

        // TODO Defender

        var isTriggered = false;
        var isChannelCommand = false;
        var isReplyMe = IsReplyMe(input);

        if (input.IsPersonal)
        {
            if (input.DodoId == _me.DodoId) return;
            isTriggered = true;
            Logger.L.Debug($"Personal message received: {input}");
        }
        else
        {
            if (!Config.HasChannel(input.ChannelId)) return;
            if (input.DodoId == _me.DodoId) return;
            Logger.L.Debug($"Channel message received: {input}");

            if (isReplyMe)
            {
                isTriggered = true;
            }
            else if (input is TextMessage text)
            {
                if (_mentionRegex.IsMatch(text.Content))
                    isTriggered = true;
                else if (_commandRegex.IsMatch(text.Content))
                    isChannelCommand = true;
            }
        }

        if (!isTriggered && !isChannelCommand) return;
        CommandPreTest(input);
        try
        {
            var testInfo = await _pickFunc(_commands, input, new CommandTestOptions
            {
                IsCommand = isChannelCommand
            });
            if (testInfo == null) return;

            var command = _commands.GetElemSafe(testInfo.Value.CommandIndex);
            if (command == null) return;
            var result = await command.Execute(this, input, testInfo.Value);
            if (result.Success)
            {
                // TODO Defender
            }
            else
            {
                Logger.L.Error($"{command.Name} Failed.");
            }
        }
        catch (Exception e)
        {
            Logger.L.Error("Command Error.");
            Logger.L.Error(e);
            Logger.L.Error(e.StackTrace);
        }
    }

    protected bool IsReplyMe(Message input)
    {
        if (!input.IsPersonal && input.Reference != null)
        {
            return input.Reference.DodoId == _me.DodoId;
        }
        return false;
    }

    protected virtual void CommandPreTest(Message input)
    {
        if (input is TextMessage text)
        {
            text.Content = _mentionRegex.Replace(text.Content, "");
            text.Content = _commandRegex.Replace(text.Content, "");
            text.Content = text.Content.Trim();
        }
    }

    public virtual async Task<string?> SendMessage<T>(string targetId, T messageBody, bool isPersonal, string? referenceId = null) where T : MessageBodyBase
    {
        if (isPersonal)
        {
            var result = await _openApiService.SetPersonalMessageSendAsync(new SetPersonalMessageSendInput<T>
            {
                DodoId = targetId,
                MessageBody = messageBody,
            });
            return result?.MessageId;
        }
        else
        {
            var result = await _openApiService.SetChannelMessageSendAsync(new SetChannelMessageSendInput<T>()
            {
                ChannelId = targetId,
                MessageBody = messageBody,
                ReferencedMessageId = referenceId
            });
            return result?.MessageId;
        }
    }

    public virtual async Task<string?> SendTextMessage(string targetId, string content, bool isPersonal, string? referenceId = null)
    {
        var body = new MessageBodyText
        {
            Content = content
        };
        return await SendMessage(targetId, body, isPersonal, referenceId);
    }

    public virtual async Task<string?> ReplyTextMessage(Message to, string content)
    {
        return await SendTextMessage(to.IsPersonal ? to.DodoId : to.ChannelId, content, to.IsPersonal, to.MessageId);
    }

    public virtual async Task<string?> SendPictureFileMessage(string targetId, string filePath, bool isPersonal, string? referenceId = null)
    {
        var imageResult = await _openApiService.SetResourcePictureUploadAsync(new SetResourceUploadInput
        {
            FilePath = filePath
        });
        if (imageResult == null) return null;

        var body = new MessageBodyPicture
        {
            Url = imageResult.Url,
            Width = imageResult.Width,
            Height = imageResult.Height,
            IsOriginal = 1,
        };
        return await SendMessage(targetId, body, isPersonal, referenceId);
    }

    public virtual async Task<string?> ReplyPictureFileMessage(Message to, string filePath)
    {
        return await SendPictureFileMessage(to.IsPersonal ? to.DodoId : to.ChannelId, filePath, to.IsPersonal, to.MessageId);
    }

    public virtual async Task<string?> SendPictureUrlMessage(string targetId, string url, bool isPersonal, string? referenceId = null)
    {
        var body = new MessageBodyPicture
        {
            Url = url,
            IsOriginal = 1,
        };
        return await SendMessage(targetId, body, isPersonal, referenceId);
    }
    
    public virtual async Task<string?> ReplyPictureUrlMessage(Message to, string url)
    {
        return await SendPictureUrlMessage(to.IsPersonal ? to.DodoId : to.ChannelId, url, to.IsPersonal, to.MessageId);
    }
    
}