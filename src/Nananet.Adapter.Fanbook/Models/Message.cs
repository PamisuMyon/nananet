namespace Nananet.Adapter.Fanbook.Models;

public class Message
{
    public string Content { get; set; } = null!;
    public EContentType ContentType { get; set; }
    public TextContent? TextContent { get; set; } 
    public long Time { get; set; }
    public string UserId { get; set; } = null!;
    public string ChannelId { get; set; } = null!;
    public string MessageId { get; set; } = null!;
    public string? QuoteL1 { get; set; }
    public string? QuoteL2 { get; set; }
    public string GuildId { get; set; } = null!;
    public int ChannelType { get; set; }
    public int Status { get; set; }
    public string Nonce { get; set; } = null!;
    public object? Ctype { get; set; }
    public List<Mention>? Mentions { get; set; }
    public Member? Member { get; set; }
    public string ResourceType { get; set; } = null!;
    public string Platform { get; set; } = null!;
    public User Author { get; set; } = null!;
    public string Desc { get; set; } = null!;
}

public enum EContentType
{
    None, Text, Image
}

public class TextContent
{
    public string Type { get; set; } = null!;
    public string Text { get; set; } = null!;
    public int ContentType { get; set; }

    public TextContent(string text)
    {
        Type = "text";
        Text = text;
        ContentType = 0;
    }
}
