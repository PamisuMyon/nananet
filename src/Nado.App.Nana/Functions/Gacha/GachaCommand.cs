using Nado.Core;
using Nado.Core.Commands;
using Nado.Core.Models;

namespace Nado.App.Nana.Functions.Gacha;

public class GachaCommand : Command
{
    public override string Name => "gacha";
    public override Task<CommandTestInfo> Test(Message input, CommandTestOptions options)
    {
        throw new NotImplementedException();
    }

    public override Task<CommandResult> Execute(NadoBot bot, Message input, CommandTestInfo testInfo)
    {
        throw new NotImplementedException();
    }
}