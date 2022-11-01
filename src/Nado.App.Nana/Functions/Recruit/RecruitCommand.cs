using System.Text.RegularExpressions;
using Nado.App.Nana.Models;
using Nado.Core;
using Nado.Core.Commands;
using Nado.Core.Models;

namespace Nado.App.Nana.Functions.Recruit;

public class RecruitCommand : Command
{
    public override string Name => "recruit"; 
    
    protected Regex[] _regexes =
    {
        new ("(公招|公开招募)(查询)?"),
        new ("(公招|公開招募)(查詢)?"),
    };
    
    public override Task<CommandTestInfo> Test(Message input, CommandTestOptions options)
    {
        if (input is TextMessage text)
        {
            if (_regexes.Any(regex => regex.IsMatch(text.Content)))
            {
                return Task.FromResult(FullConfidence);
            }
        }
        return Task.FromResult(NoConfidence);
    }

    public override async Task<CommandResult> Execute(NadoBot bot, Message input, CommandTestInfo testInfo)
    {
        await bot.ReplyTextMessage(input, Sentence.GetOne("underDevelopment"));
        return Executed;
    }
}