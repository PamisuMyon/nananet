namespace Nananet.Sdk.Fanbook.Models.Params;

public class GetMessageListParam
{
    public int Size { get; set; }
    public string ChannelId { get; set; } = null!;
    public string? MessageId { get; set; }
    public string Behavior { get; set; } = null!;
    public string Transaction { get; set; } = null!;
}