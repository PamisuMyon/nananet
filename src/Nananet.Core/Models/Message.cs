namespace Nananet.Core.Models;

public class Message
{

    #region EventSubjectDataBusiness

    public string EventId { get; set; }

    public string EventType { get; set; }

    public long Timestamp { get; set; }
    
    #endregion

    #region EventBodyChannelMessage EventBodyPersonalMessage
    
    public string GroupId { get; set; }

    public string ChannelId { get; set; }

    public string AuthorId { get; set; }

    public User Author { get; set; }

    public string MessageId { get; set; }

    public Reference? Reference { get; set; }

    public int MessageType { get; set; }

    #endregion

    public bool IsPersonal { get; set; }

    public object? Origin { get; set; }
    
    public override string ToString()
    {
        return $"[MSG] {MessageId}";
    }
    
}

public class Reference
{
    public string MessageId;
    public string AuthorId { get; set; }
    public string NickName { get; set; }
}

public class TextMessage : Message
{
    public string Content { get; set; }
    public string RichContent { get; set; }
    public string RawContent { get; set; }
    public string OriginalContent { get; set; }

    public override string ToString()
    {
        return $"[MSG TXT] {MessageId} {Content}";
    }

}

public class PictureMessage : Message
{
    public string Url { get; set; }

    public int? Width { get; set; }

    public int? Height { get; set; }

    public int? IsOriginal { get; set; }

    public override string ToString()
    {
        return $"[MSG PIC] {MessageId} {Url}";
    }
    
}
