using System.Text.RegularExpressions;
using Nananet.Core;
using Nananet.Core.Commands;
using Nananet.Core.Models;
using Nananet.Core.Storage;
using Nananet.Core.Utils;
using Nananet.Sdk.Fanbook;
using Nananet.Sdk.Fanbook.Models;
using Message = Nananet.Core.Models.Message;
using User = Nananet.Core.Models.User;

namespace Nananet.Adapter.Fanbook;

public class FanbookBot : IBot
{

    protected List<Command> Commands;
    protected Command.CommandPickFunc PickFunc = Command.PickO1;
    protected Regex MentionRegex;
    protected Regex CommandRegex = new("^[\\.。/]", RegexOptions.Multiline);
    protected Defender Defender;
    protected User Me;
    protected Queue<Message> MessageQueue = new();

    protected FanbookClient Client;
    
    public IStorage Storage { get; protected set; }
    public BotConfig Config => Storage.Config;
    public AppSettings AppSettings { get; protected set; }
    
    public FanbookBot(ClientConfig clientConfig, InitOptions? options = default)
    {
        Storage = options?.Storage ?? new FileStorage();
        Commands = options?.Commands ?? new List<Command>();
        Client = new FanbookClient(clientConfig);
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
        AppSettings = appSettings;

        Client.Ready += async () =>
        {
            var me = await Client.UserApi.GetMeAsync();
            if (me == null)
            {
                throw new Exception("Get me failed.");
            }
            Me = Converter.FromUser(me);
            // 初始化@正则
            MentionRegex = new Regex($"[@＠]{Me.NickName}");
            Logger.L.Info($"Mention Regex: {MentionRegex}");

            await Refresh();
            Client.MessageReceived += OnMessageReceived;
            Task.Run(ProcessMessageQueue);
        };
        
        // TODO hard-code
        await Client.SetBrowserContextAsync("616200178189582337", "616200820257841152");
        await Client.StartAsync();
        await Task.Delay(Timeout.Infinite);
    }

    private void OnMessageReceived(Sdk.Fanbook.Models.Message input)
    {
        // TODO TEMP
        if (input.GuildId != "616200178189582337")
            return;
        
        var msg = Converter.FromMessage(input);
        if (msg == null) return;
        
        Logger.L.Info($"OnMessageReceived: {msg}");
        // 考虑到目前为用户api，操作频率不能太高，用队列控制下
        MessageQueue.Enqueue(msg);
        // Task.Run(async () => await ProcessMessage(msg));
    }

    private async Task ProcessMessageQueue()
    {
        while (true)
        {
            if (MessageQueue.Count > 0)
            {
                var msg = MessageQueue.Dequeue();
                await ProcessMessage(msg);
            }
            await Task.Delay(500);
        }
    }
    
    protected async Task ProcessMessage(Message input)
    {
        if (Defender.IsBlocked(input.AuthorId))
        {
            Logger.L.Info($"Message form blocked user: {input.AuthorId}");
            return;
        }
        
        var isTriggered = false;
        var isChannelCommand = false;
        // var isReplyMe = IsReplyMe(input); // TODO
        var isReplyMe = false;
        
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
                if (input.HasMentioned(Me.Id)
                    || MentionRegex.IsMatch(input.OriginalContent))
                    isTriggered = true;
                // 使用指令
                else if (CommandRegex.IsMatch(input.Content))
                    isChannelCommand = true;
            }
        }
        
        if (!isTriggered && !isChannelCommand) return;
        input.Content = input.Content.Trim();
        if (input.Content.StartsWith("/"))
            input.Content = input.Content.Remove(0, 1);
        
        try
        {
            var testInfo = await PickFunc(Commands, input, new CommandTestOptions
            {
                IsCommand = isChannelCommand
            });
            if (testInfo == null) return;

            var command = Commands.GetElemSafe(testInfo.Value.CommandIndex);
            if (command == null) return;
            Logger.L.Info($"Command executing: {command.Name}");
            var result = await command.Execute(this, input, testInfo.Value);
            if (result.Success)
            {
                Defender.Record(new User
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

    public virtual async Task Refresh()
    {
        await Storage.RefreshBotConfig();
        await Storage.RefreshBlockList();
        Defender = new Defender(Storage, Config.Defender.Interval, Config.Defender.Threshold);
        foreach (var command in Commands)
        {
            await command.Init(this);
        }
    }

    public Task<string?> SendMessage(OutgoingMessage message)
    {
        throw new NotImplementedException();
    }

    public Task<string?> SendTextMessage(string targetId, string content, bool isPersonal, string? referenceId = null)
    {
        var splits = targetId.Split("/");
        return Client.SendTextMessageAsync(splits[0], splits[1], content);
    }

    public Task<string?> ReplyTextMessage(Message to, string content)
    {
        var targetId = $"{to.GuildId}/{to.ChannelId}";
        return SendTextMessage(targetId, content, false);
    }

    public Task<string?> SendLocalFileMessage(string targetId, string filePath, bool isPersonal, string? referenceId = null, FileType fileType = FileType.File)
    {
        var splits = targetId.Split("/");
        return Client.SendLocalImageMessageAsync(splits[0], splits[1], filePath);
    }

    public Task<string?> ReplyLocalFileMessage(Message to, string filePath, FileType fileType = FileType.File)
    {
        var targetId = $"{to.GuildId}/{to.ChannelId}";
        return SendLocalFileMessage(targetId, filePath, false, null, fileType);
    }

    public Task<string?> SendServerFileMessage(string targetId, string url, bool isPersonal, string? referenceId = null, FileType fileType = FileType.File)
    {
        var splits = targetId.Split("/");
        return Client.SendServerImageMessageAsync(splits[0], splits[1], url);
    }

    public Task<string?> ReplyServerFileMessage(Message to, string url, FileType fileType = FileType.File)
    {
        var targetId = $"{to.GuildId}/{to.ChannelId}";
        return SendServerFileMessage(targetId, url, false, null, fileType);
    }

    public Task<bool> DeleteMessage(string? targetId, string messageId)
    {
        if (targetId == null)
            return Task.FromResult(false);
        return Client.RecallMessageAsync(targetId, messageId);
    }
    
}