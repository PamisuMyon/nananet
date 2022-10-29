using DoDo.Open.Sdk.Models;
using DoDo.Open.Sdk.Services;
using Nado.Core.Models;
using Nado.Core.Storage;
using Nado.Core.Utils;

namespace Nado.Core;

public class Bot
{

    protected IStorage _storage;
    public IStorage Storage
    {
        get => _storage;
        set => _storage = value;
    }

    protected OpenApiService _openApiService;
    protected EventProcessService _eventProcessService;

    public Bot(InitOptions? options = default)
    {
        _storage = options?.Storage ?? new FileStorage();
    }
    
    public async Task Launch()
    {
        await Storage.Init();

        var appSettings = await Storage.GetAppSettings();
        if (appSettings == null)
        {
            Logger.L.Error("Options not available, launch failed.");
            return;
        }

        _openApiService = new OpenApiService(new OpenApiOptions
        {
            BaseApi = appSettings.BaseApi,
            ClientId = appSettings.ClientId,
            Token = appSettings.Token,
        });
        
        _eventProcessService = new NadoEventProcessService(_openApiService);
        
        var openEventService = new OpenEventService(_openApiService, _eventProcessService, new OpenEventOptions
        {
            IsReconnect = true,
            IsAsync = true,
        });
        
        await openEventService.ReceiveAsync();
    }
    
    
}