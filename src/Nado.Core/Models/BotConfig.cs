namespace Nado.Core.Models;

public record BotConfig
{
    public bool ReplyDm { get; set; }

    public Dictionary<string, ChannelConfig> ChannelConfigs { get; set; }

    public class ChannelConfig
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    private static BotConfig? s_default;
    public static BotConfig Default
    {
        get
        {
            return s_default ??= new BotConfig
            {
                ReplyDm = true,
                ChannelConfigs = new Dictionary<string, ChannelConfig>()
            };
        }
    }
    
}
