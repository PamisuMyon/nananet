using Nado.Core.Models;
using Nado.Core.Storage;

namespace Nado.Core;

public class Bot
{

    protected IStorage _storage;
    public IStorage Storage
    {
        get => _storage;
        set => _storage = value;
    }

    public Bot(InitOptions? options)
    {
        _storage = options?.Storage ?? new FileStorage();
    }
    
    public async Task Launch()
    {
        await Storage.Init();
        
                
    }
}