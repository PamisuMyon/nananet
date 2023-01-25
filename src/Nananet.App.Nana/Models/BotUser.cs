using MongoDB.Entities;

namespace Nananet.App.Nana.Models;

[Collection("users")]
public class BotUser : Entity
{
    public const string RoleAdmin = "admin";
    public const string RoleModerator = "moderator";
    public const string RoleSensei = "sensei";
    
    public string UserId { get; set; }
    public string UserName { get; set; }
    public string NickName { get; set; }
    public string Role { get; set; }
    public string Platform { get; set; }

    public static Task<BotUser?> FindById(string id)
    {
        return DB.Find<BotUser?>()
            .Match(d => d!.UserId == id)
            .ExecuteFirstAsync();
    }
}