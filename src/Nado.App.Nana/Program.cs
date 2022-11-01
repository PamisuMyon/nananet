using Nado.App.Nana;
using Nado.App.Nana.Commands.Dice;
using Nado.App.Nana.Functions.Chat;
using Nado.App.Nana.Functions.Gacha;
using Nado.App.Nana.Functions.Help;
using Nado.App.Nana.Functions.Picture;
using Nado.App.Nana.Functions.Recruit;
using Nado.App.Nana.Storage;
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
        new GachaCommand(),
        new RecruitCommand(),
        new PictureCommand(),
        new HelpCommand(),
        new ChatCommand(),
    }
};
var bot = new NanaBot(options);
await bot.Launch();
