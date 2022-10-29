using Nado.Core.Models;
using Nado.Core.Utils;

namespace Nado.Core.Storage;

public class FileStorage : IStorage
{

    protected const string OptionsPath = "./storage/options.json";
    protected const string ConfigPath = "./storage/config.json";
    protected const string BlockListPath = "./storage/block-list.json";
    
    public Task Init()
    {
        return Task.CompletedTask;
    }

    public async Task<DodoOptions?> GetApiOptions()
    {
        var option = await FileUtil.ReadJson<DodoOptions>(OptionsPath);
        if (option == null)
        {
            Logger.L.Error("No available options.json file found.");
            return null;
        }
        return null;
    }

    public async Task<BotConfig> RefreshConfig()
    {
        var config = await FileUtil.ReadJson<BotConfig>(ConfigPath);
        if (config == null)
        {
            Logger.L.Info("Failed to read config file, using default config");
        }
        else
        {
            var clone = BotConfig.Default with { };
            Config = clone.MergeWith(config);
        }
        return Config;
    }

    public BotConfig Config { get; set; }
    
}