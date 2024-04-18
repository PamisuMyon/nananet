namespace Nananet.Sdk.Fanbook.Models;

public class User
{
    // 从WS获取到可能为空，从REST获取不为空
    public string? UserId { get; set; }
    public string Nickname { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string Avatar { get; set; } = null!;
    public string? AvatarNft { get; set; }
    public bool Bot { get; set; }
}