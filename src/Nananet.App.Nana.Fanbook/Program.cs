
using Microsoft.Extensions.Configuration;
using Nananet.Adapter.Fanbook;

Console.WriteLine("Hello, World!");

var config = new ConfigurationBuilder()
    .AddJsonFile("app_settings.json")
    .Build();
var token = config.GetValue<string>("Token");
Console.WriteLine(token);
var client = new FanbookClient(token);
await client.BotApi.GetMeAsync();

await Task.Delay(500);

// await client.ChannelApi.ChannelListAsync("616200178189582337");
// await client.ChannelApi.GetPrivateChat(593707343603425280);
// await client.MessageApi.GetUpdatesV2Async();
// await client.MessageApi.SendMessageAsync(616200820257841152, "test");
// await client.MessageApi.SendMessageAsync(616855910274682880, "test");
// await client.MessageApi.SendPhotoAsync(616200820257841152);
// await client.MessageApi.GetMessage(616200820257841152, 616813114251284480);