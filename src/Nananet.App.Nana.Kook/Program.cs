using Nananet.App.Nana.Functions.Chat;
using Nananet.App.Nana.Functions.Dice;
using Nananet.App.Nana.Functions.Gacha;
using Nananet.App.Nana.Functions.Help;
using Nananet.App.Nana.Functions.Picture;
using Nananet.App.Nana.Functions.Recruit;
using Nananet.App.Nana.Functions.System;
using Nananet.App.Nana.Functions.Wiki;
using Nananet.App.Nana.Kook;
using Nananet.App.Nana.Schedulers;
using Nananet.App.Nana.Storage;
using Nananet.Core.Commands;
using Nananet.Core.Models;

var options = new InitOptions
{
    Storage = new MongoStorage("kookAppSettings", "kookBotConfig"),
    Commands = new List<Command>
    {
        new GachaCommand(),
        new CatBoyCommand(),
        new KittyCommand(),
        new DogeCommand(),
        new QuackCommand(),
        new FoxCommand(),
        new SetuCommand(),
        new RecruitCommand(),
        new DiceCommand(),
        new HelpCommand(),
        new AkOperatorEvolveCommand(),
        new AkOperatorSkillMasteryCommand(),
        new AkBirthdayOperatorCommand(),
        new AkOperatorBirthdayCommand(),
        new AkFuzzyCommand(),
        new RefreshCacheCommand(),
        new AlarmSettingsCommand(),
        new ChatCommand(),
    }
};
var bot = new NakoBot(options);
await Alarm.Instance.Schedule(bot);
await bot.Launch();
