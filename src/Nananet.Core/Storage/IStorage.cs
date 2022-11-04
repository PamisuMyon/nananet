using Nananet.Core.Models;

namespace Nananet.Core.Storage;

public interface IStorage
{
    public Task Init();

    public Task<AppSettings?> GetAppSettings();

    public Task<BotConfig> RefreshBotConfig();
    
    public BotConfig Config { get; }

    public Task<List<User>> RefreshBlockList();

    public Task UpdateBlockList(List<User> users);
    
    public List<User> BlockList { get; }

}