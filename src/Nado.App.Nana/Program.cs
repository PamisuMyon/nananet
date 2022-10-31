using Nado.App.Nana.Commands.Dice;
using Nado.App.Nana.Storage;
using Nado.Core;
using Nado.Core.Commands;
using Nado.Core.Models;
using Nado.Core.Utils;

Logger.L.LogLevel = LogLevel.Debug;

var options = new InitOptions
{
    Storage = new MongoStorage(),
    Commands = new List<Command>
    {
        new DiceCommand(),
    }
};
var bot = new NadoBot(options);
await bot.Launch();
