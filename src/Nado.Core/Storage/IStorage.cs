using Nado.Core.Models;

namespace Nado.Core.Storage;

public interface IStorage
{
    public Task Init();

    public Task<AppSettings?> GetAppSettings();

    public Task<BotConfig> RefreshBotConfig();
    
    public BotConfig Config { get; set; }
    
    
}