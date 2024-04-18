namespace Nananet.Adapter.Fanbook.Sdk.Models;

public class ClientRuntimeData
{
    public ClientRuntimeData(ClientConfig config, string tempId, string xsp)
    {
        Config = config;
        TempId = tempId;
        Xsp = xsp;
    }

    public ClientConfig Config { get; internal set; }
    public string TempId { get; private set; }
    public string Xsp { get; private set; }
    public string? ClientId { get; internal set; }
    
    public string? CurrentGuildId { get; internal set; }
    public string? CurrentChannelId { get; internal set; }
    public string? CurrentChannelLastMessageId { get; internal set; }
    
}