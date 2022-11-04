using Kook;
using Kook.WebSocket;
using Nananet.Core.Models;

namespace Nananet.Adapter.Kook;

public static class Converter
{

    public static Message? FromSocketMessage(SocketMessage input)
    {
        Message? msg = null;
        if (input.Type == MessageType.Text)
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
        }

        return msg;
    }

    private static TextMessage FromFromTextSocketMessage(SocketMessage input)
    {
        var msg = new TextMessage();
        msg.Content = input.CleanContent;
        msg.OriginalContent = msg.Content;
        // msg.RichContent = input.Content;
        // msg.RawContent = input.RawContent;
        return msg;
    }

    public static User FromSocketUser(SocketUser input)
    {
        return new User
        {
            UserId = input.Id.ToString(),
            NickName = input.Username,
            Avatar = input.Avatar,
            IsBot = input.IsBot != null && input.IsBot.Value,
        };
    }
}