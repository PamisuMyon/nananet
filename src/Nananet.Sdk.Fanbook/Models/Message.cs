using Newtonsoft.Json;

namespace Nananet.Sdk.Fanbook.Models;

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
    public string? GuildId { get; set; }
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
    [JsonProperty("contentType")]
    public int ContentType { get; set; }

    public TextContent(string text)
    {
        Type = "text";
        Text = text;
        ContentType = 0;
    }
}

public class ImageContent
{
    public string Type { get; set; }
    [JsonProperty("fileType")]
    public string FileType { get; set; }
    public string Url { get; set; } = null!;
    public bool Thumb { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public long Size { get; set; }
    [JsonProperty("localFilePath")]
    public string LocalFilePath { get; set; } = null!;
    [JsonProperty("localIdentify")]
    public string LocalIdentify { get; set; } = null!;

    public ImageContent()
    {
        Type = "image";
        FileType = "image";
    }
    
}
