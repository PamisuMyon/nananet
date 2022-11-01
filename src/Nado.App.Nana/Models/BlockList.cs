using MongoDB.Entities;
using Nado.Core.Models;

namespace Nado.App.Nana.Models;

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
            UserId = blockedUser.UserId,
            UserName = blockedUser.UserName,
            Name = blockedUser.Name
        };
    }
    
}