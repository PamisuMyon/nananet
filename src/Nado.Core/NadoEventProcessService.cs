using DoDo.Open.Sdk.Models;
using DoDo.Open.Sdk.Models.Events;
using DoDo.Open.Sdk.Services;
using Nado.Core.Utils;

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
        Logger.L.Info($"Connected: {message}");
    }

    public override void Disconnected(string message)
    {
        Logger.L.Info($"Disconnected: {message}");
    }

    public override void Reconnected(string message)
    {
        Logger.L.Info($"Reconnected: {message}");
    }

    public override void Exception(string message)
    {
        Logger.L.Error($"Exception: {message}");
    }
    
    // public override void Received(string message)
    // {
    //     Logger.L.Verbose($"Received: {message}");
    // }

    public override void PersonalMessageEvent<T>(EventSubjectOutput<EventSubjectDataBusiness<EventBodyPersonalMessage<T>>> input)
    {
        Logger.L.Debug("PersonalMessageEvent");
        Logger.L.Debug(input);
    }

    public override void ChannelMessageEvent<T>(EventSubjectOutput<EventSubjectDataBusiness<EventBodyChannelMessage<T>>> input)
    {
        Logger.L.Debug("ChannelMessageEvent");
        Logger.L.Debug(input);
    }
    
}