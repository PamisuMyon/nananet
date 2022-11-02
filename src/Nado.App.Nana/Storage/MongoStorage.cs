using Microsoft.Extensions.Configuration;
using MongoDB.Entities;
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
        _settings.DodoId = appSettings.DodoId;
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
            bulk.Match(u => u.UserId == user.UserId)
                .ModifyWith(new BlockedUser
                {
                    UserId = user.UserId,
                    Name = user.Name,
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