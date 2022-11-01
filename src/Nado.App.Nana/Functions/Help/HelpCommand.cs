using System.Text.RegularExpressions;
using Nado.App.Nana.Models;
using Nado.Core;
using Nado.Core.Commands;
using Nado.Core.Models;

namespace Nado.App.Nana.Functions.Help;

public class HelpCommand : Command
{
    public override string Name => "help";

    protected Regex _regex = new("^(帮助|help)");
    
    public override Task<CommandTestInfo> Test(Message input, CommandTestOptions options)
    {
        if (options.IsCommand) return Task.FromResult(NoConfidence);
        if (input is not TextMessage text) return Task.FromResult(NoConfidence);
        if (_regex.IsMatch(text.Content)) return Task.FromResult(FullConfidence);
        return Task.FromResult(NoConfidence);
    }

    public override async Task<CommandResult> Execute(NadoBot bot, Message input, CommandTestInfo testInfo)
    {
        var reply = Sentence.GetOne("help");
        await bot.ReplyTextMessage(input, reply);
        return Executed;
    }
    
}