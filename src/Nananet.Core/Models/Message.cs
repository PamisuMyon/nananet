using System.Text;

namespace Nananet.Core.Models;

public class Message
{

    #region 通用
    
    /// <summary>
    /// 消息ID
    /// </summary>
    public string MessageId { get; set; } = null!;
    
    /// <summary>
    /// 群/服务器/主频道ID
    /// </summary>
    public string GuildId { get; set; } = null!;
    
    /// <summary>
    /// 子频道ID
    /// </summary>
    public string ChannelId { get; set; } = null!;

    /// <summary>
    /// 消息创建者ID
    /// </summary>
    public string AuthorId { get; set; } = null!;

    /// <summary>
    /// 消息创建者
    /// </summary>
    public User Author { get; set; } = null!;
    
    /// <summary>
    /// 消息创建者成员信息
    /// </summary>
    public Member Member { get; set; } = null!;

    /// <summary>
    /// 引用消息
    /// </summary>
    public MessageReference? Reference { get; set; }
    
    /// <summary>
    /// @的人
    /// </summary>
    public List<User>? Mentions { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime Time { get; set; }
    
    /// <summary>
    /// 编辑时间
    /// </summary>
    public DateTime EditedTime { get; set; }

    public bool HasMentioned(string userId)
    {
        if (Mentions == null) return false;
        return Mentions.Exists(user => user.Id == userId);
    }

    #endregion

    #region 自定义
    
    /// <summary>
    /// 消息类型
    /// </summary>
    public int MessageType { get; set; }
    
    /// <summary>
    /// 是否为私聊
    /// </summary>
    public bool IsPersonal { get; set; }

    /// <summary>
    /// 原始数据对象
    /// </summary>
    public object? Origin { get; set; }
    
    #endregion

    #region 文本

    /// <summary>
    /// 处理过的纯文本内容
    /// </summary>
    public string Content { get; set; } = null!;
    /// <summary>
    /// 原始内容
    /// </summary>
    public string RawContent { get; set; } = null!;
    /// <summary>
    /// 未处理的纯文本内容
    /// </summary>
    public string OriginalContent { get; set; } = null!;

    /// <summary>
    /// 消息是否包含文本内容
    /// </summary>
    public bool HasContent()
    {
        return !string.IsNullOrEmpty(RawContent);
    }
    
    #endregion

    #region 媒体

    /// <summary>
    /// 消息附件列表
    /// </summary>
    public List<MessageAttachment>? Attachments { get; set; }

    public bool HasAttachment()
    {
        return Attachments != null && Attachments.Count != 0;
    }
    
    #endregion
    
    public override string ToString()
    {
        var sb = new StringBuilder($"[MSG] {MessageId}");
        if (HasContent())
            sb.Append($" Content: {Content} ");
        if (HasAttachment())
            sb.Append($" Attachment0: {Attachments?[0].Url} ");
        return sb.ToString();
    }
    
}

/// <summary>
/// 消息引用
/// </summary>
public class MessageReference
{
    /// <summary>
    /// 消息ID
    /// </summary>
    public string MessageId { get; set; } = null!;
    /// <summary>
    /// 消息创建者ID
    /// </summary>
    public string? AuthorId { get; set; }
    /// <summary>
    /// 消息创建者昵称
    /// </summary>
    public string? NickName { get; set; }
}

/// <summary>
/// 消息附件
/// </summary>
public class MessageAttachment
{
    /// <summary>
    /// Url
    /// </summary>
    public string Url { get; set; } = null!;
}


public class OutgoingMessage
{
    public string TargetId { get; set; } = null!;
    public string? Content { get; set; }
    public string? ReferenceId { get; set; }
    public string? FileUri { get; set; }
    public FileType FileType { get; set; } = FileType.None;
    public SendFileMode FileMode { get; set; } = SendFileMode.Local;
    public bool IsPersonal { get; set; }
    
    public enum SendFileMode
    {
        Local, Server
    }
    
}


