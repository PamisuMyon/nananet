namespace Nananet.Sdk.Fanbook.Models.Results;

public class ActionResult<T>
{
    public string Action { get; set; } = null!;
    public T? Data { get; set; }
    public int Ack { get; set; }
    public int Seq { get; set; }
    public bool Status { get; set; }
}

public class ActionResult
{
    public string Action { get; set; } = null!;
    public int Ack { get; set; }
    public int Seq { get; set; }
    public bool Status { get; set; }
}
