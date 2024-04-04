
namespace Nananet.Adapter.Fanbook.Models;

public class Member
{
    public string? Nick { get; set; }
    public List<string> Roles { get; set; } = null!;
    public object? GuildCard { get; set; }
    public int AssistLevel { get; set; }
}