using System.Text.RegularExpressions;
using Nananet.App.Nana.Models;
using Nananet.Core;
using Nananet.Core.Commands;
using Nananet.Core.Models;

namespace Nananet.App.Nana.Functions.Help;

public class HelpCommand : Command
{
    public override string Name => "help";

    protected string _keyHelp;
    protected Regex _regex = new("^(帮助|幫助|菜单|menu|help)", RegexOptions.IgnoreCase);

    public HelpCommand(string keyHelp = "help")
    {
        _keyHelp = keyHelp;
    }
    
    public override Task<CommandTestInfo> Test(Message input, CommandTestOptions options)
    {
        if (options.IsCommand) return Task.FromResult(NoConfidence);
        if (!input.HasContent()) return Task.FromResult(NoConfidence);
        if (_regex.IsMatch(input.Content)) return Task.FromResult(FullConfidence);
        return Task.FromResult(NoConfidence);
    }

    public override async Task<CommandResult> Execute(IBot bot, Message input, CommandTestInfo testInfo)
    {
        var reply = Sentence.GetOne(_keyHelp);
        await bot.ReplyTextMessage(input, reply);
        await ActionLog.Log(Name, input, reply);
        return Executed;
    }
    
}