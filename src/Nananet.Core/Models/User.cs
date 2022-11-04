namespace Nananet.Core.Models;

public class User
{
    public string UserId { get; set; }
    public string UserName { get; set; }
    public string NickName { get; set; }
    public string Avatar { get; set; }
    public bool IsBot { get; set; }
}