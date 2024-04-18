namespace Nananet.Sdk.Fanbook.Models;

public class ClientConfig
{
    public string AppKey { get; set; } = null!;
    public string AppVersion { get; set; } = null!;
    public int BuildNumber { get; set; }
    public string DeviceId { get; set; } = null!;
    public string Token { get; set; } = null!;
    public Dictionary<string, string> DummyCookies { get; set; } = null!;
}