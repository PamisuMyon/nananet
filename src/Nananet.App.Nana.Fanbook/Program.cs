
using Nananet.Adapter.Fanbook;
using Nananet.Adapter.Fanbook.Models;
using Nananet.Adapter.Fanbook.Utils;
using Nananet.Core.Utils;

Console.WriteLine("Hello, World!");
const string token = "c912b6f823d925c25d14e8855c04ef5bb4a5607f7f96678a51c8e9194abe1efb9a2d9c7a3814150b71a54236bb95650aeae57cbb50c68745221470c42c2e8c699faf629e4d23722d6edcec6513acb7dcfba4f7d2698733597088db280d2f20cca33f1e829d1984fb319b880f5607bf0270440ef6a323912a08aa52c63dd5160353e8574176746c401bc0dfb8b68aa7b04073c56bf31b5a1491541a9af9fdf473";

var client = new FanbookClient(token);

// -----------

// client.Debug();
// await client.SendTextMessageAsync("616200178189582337", "616200820257841152", "test");

// -----------

// var signatureParams = new SignatureParams("ww0qqc0UHMsFtDUhGbh0", token, "98c37ae4-caa7-43fd-b573-16e9661155dc", "{\"channel_id\":\"616200820257841152\",\"guild_id\":\"616200178189582337\",\"content\":\"{\\\"type\\\":\\\"text\\\",\\\"text\\\":\\\"6\\\",\\\"contentType\\\":0}\",\"desc\":\"6\",\"nonce\":\"616599250645651064\",\"token\":\"c912b6f823d925c25d14e8855c04ef5bb4a5607f7f96678a51c8e9194abe1efb9a2d9c7a3814150b71a54236bb95650aeae57cbb50c68745221470c42c2e8c699faf629e4d23722d6edcec6513acb7dcfba4f7d2698733597088db280d2f20cca33f1e829d1984fb319b880f5607bf0270440ef6a323912a08aa52c63dd5160353e8574176746c401bc0dfb8b68aa7b04073c56bf31b5a1491541a9af9fdf473\",\"transaction\":\"93927fc0-1508-41e1-abb8-c72380897745\"}");
// signatureParams.Timestamp = 1712231605235;
// var signature = signatureParams.GenerateSignature();
// Console.WriteLine(signature);

// -----------

client.MessageReceived += OnMessageReceived;

void OnMessageReceived(Message message)
{
    HandleMessage(message);
}

async void HandleMessage(Message message)
{
    if (message.GuildId == "616200178189582337" && message.ChannelId == "616200820257841152")
    {
        if (message.TextContent?.Text == "he")
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

