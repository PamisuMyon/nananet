using Nado.Core.Models;
using Nado.Core.Storage;

namespace Nado.App.Nana.Storage;

public class MongoStorage : IStorage
{
    public Task Init()
    {
        throw new NotImplementedException();
    }

    public Task<AppSettings?> GetAppSettings()
    {
        throw new NotImplementedException();
    }

    public Task<BotConfig> RefreshBotConfig()
    {
        throw new NotImplementedException();
    }

    public BotConfig Config { get; set; }
}