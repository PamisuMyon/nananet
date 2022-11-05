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
    protected List<Command> _commands;
    protected Command.CommandPickFunc _pickFunc = Command.PickO1;
    protected Regex _mentionRegex;
    protected Regex _commandRegex = new Regex("^[\\.。]", RegexOptions.Multiline);
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
                UserId = user.Id.ToString(),
                NickName = user.Username
            };
            
            _mentionRegex = new Regex($"({user.KMarkdownMention}|[@＠]{_me.NickName})", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            Logger.L.Debug($"Mention Regex: {_mentionRegex}");
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
        if (message == null) return Task.CompletedTask;
        Task.Run(async () => await ProcessMessage(message));
        return Task.CompletedTask;
    }

    protected async Task ProcessMessage(Message input)
    {
        if (input.Author.IsBot) return;
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
            Logger.L.Debug($"Personal message received: {input}");
        }
        else
        {
            if (!Config.HasChannel(input.ChannelId)) return;
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
            Logger.L.Debug($"Command executing: {command.Name}");
            var result = await command.Execute(this, input, testInfo.Value);
            if (result.Success)
            {
                _defender.Record(new User
                {
                    UserId = input.AuthorId,
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
        if (input is TextMessage text)
        {
            text.Content = _mentionRegex.Replace(text.Content, "");
            text.Content = _commandRegex.Replace(text.Content, "");
            text.Content = text.Content.Trim();
        }
    }
    
    public Task<string?> SendMessage<T>(string targetId, T messageBody, bool isPersonal, string? referenceId = null)
    {
        throw new NotImplementedException();
    }

    public async Task<string?> SendTextMessage(string targetId, string content, bool isPersonal, string? referenceId = null)
    {
        var channel = await _client.GetChannelAsync(ulong.Parse(targetId));
        if (channel is SocketTextChannel textChannel)
        {
            await textChannel.SendTextAsync(content, referenceId != null ? new Quote(Guid.Parse(referenceId)) : null);
        }
        return null;
    }

    public Task<string?> ReplyTextMessage(Message to, string content)
    {
        return SendTextMessage(to.ChannelId, content, to.IsPersonal, to.MessageId);
    }

    public Task<string?> SendPictureFileMessage(string targetId, string filePath, bool isPersonal, string? referenceId = null)
    {
        throw new NotImplementedException();
    }

    public Task<string?> ReplyPictureFileMessage(Message to, string filePath)
    {
        throw new NotImplementedException();
    }

    public Task<string?> SendPictureUrlMessage(string targetId, string url, bool isPersonal, string? referenceId = null)
    {
        throw new NotImplementedException();
    }

    public Task<string?> ReplyPictureUrlMessage(Message to, string url)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteMessage(string messageId)
    {
        throw new NotImplementedException();
    }
}