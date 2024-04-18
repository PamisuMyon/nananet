using Microsoft.Extensions.Configuration;
using Nananet.Sdk.Fanbook;
using Nananet.Sdk.Fanbook.Models;
using Nananet.Sdk.Fanbook.Models;

var config = new ConfigurationBuilder()
    .AddJsonFile("app_settings.json")
    .Build();
var clientConfig = config.GetSection("ClientConfig").Get<ClientConfig>();

var client = new FanbookClient(clientConfig);

// --------------

// client.Debug();
// await client.SendTextMessageAsync("616200178189582337", "616200820257841152", "test");

// --------------

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
        }
    }
}

await client.StartAsync();
await Task.Delay(Timeout.Infinite);

// --------------

