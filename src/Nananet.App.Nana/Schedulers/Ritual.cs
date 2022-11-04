using Nananet.App.Nana.Functions.Gacha;
using Nananet.App.Nana.Models;

namespace Nananet.App.Nana.Schedulers;

public static class Ritual
{
    public static async Task RefreshAll()
    {
        await Sentence.Cache.Refresh();
        await Conversation.Cache.Refresh();
        await GachaMan.Refresh();
    }
}