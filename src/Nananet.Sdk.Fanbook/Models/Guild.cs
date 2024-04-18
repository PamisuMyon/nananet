namespace Nananet.Sdk.Fanbook.Models;

public class Guild
{
    public string GuildId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public List<string> ChannelLists { get; set; } = null!;
}