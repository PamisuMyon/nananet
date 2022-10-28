using Nado.Core.Models;

namespace Nado.Core.Storage;

public interface IStorage
{
    public void Init();

    public Task<DodoOptions> GetApiOptions();

    public Task<BotConfig> RefreshConfig();
    
    public BotConfig Config { get; protected set; }
    
    
}