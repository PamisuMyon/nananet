using System.Text.RegularExpressions;
using Nananet.Adapter.Fanbook.Models;
using Nananet.Core.Models;
using FanbookMessage = Nananet.Adapter.Fanbook.Models.Message;
using FanbookMember = Nananet.Adapter.Fanbook.Models.Member;
using FanbookUser = Nananet.Adapter.Fanbook.Models.User;
using Member = Nananet.Core.Models.Member;
using Message = Nananet.Core.Models.Message;
using User = Nananet.Core.Models.User;

namespace Nananet.Adapter.Fanbook;

public static class Converter
{
    private static readonly Regex s_emojiRegex = new("\\[\\.+\\]", RegexOptions.Multiline);
    
    public static Message? FromMessage(FanbookMessage input)
    {
        // 目前先只处理文本消息
        if (input.ContentType != EContentType.Text || input.TextContent == null)
            return null;
        
        var msg = new Message();
        msg.MessageId = input.MessageId;
        msg.GuildId = input.GuildId;
        msg.ChannelId = input.ChannelId;
        msg.AuthorId = input.UserId;
        msg.Author = FromUser(input.Author);
        msg.Author.Id = msg.AuthorId;
        if (input.Member != null)
            msg.Member = FromMember(input.Member);
        if (input.QuoteL2 != null)
        {
            msg.Reference = new MessageReference
            {
                MessageId = input.QuoteL2
            };
        }
        else if (input.QuoteL1 != null) 
        {
            msg.Reference = new MessageReference
            {
                MessageId = input.QuoteL1
            };
        }
        msg.Mentions = input.Mentions?.Select(it =>
        {
            return new User()
            {
                NickName = it.NickName,
                Id = it.UserId,
            };
        }).ToList();
        msg.Time = DateTimeOffset.FromUnixTimeMilliseconds(input.Time).DateTime;
        msg.EditedTime = msg.Time;
        msg.Origin = input;

        if (!string.IsNullOrEmpty(input.TextContent.Text))
        {
            msg.RawContent = input.TextContent.Text;
            msg.OriginalContent = input.TextContent.Text;
            // 移除表情
            msg.Content = s_emojiRegex.Replace(input.TextContent.Text, "");
            // 移除at文本
            if (msg.Mentions != null)
            {
                foreach (var it in msg.Mentions)
                {
                    msg.Content = msg.Content.Replace($"${{@!{it.Id}}}", "");
                }
            }
        }
        
        return msg;
    }
    
    public static User FromUser(FanbookUser input)
    {
        return new User
        {
            // Id = input.Id,
            UserName = input.Username,
            NickName = input.Nickname,
            Avatar = input.Avatar,
            IsBot = input.Bot,
        };
    }

    public static Member FromMember(FanbookMember input)
    {
        return new Member
        {
            NickName = input.Nick,
            Roles = input.Roles,
        };
    }
    
}