using Microsoft.Extensions.Configuration;
using MongoDB.Entities;
using Nananet.App.Nana.Models;
using Nananet.Core.Models;
using Nananet.Core.Storage;
using Nananet.Core.Utils;

namespace Nananet.App.Nana.Storage;

public class MongoStorage : IStorage
{

    protected MongoAppSettings _settings;
    protected string _keyAppSettings;
    protected string _keyBotConfig;

    public MongoStorage(string? keyAppSettings, string? keyBotConfig)
    {
        _keyAppSettings = keyAppSettings ?? "appSettings";
        _keyBotConfig = keyBotConfig ?? "botConfig";
    }

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
        var appSettings = await MiscConfig.FindByName<AppSettings>(_keyAppSettings);
        if (appSettings == null)
        {
            Logger.L.Error("No AppSettings found!");
            return default;
        }

        _settings.AppId = appSettings.AppId;
        _settings.Token = appSettings.Token;
        _settings.Secret = appSettings.Secret;
        _settings.IsDebug = appSettings.IsDebug;
        _settings.IsPrivate = appSettings.IsPrivate;
        _settings.BotId = appSettings.BotId;
        return _settings;
    }

    public async Task<BotConfig> RefreshBotConfig()
    {
        var config = await MiscConfig.FindByName<BotConfig>(_keyBotConfig);
        if (config == null)
        {
            Logger.L.Warn("No BotConfig found, using default.");
            config = BotConfig.Default;
            await MiscConfig.Save("botConfig", config);
        }
        _config = config;
        return config;
    }

    protected BotConfig _config;
    public BotConfig Config => _config;
    public async Task<List<User>> RefreshBlockList()
    {
        _blockList.Clear();
        var list = await DB.Find<BlockedUser>().ExecuteAsync();
        if (list != null)
        {
            foreach (var it in list)
                _blockList.Add(it);
        }
        return _blockList;
    }

    public async Task UpdateBlockList(List<User> users)
    {
        var bulk = DB.Update<BlockedUser>();
        foreach (var user in users)
        {
            bulk.Match(u => u.UserId == user.Id)
                .ModifyWith(new BlockedUser
                {
                    UserId = user.Id,
                    Name = user.NickName,
                    UserName = user.UserName
                })
                .Option(o =>
                {
                    o.IsUpsert = true;
                })
                .AddToQueue();
        }

        await bulk.ExecuteAsync();
    }

    protected List<User> _blockList = new ();
    public List<User> BlockList => _blockList;
}