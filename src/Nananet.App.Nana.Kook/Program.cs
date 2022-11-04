
using Nananet.Adapter.Kook;
using Nananet.App.Nana.Storage;
using Nananet.Core.Commands;
using Nananet.Core.Models;
using Nananet.Core.Utils;

Logger.L.LogLevel = LogLevel.Debug;

var options = new InitOptions
{
    Storage = new MongoStorage("kookAppSettings", "kookBotConfig"),
    Commands = new List<Command>()
};
var bot = new KookBot(options);
await bot.Launch();
