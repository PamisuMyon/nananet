using Kook;
using Kook.WebSocket;
using Nananet.Core;
using Nananet.Core.Models;

namespace Nananet.Adapter.Kook;

public static class Converter
{

    public static Message? FromSocketMessage(SocketMessage input)
    {
        Message? msg = null;
        if (input.Type == MessageType.KMarkdown || input.Type == MessageType.Text)
            msg = FromFromTextSocketMessage(input);
        
        if (msg != null)
        {
            msg.Author = FromSocketUser(input.Author);
            msg.AuthorId = input.Author.Id.ToString();
            msg.MessageId = input.Id.ToString();
            if (input.Channel is SocketTextChannel textChannel)
            {
                msg.ChannelId = textChannel.Id.ToString();
            }
            msg.Origin = input;
        }

        return msg;
    }

    private static Message FromFromTextSocketMessage(SocketMessage input)
    {
        var msg = new Message();
        msg.Content = input.CleanContent;
        msg.OriginalContent = msg.Content;
        msg.RawContent = input.RawContent;
        return msg;
    }

    public static User FromSocketUser(SocketUser input)
    {
        return new User
        {
            Id = input.Id.ToString(),
            NickName = input.Username,
            Avatar = input.Avatar,
            IsBot = input.IsBot != null && input.IsBot.Value,
        };
    }

    public static FileType FromAttachmentType(AttachmentType type)
    {
        return (FileType)type;
    }

    public static AttachmentType ToAttachmentType(FileType type)
    {
        return (AttachmentType)type;
    }
    
}