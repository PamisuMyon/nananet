
using Microsoft.Extensions.Configuration;
using Nananet.Adapter.Fanbook;
using Nananet.Adapter.Fanbook.Models;

Console.WriteLine("Hello, World!");
var config = new ConfigurationBuilder()
    .AddJsonFile("app_settings.json")
    .Build();
var token = config.GetValue<string>("Token");

var client = new FanbookClient(token);

client.MessageReceived += OnMessageReceived;

void OnMessageReceived(Message message)
{
    HandleMessage(message);
}

async void HandleMessage(Message message)
{
    if (message.GuildId == "616200178189582337" && message.ChannelId == "616200820257841152")
    {
        if (message.TextContent?.Text == "1")
        {
            await Task.Delay(500);
            await client.SendTextMessageAsync(message.GuildId, message.ChannelId, "hello!");
            await Task.Delay(1000);
            await client.SendTextMessageAsync(message.GuildId, message.ChannelId, "hello2!");
            await Task.Delay(1000);
            await client.SendTextMessageAsync(message.GuildId, message.ChannelId, "hello3");
        }
    }
}

await client.StartAsync();

await Task.Delay(5000);
await client.SendTextMessageAsync("616200178189582337", "616200820257841152", "test");

await Task.Delay(Timeout.Infinite);

// --------------

