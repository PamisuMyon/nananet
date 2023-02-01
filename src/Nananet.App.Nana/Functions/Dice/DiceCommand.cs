using System.Text.RegularExpressions;
using Nananet.App.Nana.Models;
using Nananet.Core;
using Nananet.Core.Commands;
using Nananet.Core.Models;
using Nananet.Core.Utils;

namespace Nananet.App.Nana.Functions.Dice;

public class DiceCommand : Command
{

    public override string Name => "dice";
    
    protected Regex _regexLax = new ("((\\d+) ?\\+ ?)?((\\d*) )?r?(\\d*)d(\\d+)( ?\\+ ?(\\d+))?=?", RegexOptions.IgnoreCase);
    protected Regex _regexIgnore = new("^D32钢$"); // hard-code
    
    
    public override Task<CommandTestInfo> Test(Message input, CommandTestOptions options)
    {
        if (input.HasContent())
        {
            if (_regexIgnore.IsMatch(input.Content)) return Task.FromResult(NoConfidence);
            if (_regexLax.IsMatch(input.Content)) return Task.FromResult(FullConfidence);
        }
        return Task.FromResult(NoConfidence);
    }

    public override async Task<CommandResult> Execute(IBot bot, Message input, CommandTestInfo testInfo)
    {
        if (!input.HasContent()) return Failed;
        
        var lines = input.Content.Split('\n');
        var options = new List<DiceOptions>();
        foreach (var line in lines)
        {
            var r = _regexLax.Match(line);
            if (!r.Success) continue;
            var option = new DiceOptions {
                Add = CommonUtil.GetInt(r.Groups[2].Value, 0),
                Rounds = CommonUtil.GetInt(r.Groups[4].Value, 1),
                Times = CommonUtil.GetInt(r.Groups[5].Value, 1),
                Dice = CommonUtil.GetInt(r.Groups[6].Value, 0),
                Add2 = CommonUtil.GetInt(r.Groups[8].Value, 0),
            };
            options.Add(option);
        }
        var reply = Dicer.Rolls(options, Sentence.Get("commandError"), Sentence.Get("diceTooMany"));
        if (!input.IsPersonal && reply.Contains('\n')) {
            reply = '\n' + reply;
        }

        await bot.ReplyTextMessage(input, reply);
        await ActionLog.Log(Name, bot, input, reply);
        return Executed;
    }
    
}