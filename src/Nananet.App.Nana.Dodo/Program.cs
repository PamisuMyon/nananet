using Nananet.App.Nana.Dodo;
using Nananet.App.Nana.Functions.Chat;
using Nananet.App.Nana.Functions.Dice;
using Nananet.App.Nana.Functions.Gacha;
using Nananet.App.Nana.Functions.Help;
using Nananet.App.Nana.Functions.Picture;
using Nananet.App.Nana.Functions.Recruit;
using Nananet.App.Nana.Schedulers;
using Nananet.App.Nana.Storage;
using Nananet.Core.Commands;
using Nananet.Core.Models;
using Nananet.Core.Utils;

Logger.L.LogLevel = LogLevel.Debug;

var options = new InitOptions
{
    Storage = new MongoStorage("dodoAppSettings", "dodoBotConfig"),
    Commands = new List<Command>
    {
        new DiceCommand(),
        new GachaCommand(),
        new RecruitCommand(),
        new KittyCommand(),
        new DogeCommand(),
        new SetuCommand(),
        new HelpCommand(),
        new ChatCommand(),
    }
};
var bot = new NadoBot(options);
new Alarm(bot).Schedule();
await bot.Launch();