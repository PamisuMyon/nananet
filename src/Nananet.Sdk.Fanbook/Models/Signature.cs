using System.Reflection;
using System.Text;
using Nananet.Sdk.Fanbook.Utils;

namespace Nananet.Sdk.Fanbook.Models;

public class Signature
{
    private const string VR = "dJcPo1dQHeMgDn1s8MQr"; // TODO 可能会变
    
    public string AppKey { get; set; }
    public string Authorization { get; set; }
    public string Nonce { get; set; }
    public string Platform { get; set; }
    public string RequestBody { get; set; }
    public long Timestamp { get; set; }
    
    private string? _signature;

    public Signature(string appKey, string authorization, string nonce, string requestBody, string platform = "web")
    {
        AppKey = appKey;
        Authorization = authorization;
        Nonce = nonce;
        Platform = platform;
        RequestBody = requestBody;
        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    public string GenerateSignature()
    {
        if (_signature != null)
            return _signature;
            
        var props = GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .OrderBy(prop => prop.Name)
            .Select(prop => $"{prop.Name}={prop.GetValue(this)}");
        var t = string.Join("&", props);
        t = $"{t}&{VR}";
        t = Uri.EscapeDataString(t);
        _signature = SdkUtil.GetMd5(Encoding.UTF8.GetBytes(t));
        
        return _signature;
    }
    
}