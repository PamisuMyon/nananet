using Microsoft.Extensions.Configuration;
using Nado.App.Nana.Models;
using Nado.Core.Models;
using Nado.Core.Storage;
using Nado.Core.Utils;

namespace Nado.App.Nana.Storage;

public class MongoStorage : IStorage
{

    protected MongoAppSettings _settings;
    
    public async Task Init()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("app_settings.json")
            .Build();
        _settings = config.Get<MongoAppSettings>();
        await DbUtil.Connect(_settings.MongoDbUri, _settings.MongoDbName);
    }

    public async Task<AppSettings?> GetAppSettings()
    {
        var appSettings = await MiscConfig.FindByName<AppSettings>("dodoAppSettings");
        if (appSettings == null)
        {
            Logger.L.Error("No dodoAppSettings found!");
            return default;
        }
        _settings.Token = appSettings.Token;
        _settings.ClientId = appSettings.ClientId;
        return _settings;
    }

    public async Task<BotConfig> RefreshBotConfig()
    {
        var config = await MiscConfig.FindByName<BotConfig>("dodoBotConfig");
        if (config == null)
        {
            Logger.L.Warn("No dodoBotConfig found, using default.");
            config = BotConfig.Default;
            await MiscConfig.Save("botConfig", config);
        }
        _config = config;
        return config;
    }

    protected BotConfig _config;
    public BotConfig Config => _config;
}