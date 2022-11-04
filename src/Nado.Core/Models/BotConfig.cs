
using System.Dynamic;
using Newtonsoft.Json.Linq;

namespace Nado.Core.Models;

public record BotConfig
{
    public bool ReplyDm { get; set; }
    
    public Dictionary<string, ChannelConfig> Channels { get; set; }

    public class ChannelConfig
    {
        public string ChannelId { get; set; }
        public string? Name { get; set; }
        public bool AlarmBirthday { get; set; }
        public JObject? Wildcards { get; set; }
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

    public DefenderConfig Defender { get; set; }
    
    public class DefenderConfig
    {
        public int Interval { get; set; }
        public int Threshold { get; set; }
    }
    
    public Dictionary<string, object>? Extra { get; set; }

    private static BotConfig? s_default;
    public static BotConfig Default
    {
        get
        {
            return s_default ??= new BotConfig
            {
                ReplyDm = true,
                Channels = new Dictionary<string, ChannelConfig>(),
                Defender = new DefenderConfig
                {
                    Interval = 1200,
                    Threshold = 12,
                }
            };
        }
    }
    
}
