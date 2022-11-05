using Nananet.Adapter.Kook;
using Nananet.App.Nana.Schedulers;
using Nananet.Core.Models;

namespace Nananet.App.Nana.Kook;

public class NakoBot : KookBot
{
    
    public NakoBot(InitOptions? options = default) : base(options)
    {
    }
    
    public override async Task Refresh()
    {
        await Ritual.RefreshAll();
        await base.Refresh();
    }
}