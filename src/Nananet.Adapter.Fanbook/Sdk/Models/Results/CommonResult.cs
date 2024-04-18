namespace Nananet.Adapter.Fanbook.Sdk.Models.Results;

public class CommonResult<T>
{
    public bool Status { get; set; }
    public int Code { get; set; }
    public string Message { get; set; } = null!;
    public string Desc { get; set; } = null!;
    public string RequestId { get; set; } = null!;
    public T? Data { get; set; }
}

public class CommonResult
{
    public bool Status { get; set; }
    public int Code { get; set; }
    public string Message { get; set; } = null!;
    public string Desc { get; set; } = null!;
    public string RequestId { get; set; } = null!;
}
