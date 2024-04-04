using Nananet.Adapter.Fanbook.Api;
using Nananet.Adapter.Fanbook.Models;
using Nananet.Adapter.Fanbook.WebSocket;
using Nananet.Core.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Nananet.Adapter.Fanbook;

public class FanbookClient
{
    private const string Tag = "FanbookClient";
    private const int HeartbeatInterval = 25000;

    private WebSocketHandler _wsHandler;
    private BaseApi _baseApi;
    private MessageApi _messageApi;
    private CancellationTokenSource? _ctsHeartbeat;
    private bool _isReady;
    
    public event Action? Ready;
    public event Action<Message>? MessageReceived;
    
    public FanbookClient(string token)
    {
        _wsHandler = new WebSocketHandler();
        _wsHandler.MessageReceived += OnMessageReceived;

        _baseApi = new BaseApi(token);
        _messageApi = new MessageApi(_baseApi);
    }

    public async Task StartAsync()
    {
        var wsUrl = $"wss://web-gw.fanbook.cn/?dId={_baseApi.DeviceId}&id={_baseApi.Token}&tId={_baseApi.TempId}&v={BaseApi.Version}&x-super-properties={_baseApi.Xsp}";
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
            resJo["seq"] = 0;
            resJo["app_version"] = BaseApi.Version;
            await _wsHandler.SendAsync(resJo.ToString(Formatting.None));
        } 
        else if (action == "init")
        {
            if (jo.TryGetValue("data", out var data)
                && data is JObject dataJo
                && dataJo.TryGetValue("client_id", out var clientId))
            {
                _baseApi.ClientId = clientId.Value<string>();
                
                _ctsHeartbeat?.Cancel();
                _ctsHeartbeat = new CancellationTokenSource();
                StartHeartbeat(_ctsHeartbeat.Token);

                _isReady = true;
                Ready?.Invoke();
            }
        }
        else if (action == "push")
        {
            Console.WriteLine("Push Message: \n" + jo["data"]);
            if (jo.TryGetValue("data", out var data))
            {
                ParsePushMessage(data.ToString());
            }
        }
    }

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

    private void ParsePushMessage(string data)
    {
        try
        {
            var message = _baseApi.FromJson<Message>(data);
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
                            message.TextContent = _baseApi.FromJson<TextContent>(message.Content);
                        }
                    }
                }
                MessageReceived?.Invoke(message);
            }
        }
        catch (Exception ex)
        {
            Logger.L.Error(ex);
        }
    }

    public async Task SendTextMessageAsync(string guildId, string channelId, string text)
    {
        if (!_isReady) return;
        var textContent = new TextContent(text);
        var contentJson = _baseApi.ToJson(textContent);
        await _messageApi.ClientSendAsync(guildId, channelId, contentJson, text);
    }

    public void Debug()
    {
        _baseApi.ClientId = "0a0504670b5500152139";
    }
    
}
