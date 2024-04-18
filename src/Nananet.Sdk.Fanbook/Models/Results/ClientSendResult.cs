namespace Nananet.Sdk.Fanbook.Models.Results;

public class ClientSendResult
{
    public long Time { get; set; }
    public string MessageId { get; set; } = null!;
    public int Status { get; set; }
    public int AssistLevel { get; set; }
}