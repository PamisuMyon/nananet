using System.Text.RegularExpressions;
using Nado.App.Nana.Models;
using Nado.Core;
using Nado.Core.Commands;
using Nado.Core.Models;

namespace Nado.App.Nana.Functions.Picture;

public class PictureCommand : Command
{
    public override string Name => "picture";

    protected Regex[] _regexes =
    {
        new ("来(点|电|份|张)猫(猫|图)"),
        new ("來(點|電|份|張)貓(貓|圖)"),
        new ("来(点|电|份|张)狗(狗|图)"),
        new ("來(點|電|份|張)狗(狗|圖)"),
        new ("来(点|电|份|张)(涩|瑟|色|美|帅)?图(片|图)? *　*(.*)"),
        new ("來(點|電|份|張)(澀|瑟|色|美|帥)?圖(片|圖)? *　*(.*)"),
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