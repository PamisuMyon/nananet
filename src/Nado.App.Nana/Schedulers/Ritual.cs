using Nado.App.Nana.Functions.Gacha;
using Nado.App.Nana.Models;

namespace Nado.App.Nana.Schedulers;

public static class Ritual
{
    public static async Task RefreshAll()
    {
        await Sentence.Cache.Refresh();
        await Conversation.Cache.Refresh();
        await GachaMan.Refresh();
    }
}