using System.Text;

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
    private RestHandler _restHandler;
    private CancellationTokenSource? _ctsHeartbeat;
    private bool _isReady;
    private long _actionSeq = 0;
    
    public ClientRuntimeData RuntimeData { get; private set; }
    public MessageApi MessageApi { get; private set; }
    public FileApi FileApi { get; private set; }
    
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

        _restHandler = new RestHandler(RuntimeData);
        MessageApi = new MessageApi(_restHandler);
        FileApi = new FileApi(_restHandler);
    }

    public async Task SetBrowserContextAsync(string guildId, string channelId)
    {
        RuntimeData.CurrentGuildId = guildId;
        RuntimeData.CurrentChannelId = channelId;
        if (_isReady)
        {
            await RecordCurrentChannelLastMessageAsync();
            if (!string.IsNullOrEmpty(RuntimeData.CurrentChannelLastMessageId))
                await ReadMessageAsync(RuntimeData.CurrentGuildId, RuntimeData.CurrentChannelId, RuntimeData.CurrentChannelLastMessageId);
        }
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

    /// <summary>
    /// 启动流程：
    /// 1. ws连接成功
    /// 2. ws收到connect  回复init
    /// 3. ws收到init 获取到client_id，开启心跳，rest拉取当前频道消息列表，最后一条消息读三次
    /// </summary>
    public async Task StartAsync()
    {
        var wsUrl = $"wss://web-gw.fanbook.cn/?dId={RuntimeData.Config.DeviceId}&id={RuntimeData.Config.Token}&tId={RuntimeData.TempId}&v={RuntimeData.Config.AppVersion}&x-super-properties={RuntimeData.Xsp}";
        Logger.L.Debug($"{Tag} Connecting to URL: {wsUrl}");
        await _wsHandler.ConnectAsync(wsUrl, GetWsHeaders());
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
                
                await RecordCurrentChannelLastMessageAsync();
                if (!string.IsNullOrEmpty(RuntimeData.CurrentGuildId)
                    && !string.IsNullOrEmpty(RuntimeData.CurrentChannelId)
                    && !string.IsNullOrEmpty(RuntimeData.CurrentChannelLastMessageId))
                {
                    await ReadMessageAsync(RuntimeData.CurrentGuildId, RuntimeData.CurrentChannelId, RuntimeData.CurrentChannelLastMessageId);
                    await ReadMessageAsync(RuntimeData.CurrentGuildId, RuntimeData.CurrentChannelId, RuntimeData.CurrentChannelLastMessageId);
                    await ReadMessageAsync(RuntimeData.CurrentGuildId, RuntimeData.CurrentChannelId, RuntimeData.CurrentChannelLastMessageId);
                }
                
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

    public async Task SendTextMessageAsync(string guildId, string channelId, string text)
    {
        if (!_isReady) return;
        var textContent = new TextContent(text);
        var contentJson = _restHandler.ToJson(textContent);
        await MessageApi.ClientSendAsync(guildId, channelId, contentJson, text);
    }

    public async Task SendImageMessageAsync(string guildId, string channelId, string filePath)
    {
        if (!_isReady) return;
        if (!File.Exists(filePath))
        {
            Logger.L.Error("SendImageMessageAsync file not exists.");
            return;
        }
        
        var imageUrl = await FileApi.UploadImageAsync(filePath);
        if (string.IsNullOrEmpty(imageUrl))
        {
            Logger.L.Error("SendImageMessageAsync upload failed.");
            return;
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
        await MessageApi.ClientSendAsync(guildId, channelId, contentJson, "[图片]");
    }
    
    public void Debug()
    {
        RuntimeData.ClientId = "0a0504670b58001d6871";
        _isReady = true;
    }
    
}
