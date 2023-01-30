using System.Text.RegularExpressions;
using Kook;
using Kook.WebSocket;
using Nananet.Core;
using Nananet.Core.Commands;
using Nananet.Core.Models;
using Nananet.Core.Storage;
using Nananet.Core.Utils;

namespace Nananet.Adapter.Kook;

public class KookBot : IBot
{
    protected IStorage _storage;
    public IStorage Storage => _storage;
    public BotConfig Config => _storage.Config;
    protected AppSettings _appSettings;
    public AppSettings AppSettings => _appSettings;
    protected List<Command> _commands;
    protected Command.CommandPickFunc _pickFunc = Command.PickO1;
    protected Regex _mentionRegex;
    protected Regex _commandRegex = new("^[\\.。]", RegexOptions.Multiline);
    protected Defender _defender;
    protected User _me;

    protected KookSocketClient _client;

    public KookBot(InitOptions? options = default)
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

        var config = new KookSocketConfig { MessageCacheSize = 100 };
        _client = new KookSocketClient(config);
        _client.Log += Log;
        await _client.LoginAsync(TokenType.Bot, _appSettings.Token);
        await _client.StartAsync();
        _client.Ready += async () =>
        {
            Logger.L.Info("Bot connected.");
            
            var user = _client.Rest.CurrentUser;
            _me = new User
            {
                Id = user.Id.ToString(),
                NickName = user.Username
            };

            _mentionRegex = new Regex($"({Regex.Escape(user.KMarkdownMention)}|[@＠]{_me.NickName}#{user.IdentifyNumber}|[@＠]{_me.NickName})",
                RegexOptions.IgnoreCase | RegexOptions.Multiline);
            Logger.L.Info($"Mention Regex: {_mentionRegex}");
            await Refresh();
        };
        _client.MessageReceived += OnMessageReceived;

        await Task.Delay(Timeout.Infinite);
    }

    private Task Log(LogMessage msg)
    {
        Logger.L.Debug(msg);
        return Task.CompletedTask;
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

    protected Task OnMessageReceived(SocketMessage input)
    {
        Logger.L.Debug($"OnMessageReceived: {input}");
        var message = Converter.FromSocketMessage(input);
        if (message == null || message.Author.IsBot) return Task.CompletedTask;
        Task.Run(async () => await ProcessMessage(message));
        return Task.CompletedTask;
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
            Logger.L.Info($"Personal message received: {input}");
        }
        else
        {
            // if (!Config.HasChannel(input.ChannelId)) return;
            if (isReplyMe)
            {
                isTriggered = true;
            }
            else if (input.HasContent())
            {
                if (_mentionRegex.IsMatch(input.OriginalContent))
                    isTriggered = true;
                else if (_commandRegex.IsMatch(input.Content))
                    isChannelCommand = true;
            }
            
            if (isTriggered || isChannelCommand)
                Logger.L.Info($"Channel message received: {input}");
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
            Logger.L.Error("Command Error.");
            Logger.L.Error(e);
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

    protected virtual void CommandPreTest(Message input)
    {
        if (input.HasContent())
        {
            input.Content = _mentionRegex.Replace(input.Content, "");
            input.Content = _commandRegex.Replace(input.Content, "");
            input.Content = input.Content.Replace("@#", "");
            input.Content = input.Content.Trim();
            Logger.L.Debug($"Pre test content: {input.Content}");
        }
    }

    public async Task<string?> SendTextMessage(string targetId, string content, bool isPersonal,
        string? referenceId = null)
    {
        var channel = await _client.GetChannelAsync(ulong.Parse(targetId));
        if (channel is SocketTextChannel textChannel)
        {
            var result = await textChannel.SendTextAsync(content,
                referenceId != null ? new Quote(Guid.Parse(referenceId)) : null);
            return result.Id.ToString();
        }

        return null;
    }

    public async Task<string?> ReplyTextMessage(Message to, string content)
    {
        if (to.Origin is SocketMessage socketMessage)
        {
            var result = await socketMessage.Channel.SendTextAsync(content, new Quote(socketMessage.Id));
            return result.Id.ToString();
        }
        return await SendTextMessage(to.ChannelId, content, to.IsPersonal, to.MessageId);
    }

    public async Task<string?> SendLocalFileMessage(string targetId, string filePath, bool isPersonal,
        string? referenceId = null, FileType fileType = FileType.File)
    {
        var channel = await _client.GetChannelAsync(ulong.Parse(targetId));
        if (channel is SocketTextChannel textChannel)
        {
            var result = await textChannel.SendFileAsync(filePath, null,
                Converter.ToAttachmentType(fileType),
                referenceId != null ? new Quote(Guid.Parse(referenceId)) : null);
            return result.Id.ToString();
        }
        return null;
    }

    public async Task<string?> ReplyLocalFileMessage(Message to, string filePath, FileType fileType = FileType.File)
    {
        if (to.Origin is SocketMessage socketMessage)
        {
            var result = await socketMessage.Channel.SendFileAsync(filePath, null, Converter.ToAttachmentType(fileType),
                new Quote(socketMessage.Id));
            return result.Id.ToString();
        }
        return await SendLocalFileMessage(to.ChannelId, filePath, to.IsPersonal, to.MessageId, fileType);
    }

    public async Task<string?> SendServerFileMessage(string targetId, string url, bool isPersonal, string? referenceId = null, FileType fileType = FileType.File)
    {
        var channel = await _client.GetChannelAsync(ulong.Parse(targetId));
        if (channel is SocketTextChannel textChannel)
        {
            var attachment = new FileAttachment(new Uri(url), null, Converter.ToAttachmentType(fileType));
            var result = await textChannel.SendFileAsync(attachment,
                referenceId != null ? new Quote(Guid.Parse(referenceId)) : null);
            return result.Id.ToString();
        }
        return null;
    }

    public async Task<string?> ReplyServerFileMessage(Message to, string url, FileType fileType = FileType.File)
    {
        if (to.Origin is SocketMessage socketMessage)
        {
            var attachment = new FileAttachment(new Uri(url), null, Converter.ToAttachmentType(fileType));
            var result = await socketMessage.Channel.SendFileAsync(attachment, new Quote(socketMessage.Id));
            return result.Id.ToString();
        }
        return await SendServerFileMessage(to.ChannelId, url, to.IsPersonal, to.MessageId, fileType);
    }

    public async Task<bool> DeleteMessage(string? targetId, string messageId)
    {
        var channel = await _client.GetChannelAsync(ulong.Parse(targetId!));
        if (channel is SocketTextChannel textChannel)
        {
            await textChannel.DeleteMessageAsync(Guid.Parse(messageId));
            return true;
        }
        return false;
    }
    
}