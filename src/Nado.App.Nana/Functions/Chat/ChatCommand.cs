using System.Text.RegularExpressions;
using Nado.App.Nana.Models;
using Nado.Core;
using Nado.Core.Commands;
using Nado.Core.Models;
using Nado.Core.Utils;

namespace Nado.App.Nana.Functions.Chat;

public class ChatCommand : Command
{

    public override string Name => "chat";

    protected Regex _contentRegex = new("^image:(.+)"); 
    
    public override Task<CommandTestInfo> Test(Message input, CommandTestOptions options)
    {
        if (options.IsCommand) return Task.FromResult(NoConfidence);
        if (input is not TextMessage) return Task.FromResult(NoConfidence);
        return Task.FromResult(FullConfidence);
    }

    public override async Task<CommandResult> Execute(NadoBot bot, Message input, CommandTestInfo testInfo)
    {
        if (input is not TextMessage text) return Failed;

        if (!text.IsPersonal
            && (string.IsNullOrEmpty(text.Content)
                || string.IsNullOrEmpty(text.Content.Replace("　", ""))))
        {
            var s = Sentence.GetOne("atOnly");
            await bot.ReplyTextMessage(input, s);
            return Executed;
        }

        var content = Chatter.Wash(text.Content);
        var convReply = Chatter.GetConversationReply(content);
        if (convReply != null && convReply.Value.Priority == "2")
        {
            await DoReply(bot, input, content);
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

    protected async Task DoReply(NadoBot bot, Message input, string reply)
    {
        var m = _contentRegex.Match(reply);
        if (m.Success)
        {
            await bot.ReplyPictureUrlMessage(input, m.Groups[2].Value);
        }

        await bot.ReplyTextMessage(input, reply);
    }
    
}