namespace Nananet.Core.Models;

public class Member
{
    public string? NickName { get; set; }
    public List<string> Roles { get; set; }

    public bool HasRole(string role)
    {
        return Roles.Contains(role);
    }

    public bool HasAnyRole(IEnumerable<string> roles)
    {
        return Roles.Any(roles.Contains);
    }
    
}