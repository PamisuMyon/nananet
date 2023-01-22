﻿namespace Nananet.Core.Models;

public class AppSettings
{
    /// <summary>
    /// Kook/QQ机器人 Token
    /// </summary>
    public string Token { get; set; }
    
    /// <summary>
    /// QQ机器人 AppID
    /// </summary>
    public string AppId { get; set; }
    
    /// <summary>
    /// QQ机器人 秘钥
    /// </summary>
    public string Secret { get; set; }
    
    /// <summary>
    /// QQ机器人 是否使用沙盒模式
    /// </summary>
    public bool IsDebug { get; set; }
    
    /// <summary>
    /// 手动配置的bot账号ID
    /// </summary>
    public string BotId { get; set; }

}