using System.Net.WebSockets;
using System.Text;
using Nananet.Core.Utils;

namespace Nananet.Adapter.Fanbook.WebSocket;

public class WebSocketHandler
{
    
    private ClientWebSocket? _webSocket;
    private byte[] _buffer = new byte[4096];
    private CancellationTokenSource? _cts;

    public event Action? Connected;
    public event Action? Disconnected;
    public event Action<string>? MessageReceived;

    public async Task ConnectAsync(string url, Dictionary<string, string>? headers = null)
    {
        Close();
        try
        {
            _webSocket = new ClientWebSocket();
            if (headers != null)
            {
                foreach (var pair in headers)
                {
                    _webSocket.Options.SetRequestHeader(pair.Key, pair.Value);
                }
            }
            _cts = new CancellationTokenSource();
            await _webSocket.ConnectAsync(new Uri(url), _cts.Token).ConfigureAwait(false);
            Connected?.Invoke();
            DoReceive(_cts.Token);
        }
        catch (Exception ex)
        {
            Logger.L.Error("WebSocketHandler connect error:");
            Logger.L.Error(ex);
        }
    }

    private void DoReceive(CancellationToken cancellationToken)
    {
        Task.Run(() => ReceiveAsync(cancellationToken));
    }

    private async Task ReceiveAsync(CancellationToken cancellationToken)
    {
        if (_cts == null || _webSocket == null)
            return;
        try
        {
            await using var ms = new MemoryStream();
            WebSocketReceiveResult? result = null;
            while (!cancellationToken.IsCancellationRequested)
            {
                result = await _webSocket.ReceiveAsync(_buffer, cancellationToken).ConfigureAwait(false);
                if (result.Count > 0)
                    ms.Write(_buffer, 0, result.Count);
                if (result.EndOfMessage)
                    break;
            }

            cancellationToken.ThrowIfCancellationRequested();
            if (result?.MessageType != WebSocketMessageType.Close)
            {
                var bytes = ms.ToArray();
                var data = Encoding.UTF8.GetString(bytes);
                if (data.Length > 0)
                {
                    Logger.L.Debug($"Message received: {data}");
                    MessageReceived?.Invoke(data);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.L.Error(ex);
        }
        finally
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                if (_webSocket?.State == WebSocketState.Open)
                {
                    DoReceive(cancellationToken);
                }
                else
                {
                    Disconnected?.Invoke();
                }
            }
        }
    }

    public async Task SendAsync(string data)
    {
        if (_webSocket == null || _cts == null)
        {
            Logger.L.Error("Not connected.");
            return;
        }
        if (!_cts.IsCancellationRequested &&
            _webSocket.State != WebSocketState.Open)
        {
            Logger.L.Error("The connection is closed.");
            return;
        }

        Logger.L.Debug($"Sending message: {data}");
        try
        {
            var bytes = Encoding.UTF8.GetBytes(data);
            await _webSocket.SendAsync(bytes, WebSocketMessageType.Text, true, _cts.Token).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.L.Error(ex);
        }
    }

    public void Close()
    {
        try
        {
            _cts?.Cancel();
            _webSocket?.Dispose();
            _cts?.Dispose();
        }
        finally
        {
            _cts = null;
            _webSocket = null;
        }
    }
    
}
