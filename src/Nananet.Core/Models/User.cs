namespace Nananet.Core.Models;

public class User
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public string Id { get; set; } = null!;

    /// <summary>
    /// 用户名
    /// </summary>
    public string UserName { get; set; } = null!;

    /// <summary>
    /// 昵称
    /// </summary>
    public string NickName { get; set; } = null!;

    /// <summary>
    /// 头像Url
    /// </summary>
    public string Avatar { get; set; } = null!;

    /// <summary>
    /// 是否是机器人
    /// </summary>
    public bool IsBot { get; set; }
}