using Nananet.App.Nana.Functions.Chat;
using Nananet.App.Nana.Functions.Dice;
using Nananet.App.Nana.Functions.Gacha;
using Nananet.App.Nana.Functions.Help;
using Nananet.App.Nana.Functions.Picture;
using Nananet.App.Nana.Functions.Recruit;
using Nananet.App.Nana.QQGuild;
using Nananet.App.Nana.Storage;
using Nananet.Core.Commands;
using Nananet.Core.Models;
using Nananet.Core.Utils;

Logger.L.LogLevel = LogLevel.Debug;

var options = new InitOptions
{
    Storage = new MongoStorage("qqGuildAppSettings", "qqGuildBotConfig"),
    Commands = new List<Command>
    {
        new DiceCommand(),
        new GachaCommand(),
        new KittyCommand(),
        new DogeCommand(),
        new BukeyiseseCommand(),
        new RecruitCommand(),
        new HelpCommand(),
        new ChatCommand(),
    }
};
var bot = new NaqBot(options);
await bot.Launch();
