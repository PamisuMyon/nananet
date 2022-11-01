using Nado.App.Nana.Models;
using Nado.Core;
using Nado.Core.Commands;
using Nado.Core.Models;

namespace Nado.App.Nana.Functions.Chat;

public class ChatCommand : Command
{

    public override string Name => "chat";
    
    public override Task<CommandTestInfo> Test(Message input, CommandTestOptions options)
    {
        if (options.IsCommand) return Task.FromResult(NoConfidence);
        if (input is not TextMessage) return Task.FromResult(NoConfidence);
        return Task.FromResult(FullConfidence);
    }

    public override async Task<CommandResult> Execute(NadoBot bot, Message input, CommandTestInfo testInfo)
    {
        var text = input as TextMessage;
        if (text == null) return Failed;

        if (!text.IsPersonal
            && (string.IsNullOrEmpty(text.Content)
                || string.IsNullOrEmpty(text.Content.Replace("　", ""))))
        {
            var reply = Sentence.GetOne("atOnly");
            await bot.ReplyTextMessage(input, reply);
            return Executed;
        }

        return Executed;
    }
    
}