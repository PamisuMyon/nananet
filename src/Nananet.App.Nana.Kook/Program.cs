
using Nananet.Adapter.Kook;
using Nananet.App.Nana.Functions.Dice;
using Nananet.App.Nana.Kook;
using Nananet.App.Nana.Storage;
using Nananet.Core.Commands;
using Nananet.Core.Models;
using Nananet.Core.Utils;

Logger.L.LogLevel = LogLevel.Debug;

var options = new InitOptions
{
    Storage = new MongoStorage("kookAppSettings", "kookBotConfig"),
    Commands = new List<Command>
    {
        new DiceCommand(),
    }
};
var bot = new NakoBot(options);
await bot.Launch();
