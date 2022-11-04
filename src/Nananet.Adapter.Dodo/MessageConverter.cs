using DoDo.Open.Sdk.Models.Events;
using DoDo.Open.Sdk.Models.Messages;
using Nananet.Core.Models;

namespace Nananet.Adapter.Dodo;

public enum DodoMessageType
{
    None = 0,
    Text = 1,
    Picture = 2,
    Video = 3,
    Share = 4,
    File = 5,
    Card = 6,
}

public static class MessageConverter
{
    public static Message? FromChannelMessageEvent<T>(EventSubjectOutput<EventSubjectDataBusiness<EventBodyChannelMessage<T>>> input) where T : MessageBodyBase
    {
        Message? message = null;
        var eventBody = input.Data.EventBody;
        if (eventBody.MessageBody is MessageBodyText text)
        {
            message = FromMessageBody(text);
        } 
        else if (eventBody.MessageBody is MessageBodyPicture picture)
        {
            message = FromMessageBody(picture);
        }
        if (message != null)
        {
            message.EventId = input.Data.EventId;
            message.EventType = input.Data.EventType;
            message.Timestamp = input.Data.Timestamp;
            message.MessageId = input.Data.EventBody.MessageId;
            message.Reference = new Reference
            {
                AuthorId = input.Data.EventBody.Reference.DodoSourceId,
                MessageId = input.Data.EventBody.Reference.MessageId,
                NickName = input.Data.EventBody.Reference.NickName,
            };
            message.MessageType = input.Data.EventBody.MessageType;
            message.AuthorId = input.Data.EventBody.DodoSourceId;
            message.ChannelId = input.Data.EventBody.ChannelId;
            message.GroupId = input.Data.EventBody.IslandSourceId;
            message.Author = new User
            {
                Avatar = input.Data.EventBody.Personal.AvatarUrl,
                NickName = input.Data.EventBody.Personal.NickName,
                UserId = message.AuthorId
            };
            message.IsPersonal = false;
        }
        return message;
    }
    
    public static Message? FromPersonalMessageEvent<T>(EventSubjectOutput<EventSubjectDataBusiness<EventBodyPersonalMessage<T>>> input) where T : MessageBodyBase
    {
        Message? message = null;
        var eventBody = input.Data.EventBody;
        if (eventBody.MessageBody is MessageBodyText text)
        {
            message = FromMessageBody(text);
        } 
        else if (eventBody.MessageBody is MessageBodyPicture picture)
        {
            message = FromMessageBody(picture);
        }
        if (message != null)
        {
            message.EventId = input.Data.EventId;
            message.EventType = input.Data.EventType;
            message.Timestamp = input.Data.Timestamp;
            message.MessageId = input.Data.EventBody.MessageId;
            message.MessageType = input.Data.EventBody.MessageType;
            message.AuthorId = input.Data.EventBody.DodoSourceId;
            message.Author = new User
            {
                Avatar = input.Data.EventBody.Personal.AvatarUrl,
                NickName = input.Data.EventBody.Personal.NickName,
                UserId = message.AuthorId
            };
            message.IsPersonal = true;
        }
        return message;
    }
    
    public static TextMessage FromMessageBody(MessageBodyText text)
    {
        return new TextMessage
        {
            Content = text.Content,
            OriginalContent = text.Content
        };
    }
    
    public static PictureMessage FromMessageBody(MessageBodyPicture picture)
    {
        return new PictureMessage
        {
            Url = picture.Url,
            Height = picture.Height,
            Width = picture.Width,
            IsOriginal = picture.IsOriginal
        };
    }
}