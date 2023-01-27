using System.Text.RegularExpressions;
using Nananet.Core.Models;
using QQGuildMessage = QQChannelFramework.Models.MessageModels.Message;
using QQGuildUser = QQChannelFramework.Models.User;
using QQGuildMember = QQChannelFramework.Models.Member;

namespace Nananet.Adapter.QQGuild;

public static class Converter
{

    private static readonly Regex s_emojiRegex = new("<emoji:\\d+>", RegexOptions.Multiline);

    public static Message FromMessage(QQGuildMessage input)
    {
        var msg = new Message();
        msg.MessageId = input.Id;
        msg.GuildId = input.GuildId;
        msg.ChannelId = input.ChannelId;
        msg.AuthorId = input.Author.Id;
        msg.Author = FromUser(input.Author);
        if (input.Member != null)
            msg.Member = FromMember(input.Member);
        if (input.MessageReference != null)
        {
            msg.Reference = new MessageReference
            {
                MessageId = input.MessageReference.MessageId
            };
        }
        msg.Metions = input.Mentions?.Select(FromUser).ToList();
        msg.Time = input.Time;
        msg.EditedTime = input.EditedTime;
        msg.Origin = input;

        if (input.Attachments != null && input.Attachments.Count != 0)
        {
            msg.Attachments = input.Attachments.Select(it =>
            {
                var url = it.Url;
                if (!url.StartsWith("http"))
                    url = "https://" + url;
                return new MessageAttachment { Url = url };
            }).ToList();
        }
        if (!string.IsNullOrEmpty(input.Content))
        {
            msg.RawContent = input.Content;
            msg.OriginalContent = input.Content;
            // 移除表情
            msg.Content = s_emojiRegex.Replace(input.Content, "");
            // 移除at文本
            if (msg.Metions != null)
            {
                foreach (var it in msg.Metions)
                {
                    msg.Content = msg.Content.Replace($"<@!{it.Id}>", "");
                }
            }
        }
        
        return msg;
    }

    public static User FromUser(QQGuildUser input)
    {
        return new User
        {
            Id = input.Id,
            UserName = input.UserName,
            NickName = input.UserName,
            Avatar = input.Avatar,
            IsBot = input.IsBot,
        };
    }

    public static Member FromMember(QQGuildMember input)
    {
        return new Member
        {
            NickName = input.Nick,
            Roles = input.Roles,
        };
    }
    
}