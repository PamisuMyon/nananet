using DoDo.Open.Sdk.Models;
using DoDo.Open.Sdk.Services;

namespace Nado.Core;

public class NadoEventProcessService : EventProcessService
{

    protected OpenApiService _openApiService;
    protected OpenApiOptions _openApiOptions;
    
    public NadoEventProcessService(OpenApiService openApiService)
    {
        _openApiService = openApiService;
        _openApiOptions = _openApiService.GetBotOptions();
    }
    
    public override void Connected(string message)
    {
        throw new NotImplementedException();
    }

    public override void Disconnected(string message)
    {
        throw new NotImplementedException();
    }

    public override void Reconnected(string message)
    {
        throw new NotImplementedException();
    }

    public override void Exception(string message)
    {
        throw new NotImplementedException();
    }
    
    
}