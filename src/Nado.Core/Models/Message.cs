using DoDo.Open.Sdk.Models.Events;
using DoDo.Open.Sdk.Models.Messages;

namespace Nado.Core.Models;

public class Message
{

    #region EventSubjectDataBusiness

    /// <summary>事件ID</summary>
    public string EventId { get; set; }

    /// <summary>事件类型，EventTypeConst.</summary>
    public string EventType { get; set; }

    /// <summary>发送时间戳</summary>
    public long Timestamp { get; set; }
    
    #endregion

    #region EventBodyChannelMessage EventBodyPersonalMessage
    
    /// <summary>来源群号</summary>
    public string IslandId { get; set; }

    /// <summary>来源频道ID</summary>
    public string ChannelId { get; set; }

    /// <summary>来源DoDo号</summary>
    public string DodoId { get; set; }

    /// <summary>个人信息</summary>
    public MessageModelPersonal Personal { get; set; }

    /// <summary>成员信息</summary>
    public MessageModelMember Member { get; set; }

    /// <summary>消息ID</summary>
    public string MessageId { get; set; }

    /// <summary>回复信息</summary>
    public MessageModelReference? Reference { get; set; }

    /// <summary>消息类型，1：文字消息，2：图片消息，3：视频消息，4：分享消息，5：文件消息，6：卡片消息</summary>
    public Type MessageType { get; set; }

    public enum Type
    {
        None = 0,
        Text = 1,
        Picture = 2,
        Video = 3,
        Share = 4,
        File = 5,
        Card = 6,
    }
    
    #endregion

    /// <summary>是否为私聊消息</summary>
    public bool IsPersonal { get; set; }

    public override string ToString()
    {
        return $"[MSG] {MessageId}";
    }

    public static Message? FromChannelMessageEvent<T>(EventSubjectOutput<EventSubjectDataBusiness<EventBodyChannelMessage<T>>> input) where T : MessageBodyBase
    {
        Message? message = null;
        var eventBody = input.Data.EventBody;
        if (eventBody.MessageBody is MessageBodyText text)
        {
            message = TextMessage.FromMessageBody(text);
        } 
        else if (eventBody.MessageBody is MessageBodyPicture picture)
        {
            message = PictureMessage.FromMessageBody(picture);
        }
        if (message != null)
        {
            message.EventId = input.Data.EventId;
            message.EventType = input.Data.EventType;
            message.Timestamp = input.Data.Timestamp;
            message.MessageId = input.Data.EventBody.MessageId;
            message.Reference = input.Data.EventBody.Reference;
            message.MessageType = (Type)input.Data.EventBody.MessageType;
            message.DodoId = input.Data.EventBody.DodoSourceId;
            message.ChannelId = input.Data.EventBody.ChannelId;
            message.IslandId = input.Data.EventBody.IslandSourceId;
            message.Personal = input.Data.EventBody.Personal;
            message.Member = input.Data.EventBody.Member;
            message.IsPersonal = false;
        }
        return message;
    }
    
    public static Message? FromPersonalMessageEvent<T>(EventSubjectOutput<EventSubjectDataBusiness<EventBodyPersonalMessage<T>>> input) where T : MessageBodyBase
    {
        Message? message = null;
        var eventBody = input.Data.EventBody;
        if (eventBody.MessageBody is MessageBodyText text)
        {
            message = TextMessage.FromMessageBody(text);
        } 
        else if (eventBody.MessageBody is MessageBodyPicture picture)
        {
            message = PictureMessage.FromMessageBody(picture);
        }
        if (message != null)
        {
            message.EventId = input.Data.EventId;
            message.EventType = input.Data.EventType;
            message.Timestamp = input.Data.Timestamp;
            message.MessageId = input.Data.EventBody.MessageId;
            message.MessageType = (Type)input.Data.EventBody.MessageType;
            message.DodoId = input.Data.EventBody.DodoSourceId;
            message.Personal = input.Data.EventBody.Personal;
            message.IsPersonal = true;
        }
        return message;
    }
    
}

public class TextMessage : Message
{
    /// <summary>文字内容</summary>
    public string Content { get; set; }
    
    public string OriginalContent { get; set; }

    public override string ToString()
    {
        return $"[MSG TXT] {MessageId} {Content}";
    }

    public static TextMessage FromMessageBody(MessageBodyText text)
    {
        return new TextMessage
        {
            Content = text.Content,
            OriginalContent = text.Content
        };
    }
}

public class PictureMessage : Message
{
    /// <summary>图片链接</summary>
    public string Url { get; set; }

    /// <summary>图片宽度</summary>
    public int? Width { get; set; }

    /// <summary>图片高度</summary>
    public int? Height { get; set; }

    /// <summary>是否原图，0：压缩图，1：原图</summary>
    public int? IsOriginal { get; set; }

    public override string ToString()
    {
        return $"[MSG PIC] {MessageId} {Url}";
    }

    public static PictureMessage FromMessageBody(MessageBodyPicture picture)
    {
        return new PictureMessage
        {
            Url = picture.Url,
            Height = picture.Height,
            Width = picture.Width,
            IsOriginal = picture.IsOriginal
        };
    }
    
}
