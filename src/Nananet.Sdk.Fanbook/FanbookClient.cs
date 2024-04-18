using System.Text;
using COSXML.Model.Bucket;
using Nananet.Core.Utils;
using Nananet.Sdk.Fanbook.Api;
using Nananet.Sdk.Fanbook.Models;
using Nananet.Sdk.Fanbook.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SixLabors.ImageSharp;

namespace Nananet.Sdk.Fanbook;

// TODO 断线重连

public class FanbookClient
{
    private const string Tag = "FanbookClient";
    private const int HeartbeatInterval = 25000;

    private WebSocketHandler _wsHandler;
    private string? _wsUrl;
    private RestHandler _restHandler;
    private CancellationTokenSource? _ctsHeartbeat;
    private bool _isReady;
    private long _actionSeq = 0;
    private Dictionary<string, string> _channelGuildMap = new();
    
    public ClientRuntimeData RuntimeData { get; private set; }
    public UserApi UserApi { get; private set; }
    public MessageApi MessageApi { get; private set; }
    public FileApi FileApi { get; private set; }
    public GuildApi GuildApi { get; private set; }
    
    public event Action? Ready;
    public event Action<Message>? MessageReceived;
    
    public FanbookClient(ClientConfig config)
    {
        var tempId = Guid.NewGuid().ToString();
        var xspJson = $@"{{""platform"":""web"",""version"":""{config.AppVersion}"",""device_id"":""{config.DeviceId}"",""build_number"":""{config.BuildNumber}""}}";
        var xsp = Convert.ToBase64String(Encoding.UTF8.GetBytes(xspJson));
        RuntimeData = new ClientRuntimeData(config, tempId, xsp);
        
        _wsHandler = new WebSocketHandler();
        _wsHandler.MessageReceived += OnMessageReceived;
        _wsHandler.Disconnected += OnDisconnected;

        _restHandler = new RestHandler(RuntimeData);
        UserApi = new UserApi(_restHandler);
        MessageApi = new MessageApi(_restHandler);
        FileApi = new FileApi(_restHandler);
        GuildApi = new GuildApi(_restHandler);
    }

    public async Task SetBrowserContextAsync(string guildId, string channelId)
    {
        RuntimeData.CurrentGuildId = guildId;
        RuntimeData.CurrentChannelId = channelId;
        // if (_isReady)
        // {
        //     await RecordCurrentChannelLastMessageAsync();
        //     if (!string.IsNullOrEmpty(RuntimeData.CurrentChannelLastMessageId))
        //         await ReadMessageAsync(RuntimeData.CurrentGuildId, RuntimeData.CurrentChannelId, RuntimeData.CurrentChannelLastMessageId);
        // }
    }

    private async Task RecordCurrentChannelLastMessageAsync()
    {
        if (RuntimeData.CurrentGuildId == null || RuntimeData.CurrentChannelId == null)
        {
            Logger.L.Error("Please set browser context first.");
            return;
        }
        var messages = await MessageApi.GetListAsync(RuntimeData.CurrentChannelId);
        if (messages != null && messages.Count > 0)
        {
            RuntimeData.CurrentChannelLastMessageId = messages[0].MessageId;
        }
    }

    public async Task RefreshGuildInfoAsync()
    {
        if (!_isReady)
            return;
        var guilds = await GuildApi.GetMyGuildsAsync();
        if (guilds != null)
        {
            for (var i = 0; i < guilds.Count; i++)
            {
                for (var j = 0; j < guilds[i].ChannelLists.Count; j++)
                {
                    _channelGuildMap[guilds[i].ChannelLists[j]] = guilds[i].GuildId;
                }
            }
        }
    }

    public async Task<string?> GetGuildIdByChannelIdAsync(string channelId)
    {
        if (_channelGuildMap.TryGetValue(channelId, out var guildId))
            return guildId;
        await RefreshGuildInfoAsync();
        if (_channelGuildMap.TryGetValue(channelId, out guildId))
            return guildId;
        return null;
    } 

    /// <summary>
    /// 启动流程：
    /// 1. ws连接成功
    /// 2. ws收到connect  回复init
    /// 3. ws收到init 获取到client_id，开启心跳，rest拉取当前频道消息列表，最后一条消息读三次
    /// </summary>
    public async Task StartAsync()
    {
        _wsUrl = $"wss://web-gw.fanbook.cn/?dId={RuntimeData.Config.DeviceId}&id={RuntimeData.Config.Token}&tId={RuntimeData.TempId}&v={RuntimeData.Config.AppVersion}&x-super-properties={RuntimeData.Xsp}";
        Logger.L.Debug($"{Tag} Connecting to URL: {_wsUrl}");
        await _wsHandler.ConnectAsync(_wsUrl, GetWsHeaders());
    }

    private Dictionary<string, string> GetWsHeaders()
    {
        return new Dictionary<string, string>()
        {
            { "Host", "web-gw.fanbook.cn" },
            { "Connection", "Upgrade" },
            { "Pragma", "no-cache" },
            { "Cache-Control", "no-cache" },
            { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36 Edg/123.0.0.0" },
            { "Upgrade", "websocket" },
            { "Origin", "https://web.fanbook.cn" },
            { "Accept-Encoding", "gzip, deflate, br, zstd" },
            { "Accept-Language", "zh-CN,zh;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6" },
            // { "Sec-WebSocket-Key", "n+nz5DzYdAcfR/5PaVEXRw==" },
            // { "Sec-WebSocket-Extensions", "permessage-deflate; client_max_window_bits" },
        };
    }
    
    private void OnMessageReceived(string data)
    {
        try
        {
            var jo = JObject.Parse(data);
            var action = jo["action"]?.Value<string>();
            if (!string.IsNullOrEmpty(action))
            {
                HandleAction(action, jo);
            }
        }
        catch (Exception ex)
        {
            Logger.L.Error($"{Tag} OnMessageReceived Error:");
            Logger.L.Error(ex);
        }
    }
    
    private void OnDisconnected()
    {
        Logger.L.Error("Websocket disconnected, reconnecting...");
        Reconnect();
    }

    private async void Reconnect()
    {
        if (_wsUrl != null)
        {
            _wsHandler.Close();
            await Task.Delay(2000);
            await _wsHandler.ConnectAsync(_wsUrl, GetWsHeaders());
        }
    }

    private async void HandleAction(string action, JObject jo)
    {
        if (action == "connect")
        {
            var resJo = new JObject();
            resJo["action"] = "init";
            resJo["seq"] = _actionSeq++;
            resJo["app_version"] = RuntimeData.Config.AppVersion;
            await _wsHandler.SendAsync(resJo.ToString(Formatting.None));
        } 
        else if (action == "init")
        {
            if (jo.TryGetValue("data", out var data)
                && data is JObject dataJo
                && dataJo.TryGetValue("client_id", out var clientId))
            {
                RuntimeData.ClientId = clientId.Value<string>();
                
                _ctsHeartbeat?.Cancel();
                _ctsHeartbeat = new CancellationTokenSource();
                StartHeartbeat(_ctsHeartbeat.Token);

                _isReady = true;
                
                // 获取当前频道最后一条消息并读三次
                await RecordCurrentChannelLastMessageAsync();
                if (!string.IsNullOrEmpty(RuntimeData.CurrentGuildId)
                    && !string.IsNullOrEmpty(RuntimeData.CurrentChannelId)
                    && !string.IsNullOrEmpty(RuntimeData.CurrentChannelLastMessageId))
                {
                    await ReadMessageAsync(RuntimeData.CurrentGuildId, RuntimeData.CurrentChannelId, RuntimeData.CurrentChannelLastMessageId);
                    await ReadMessageAsync(RuntimeData.CurrentGuildId, RuntimeData.CurrentChannelId, RuntimeData.CurrentChannelLastMessageId);
                    await ReadMessageAsync(RuntimeData.CurrentGuildId, RuntimeData.CurrentChannelId, RuntimeData.CurrentChannelLastMessageId);
                }
                
                // 获取当前所有服务器信息，构建频道-服务器映射
                await RefreshGuildInfoAsync();
                
                Ready?.Invoke();
            }
        }
        else if (action == "push")
        {
            // Console.WriteLine("Push Message: \n" + jo["data"]);
            if (jo.TryGetValue("data", out var data))
            {
                await ParsePushMessageAsync(data.ToString());
            }
        }
        // else if (action == "pong")
        // {
        //     if (!_isFirstPongHandled)
        //     {
        //         _isFirstPongHandled = true;
        //         await ReadMessageAsync("616200178189582337", "616200178307022848", "616856931931639808");
        //         await ReadMessageAsync("616200178189582337", "616200178307022848", "616856931931639808");
        //         await ReadMessageAsync("616200178189582337", "616200178307022848", "616856931931639808");
        //     }
        // }
    }

    // private bool _isFirstPongHandled = false; 

    private void StartHeartbeat(CancellationToken cancellationToken)
    {
        Task.Run(() => SendHeartbeatAsync(cancellationToken));
    }

    private async Task SendHeartbeatAsync(CancellationToken cancellationToken)
    {
        const string ping = @"{""type"":""ping""}";
        while (!cancellationToken.IsCancellationRequested)
        {
            await _wsHandler.SendAsync(ping);
            await Task.Delay(HeartbeatInterval);
        }
    }

    private async Task ParsePushMessageAsync(string data)
    {
        try
        {
            var message = _restHandler.FromJson<Message>(data);
            if (message != null)
            {
                if (!string.IsNullOrEmpty(message.Content))
                {
                    var contentJo = JObject.Parse(message.Content);
                    if (contentJo.TryGetValue("type", out var typeToken))
                    {
                        var type = typeToken.Value<string>();
                        if (type == "text")
                        {
                            message.ContentType = EContentType.Text;
                            message.TextContent = _restHandler.FromJson<TextContent>(message.Content);
                        }
                    }
                }

                if (RuntimeData.CurrentGuildId == message.GuildId 
                    && RuntimeData.CurrentChannelId == message.ChannelId)
                {
                    await ReadMessageAsync(message.GuildId, message.ChannelId, message.MessageId);
                }
                MessageReceived?.Invoke(message);
            }
        }
        catch (Exception ex)
        {
            Logger.L.Error(ex);
        }
    }
    
    public async Task ReadMessageAsync(string guildId, string channelId, string messageId)
    {
        var resJo = new JObject();
        resJo["action"] = "upLastRead";
        resJo["guild_id"] = guildId;
        resJo["channel_id"] = channelId;
        resJo["read_id"] = messageId;
        resJo["seq"] = _actionSeq++;
        resJo["app_version"] = RuntimeData.Config.AppVersion;
        await _wsHandler.SendAsync(resJo.ToString(Formatting.None));
    }

    public async Task<string?> SendTextMessageAsync(string channelId, string text, string? quoteL1 = null, string? quoteL2 = null)
    {
        if (!_isReady) return null;
        var guildId = await GetGuildIdByChannelIdAsync(channelId);
        if (guildId == null)
        {
            Logger.L.Error($"Guild of channel {channelId} not found.");
            return null;
        }
        var textContent = new TextContent(text);
        var contentJson = _restHandler.ToJson(textContent);
        var result = await MessageApi.ClientSendAsync(guildId, channelId, contentJson, text, quoteL1, quoteL2);
        if (result?.Status == true)
            return result.Data?.MessageId;
        return null;
    }

    public async Task<string?> SendLocalImageMessageAsync(string channelId, string filePath)
    {
        if (!_isReady) return null;
        var guildId = await GetGuildIdByChannelIdAsync(channelId);
        if (guildId == null)
        {
            Logger.L.Error($"Guild of channel {channelId} not found.");
            return null;
        }
        
        if (!File.Exists(filePath))
        {
            Logger.L.Error("SendImageMessageAsync file not exists.");
            return null;
        }
        
        var imageUrl = await FileApi.UploadImageAsync(filePath);
        if (string.IsNullOrEmpty(imageUrl))
        {
            Logger.L.Error("SendImageMessageAsync upload failed.");
            return null;
        }
        
        using var image = Image.Load(filePath);
        var fileInfo = new FileInfo(filePath);
        
        var imageContent = new ImageContent
        {
            Url = imageUrl,
            Width = image.Width,
            Height = image.Height,
            Size = fileInfo.Length,
            LocalFilePath = "",
            LocalIdentify = Path.GetFileName(filePath)
        };
        var contentJson = _restHandler.ToJson(imageContent);
        var result = await MessageApi.ClientSendAsync(guildId, channelId, contentJson, "[图片]");
        if (result?.Status == true)
            return result.Data?.MessageId;
        return null;
    }

    public async Task<string?> SendServerImageMessageAsync(string channelId, string fileUrl)
    {
        if (!_isReady) return null;
        var guildId = await GetGuildIdByChannelIdAsync(channelId);
        if (guildId == null)
        {
            Logger.L.Error($"Guild of channel {channelId} not found.");
            return null;
        }
        
        var imageContent = new ImageContent
        {
            Url = fileUrl,
            Width = 128,
            Height = 128,
            Size = 28184,
            LocalFilePath = "",
            LocalIdentify = ""
        };
        var contentJson = _restHandler.ToJson(imageContent);
        var result = await MessageApi.ClientSendAsync(guildId, channelId, contentJson, "[图片]");
        if (result?.Status == true)
            return result.Data?.MessageId;
        return null;
    }

    public Task<bool> RecallMessageAsync(string channelId, string messageId)
    {
        if (!_isReady) return Task.FromResult(false);
        return MessageApi.RecallAsync(channelId, messageId);
    }
    
}
