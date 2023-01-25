using Nananet.App.Nana.Functions.Chat;
using Nananet.App.Nana.Functions.Dice;
using Nananet.App.Nana.Functions.Gacha;
using Nananet.App.Nana.Functions.Help;
using Nananet.App.Nana.Functions.Picture;
using Nananet.App.Nana.Functions.Recruit;
using Nananet.App.Nana.Functions.System;
using Nananet.App.Nana.QQGuild;
using Nananet.App.Nana.Schedulers;
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
        new BukeyiseseCommand(),
        new CatBoyCommand(),
        new KittyCommand(),
        new DogeCommand(),
        new QuackCommand(),
        new FoxCommand(),
        new RecruitCommand(),
        new HelpCommand("qqGuildHelp"),
        new RefreshCacheCommand(),
        new AlarmSettingsCommand(),
        new ChatCommand(),
    }
};
var bot = new NaqBot(options);
await Alarm.Instance.Schedule(bot);
await bot.Launch();
