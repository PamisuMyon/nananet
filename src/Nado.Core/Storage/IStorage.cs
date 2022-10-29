using Nado.Core.Models;

namespace Nado.Core.Storage;

public interface IStorage
{
    public Task Init();

    public Task<DodoOptions?> GetApiOptions();

    public Task<BotConfig> RefreshConfig();
    
    public BotConfig Config { get; set; }
    
    
}