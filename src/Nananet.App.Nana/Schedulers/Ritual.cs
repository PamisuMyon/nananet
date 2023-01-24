using Nananet.App.Nana.Functions.AI;
using Nananet.App.Nana.Functions.Gacha;
using Nananet.App.Nana.Functions.Recruit;
using Nananet.App.Nana.Models;

namespace Nananet.App.Nana.Schedulers;

public static class Ritual
{
    public static async Task RefreshAll()
    {
        await BaiduAuth.Init();
        await Sentence.Cache.Refresh();
        await Conversation.Cache.Refresh();
        await GachaMan.Refresh();
        await Recruiter.Instance.Refresh();
    }
}