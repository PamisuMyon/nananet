using Nado.App.Nana.Schedulers;
using Nado.Core;
using Nado.Core.Models;

namespace Nado.App.Nana;

public class NanaBot : NadoBot
{
    public NanaBot(InitOptions? options = default) : base(options)
    {
    }

    protected override async Task Refresh()
    {
        await Ritual.RefreshAll();
        await base.Refresh();
    }
    
}