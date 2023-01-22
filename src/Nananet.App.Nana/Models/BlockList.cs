using MongoDB.Entities;
using Nananet.Core.Models;

namespace Nananet.App.Nana.Models;

[Collection("block-list")]
public class BlockedUser : Entity, ICreatedOn
{
    public string UserId { get; set; }
    public string UserName { get; set; }
    public string Name { get; set; }
    public DateTime CreatedOn { get; set; }

    public static implicit operator User(BlockedUser blockedUser)
    {
        return new User
        {
            Id = blockedUser.UserId,
            UserName = blockedUser.UserName,
            NickName = blockedUser.Name
        };
    }
    
}