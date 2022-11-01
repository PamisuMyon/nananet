namespace Nado.Core.Models;

public record BotConfig
{
    public bool ReplyDm { get; set; }

    public Dictionary<string, ChannelConfig> Channels { get; set; }

    public class ChannelConfig
    {
        public string Id { get; set; }
        public string? Name { get; set; }
    }

    public bool HasChannel(string channelId)
    {
        return Channels.ContainsKey("all") || Channels.ContainsKey(channelId);
    }

    public ChannelConfig? GetChannel(string channelId)
    {
        if (!Channels.ContainsKey(channelId)) return null;
        return Channels[channelId];
    }
    
    private static BotConfig? s_default;
    public static BotConfig Default
    {
        get
        {
            return s_default ??= new BotConfig
            {
                ReplyDm = true,
                Channels = new Dictionary<string, ChannelConfig>()
            };
        }
    }
    
}
