namespace Nananet.Adapter.Fanbook.Sdk.Models.Params;

// https://open.fanbook.mobi/document/manage/doc/#%E5%8F%91%E6%99%AE%E9%80%9A%E6%B6%88%E6%81%AF
public class SendMessage
{
    // public string ChatId { get; set; } = null!;
    public long ChatId { get; set; }
    public string Text { get; set; } = null!;
    public string Desc { get; set; } = null!;
    public string? ParseMode { get; set; }
    public bool Ephemeral { get; set; } = false;
    public List<string>? Users { get; set; }
    public bool Selective { get; set; }
    public long ReplyToMessageId { get; set; }
    public long ReplyToMessageIdLevel2 { get; set; }
    public object? ReplyMarkup { get; set; }
    public int Unreactive { get; set; }
    public List<string>? Mentions { get; set; }
    public List<string>? MentionRoles { get; set; }

}