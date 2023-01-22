using Nananet.Adapter.QQGuild;
using Nananet.App.Nana.Schedulers;
using Nananet.Core.Models;

namespace Nananet.App.Nana.QQGuild;

public class NaqBot : QQGuildBot
{
    public NaqBot(InitOptions? options = default) : base(options)
    {
    }
    
    public override async Task Refresh()
    {
        await Ritual.RefreshAll();
        await base.Refresh();
    }
}