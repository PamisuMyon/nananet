namespace Nananet.Sdk.Fanbook.Models.Params;

public class ClientSendParam
{
    public string ChannelId { get; set; } = null!;
    public string GuildId { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string Desc { get; set; } = null!;
    public string Nonce { get; set; } = null!;
    public string Token { get; set; } = null!;
    public string Transaction { get; set; } = null!;
}
