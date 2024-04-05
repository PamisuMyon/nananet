using Nananet.Adapter.Fanbook.Api;
using Nananet.Adapter.Fanbook.Models;

namespace Nananet.Adapter.Fanbook;

public class FanbookClient
{
    private const string Tag = "FanbookClient";

    private RestHandler _restHandler; 
    
    public BotApi BotApi { get; private set; }
    public MessageApi MessageApi { get; private set; }
    public ChannelApi ChannelApi { get; private set; }
        
    public event Action? Ready;
    public event Action<Message>? MessageReceived;
    
    public FanbookClient(string token)
    {
        _restHandler = new RestHandler(token);
        BotApi = new BotApi(_restHandler);
        MessageApi = new MessageApi(_restHandler);
        ChannelApi = new ChannelApi(_restHandler);
    }

}
