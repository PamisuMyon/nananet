using DoDo.Open.Sdk.Models;
using DoDo.Open.Sdk.Models.Events;
using DoDo.Open.Sdk.Services;
using Nananet.Core.Models;
using Nananet.Core.Utils;

namespace Nananet.Adapter.Dodo;

public class NadoEventProcessService : EventProcessService
{

    protected OpenApiService _openApiService;
    protected OpenApiOptions _openApiOptions;

    public event Action<Message> OnMessage;

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
        Logger.L.Debug("NadoEventProcessService PersonalMessageEvent");
        var message = MessageConverter.FromPersonalMessageEvent(input);
        if (message != null)
        {
            OnMessage.Invoke(message);
        }
    }

    public override void ChannelMessageEvent<T>(EventSubjectOutput<EventSubjectDataBusiness<EventBodyChannelMessage<T>>> input)
    {
        Logger.L.Debug("NadoEventProcessService ChannelMessageEvent");
        var message = MessageConverter.FromChannelMessageEvent(input);
        if (message != null)
        {
            OnMessage.Invoke(message);
        }
    }
    
}