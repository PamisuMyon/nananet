using System.Text.RegularExpressions;
using Nananet.App.Nana.Models;
using Nananet.Core;
using Nananet.Core.Commands;
using Nananet.Core.Models;
using Nananet.Core.Utils;

namespace Nananet.App.Nana.Functions.Chat;

public class ChatCommand : Command
{

    public override string Name => "chat";

    protected Regex _contentRegex = new("^image:(.+)"); 
    
    public override Task<CommandTestInfo> Test(Message input, CommandTestOptions options)
    {
        if (options.IsCommand) return Task.FromResult(NoConfidence);
        if (!input.HasContent()) return Task.FromResult(NoConfidence);
        return Task.FromResult(FullConfidence);
    }

    public override async Task<CommandResult> Execute(IBot bot, Message input, CommandTestInfo testInfo)
    {
        if (!input.HasContent()) return Failed;

        if (!input.IsPersonal
            && (string.IsNullOrEmpty(input.Content)
                || string.IsNullOrEmpty(input.Content.Replace("　", ""))))
        {
            var s = Sentence.GetOne("atOnly");
            await bot.ReplyTextMessage(input, s);
            return Executed;
        }

        var content = Chatter.Wash(input.Content);
        var convReply = Chatter.GetConversationReply(content);
        if (convReply != null && convReply.Value.Priority == "2")
        {
            await DoReply(bot, input, convReply.Value.Content);
            return Executed;
        }
         
        var chatReply = await Chatter.RequestNanaChat(content);
        string? reply = null;
        if (chatReply != null && !string.IsNullOrEmpty(chatReply.Value.reply))
        {
            if (convReply != null && convReply.Value.Priority == "1")
            {
                if (chatReply.Value.confidence > .5 && CommonUtil.RandomSingle(0, 1f) < .2f)
                {
                    reply = chatReply.Value.reply;
                }
                else
                {
                    reply = convReply.Value.Content;
                }
            }
            else
            {
                reply = chatReply.Value.reply;
            }
        }

        if (string.IsNullOrEmpty(reply))
            reply = Sentence.GetOne("test");

        await DoReply(bot, input, reply);
        return Executed;
    }

    protected async Task DoReply(IBot bot, Message input, string reply)
    {
        var m = _contentRegex.Match(reply);
        if (m.Success)
        {
            var dict = JsonUtil.FromJson<Dictionary<string, string>>(m.Groups[1].Value);
            if (dict != null && dict.ContainsKey(bot.AppSettings.Platform))
            {
                await bot.ReplyServerFileMessage(input, dict[bot.AppSettings.Platform], FileType.Image);
                await ActionLog.Log(Name, input, reply);
            }
            return;
        }

        await bot.ReplyTextMessage(input, reply);
        await ActionLog.Log(Name, input, reply);
    }
    
}