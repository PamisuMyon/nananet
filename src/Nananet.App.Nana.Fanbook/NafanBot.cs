using Nananet.Adapter.Fanbook;
using Nananet.App.Nana.Schedulers;
using Nananet.Core.Models;
using Nananet.Sdk.Fanbook.Models;

namespace Nananet.App.Nana.Fanbook;

public class NafanBot : FanbookBot
{
    public NafanBot(ClientConfig clientConfig, InitOptions? options = default) : base(clientConfig, options)
    {
    }
    
    public override async Task Refresh()
    {
        await Ritual.RefreshAll();
        await base.Refresh();
    }
    
}