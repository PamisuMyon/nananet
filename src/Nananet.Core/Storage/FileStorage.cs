using Microsoft.Extensions.Configuration;
using Nananet.Core.Models;
using Nananet.Core.Utils;

namespace Nananet.Core.Storage;

public class FileStorage : IStorage
{

    protected const string ConfigPath = "config.json";
    protected const string BlockListPath = "block-list.json";

    public Task Init()
    {
        return Task.CompletedTask;
    }

    public Task<AppSettings?> GetAppSettings()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("app_settings.json", false)
            .Build();
        var options = config.Get<AppSettings>();
        return Task.FromResult(options)!;
    }

    public async Task<BotConfig> RefreshBotConfig()
    {
        var config = await FileUtil.ReadJson<BotConfig>(ConfigPath);
        if (config == null)
        {
            Logger.L.Info("Failed to read config file, using default config");
            _config = BotConfig.Default;
        }
        else
        {
            var clone = BotConfig.Default with { };
            _config = clone.MergeWith(config);
        }
        return _config;
    }

    protected BotConfig _config;
    public BotConfig Config { get; }
    
    public Task<List<User>> RefreshBlockList()
    {
        throw new NotImplementedException();
    }

    public Task UpdateBlockList(List<User> users)
    {
        throw new NotImplementedException();
    }

    public List<User> BlockList { get; }
    
}