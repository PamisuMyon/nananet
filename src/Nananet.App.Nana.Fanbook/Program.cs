
using Nananet.Adapter.Fanbook;
using Nananet.Core.Utils;

Console.WriteLine("Hello, World!");
const string token = "c912b6f823d925c25d14e8855c04ef5bb4a5607f7f96678a51c8e9194abe1efb9a2d9c7a3814150b71a54236bb95650aeae57cbb50c68745221470c42c2e8c699faf629e4d23722d6edcec6513acb7dcfba4f7d2698733597088db280d2f20cca33f1e829d1984fb319b880f5607bf0270440ef6a323912a08aa52c63dd5160353e8574176746c401bc0dfb8b68aa7b04073c56bf31b5a1491541a9af9fdf473";

var client = new FanbookClient(token);
client.Ready += async () =>
{
    Logger.L.Debug("Ready");
    await Task.Delay(500);
    await client.SendTextMessageAsync("616200178189582337", "616200820257841152", "test");
};
await client.StartAsync();

await Task.Delay(Timeout.Infinite);

// client.Debug();
// await client.SendTextMessageAsync("616200178189582337", "616200820257841152", "test");