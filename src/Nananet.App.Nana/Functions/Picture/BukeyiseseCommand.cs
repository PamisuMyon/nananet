using System.Text.RegularExpressions;
using Nananet.App.Nana.Models;
using Nananet.Core;
using Nananet.Core.Commands;
using Nananet.Core.Models;

namespace Nananet.App.Nana.Functions.Picture;

public class BukeyiseseCommand : Command
{
    public override string Name => "recruit"; 
    
    protected Regex[] _regexes =
    {
        new ("来(点|电|份|张)(涩|瑟|色|美|帅)?图(片|图)? *　*(.*)"),
        new ("來(點|電|份|張)(澀|瑟|色|美|帥)?圖(片|圖)? *　*(.*)"),
    };
    
    public override Task<CommandTestInfo> Test(Message input, CommandTestOptions options)
    {
        if (input is TextMessage text)
        {
            if (_regexes.Any(regex => regex.IsMatch(text.Content)))
                return Task.FromResult(FullConfidence);
        }
        return Task.FromResult(NoConfidence);
    }

    public override async Task<CommandResult> Execute(IBot bot, Message input, CommandTestInfo testInfo)
    {
        await bot.ReplyTextMessage(input, Sentence.GetOne("underDevelopment"));
        return Executed;
    }
}