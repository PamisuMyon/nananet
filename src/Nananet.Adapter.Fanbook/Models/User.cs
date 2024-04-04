namespace Nananet.Adapter.Fanbook.Models;

public class User
{
    public string Nickname { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string Avatar { get; set; } = null!;
    public string? AvatarNft { get; set; }
    public bool Bot { get; set; }
}