namespace Nado.Core.Models;

public class BotConfig
{
    public bool ReplyDm { get; set; }

    public Dictionary<string, ChannelConfig> ChannelConfigs { get; set; }

    public class ChannelConfig
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
    
}
