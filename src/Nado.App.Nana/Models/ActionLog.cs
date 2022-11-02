using MongoDB.Entities;
using Nado.Core.Models;

namespace Nado.App.Nana.Models;

[Collection("action-logs")]
public class ActionLog : Entity, ICreatedOn
{
    public string Command { get; set; }
    public string Content { get; set; }
    public string Reply { get; set; }
    public string UserId { get; set; }
    public string NickName { get; set; }
    public string ChannelId { get; set; }
    public string IslandId { get; set; }
    public bool IsPersonal { get; set; }
    public DateTime CreatedOn { get; set; }

    public static async Task Log(string command, Message input, string? reply)
    {
        var log = new ActionLog
        {
            Command = command,
            Reply = reply,
            UserId = input.DodoId,
            NickName = input.Personal.NickName,
            ChannelId = input.ChannelId,
            IslandId = input.IslandId,
            IsPersonal = input.IsPersonal
        };
        if (input is TextMessage text)
            log.Content = text.Content;
        await log.SaveAsync();
    }
    
}