using Microsoft.Extensions.Configuration;
using Nananet.App.Nana.Fanbook;
using Nananet.App.Nana.Functions.Chat;
using Nananet.App.Nana.Functions.Dice;
using Nananet.App.Nana.Functions.Gacha;
using Nananet.App.Nana.Functions.Help;
using Nananet.App.Nana.Functions.Picture;
using Nananet.App.Nana.Functions.System;
using Nananet.App.Nana.Functions.Wiki;
using Nananet.App.Nana.Schedulers;
using Nananet.App.Nana.Storage;
using Nananet.Core.Commands;
using Nananet.Core.Models;
using Nananet.Sdk.Fanbook.Models;

var config = new ConfigurationBuilder()
    .AddJsonFile("app_settings.json")
    .Build();
var clientConfig = config.GetSection("ClientConfig").Get<ClientConfig>();

var options = new InitOptions
{
    Storage = new MongoStorage("fanbookAppSettings", "fanbookBotConfig"),
    Commands = new List<Command>
    {
        // new GachaCommand(),
        // new FakeGachaCommand(),
        // new CatBoyCommand(),
        new KittyCommand(),
        new DogeCommand(),
        new QuackCommand(),
        new FoxCommand(),
        new SetuCommand(),
        // new RecruitCommand(),
        new DiceCommand(),
        new HelpCommand("fanbookHelp"),
        // new AkOperatorEvolveCommand(),
        // new AkOperatorSkillMasteryCommand(),
        // new AkBirthdayOperatorCommand(),
        // new AkOperatorBirthdayCommand(),
        // new AkFuzzyCommand(),
        new RefreshCacheCommand(),
        // new AlarmSettingsCommand(),
        new ChatCommand(),
    }
};
var bot = new NafanBot(clientConfig, options);
// await Alarm.Instance.Schedule(bot);
await bot.Launch();


// var client = new FanbookClient(clientConfig);

// --------------

// client.Debug();
// await client.SendTextMessageAsync("616200178189582337", "616200820257841152", "test");

// --------------

// client.MessageReceived += OnMessageReceived;
//
// void OnMessageReceived(Message message)
// {
//     HandleMessage(message);
// }
//
// async void HandleMessage(Message message)
// {
//     if (message.GuildId == "616200178189582337" && message.ChannelId == "616200820257841152")
//     {
//         if (message.TextContent?.Text == "1")
//         {
//             await Task.Delay(500);
//             await client.SendTextMessageAsync(message.GuildId, message.ChannelId, "hello!");
//         }
//         else if (message.TextContent?.Text == "2")
//         {
//             await Task.Delay(500);
//             await client.SendImageMessageAsync(message.GuildId, message.ChannelId, "D:\\18431867162250697381.JPG");
//         }
//     }
// }
//
// await client.SetBrowserContextAsync("616200178189582337", "616200820257841152");
// await client.StartAsync();
// await Task.Delay(Timeout.Infinite);

// --------------

