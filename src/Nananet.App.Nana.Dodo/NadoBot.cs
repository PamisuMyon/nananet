using Nananet.Adapter.Dodo;
using Nananet.App.Nana.Schedulers;
using Nananet.Core.Models;

namespace Nananet.App.Nana.Dodo;

public class NadoBot : DodoBot
{
    public NadoBot(InitOptions? options = default) : base(options)
    {
    }
    
    public override async Task Refresh()
    {
        await Ritual.RefreshAll();
        await base.Refresh();
    }
    
}