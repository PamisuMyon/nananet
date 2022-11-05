using MongoDB.Entities;
using Nananet.Core.Models;

namespace Nananet.App.Nana.Models;

[Collection("action-logs")]
public class ActionLog : Entity, ICreatedOn
{
    public string Command { get; set; }
    public string Content { get; set; }
    public string Reply { get; set; }
    public string UserId { get; set; }
    public string NickName { get; set; }
    public string ChannelId { get; set; }
    public string GroupId { get; set; }
    public bool IsPersonal { get; set; }
    public DateTime CreatedOn { get; set; }

    public static async Task Log(string command, Message input, string? reply)
    {
        var log = new ActionLog
        {
            Command = command,
            Reply = reply!,
            UserId = input.AuthorId,
            NickName = input.Author.NickName,
            ChannelId = input.ChannelId,
            GroupId = input.GroupId,
            IsPersonal = input.IsPersonal
        };
        if (input is TextMessage text)
            log.Content = text.Content;
        await log.SaveAsync();
    }
    
}