using System.Text.RegularExpressions;
using Nananet.Core;
using Nananet.Core.Commands;
using Nananet.Core.Models;

namespace Nananet.App.Nana.Functions.Gacha;

public class FakeGachaCommand : Command
{
    public override string Name => "gacha";

    protected Regex _regex = new("(寻访十次|尋訪十次|寻访十连|尋訪十連|十次寻访|十次尋訪|十连|十連|抽十次|寻访|尋訪|单抽|單抽) *(.*)");

    public override Task<CommandTestInfo> Test(Message input, CommandTestOptions options)
    {
        if (!input.HasContent()) return Task.FromResult(NoConfidence);
        if (_regex.IsMatch(input.Content))
        {
            input.Content = "来点图 明日方舟";
            return Task.FromResult(NoConfidence);
        }

        return Task.FromResult(NoConfidence);
    }

    public override Task<CommandResult> Execute(IBot bot, Message input, CommandTestInfo testInfo)
    {
        return Task.FromResult(Failed);
    }
}