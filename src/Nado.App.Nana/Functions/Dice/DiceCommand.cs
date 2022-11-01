using System.Text.RegularExpressions;
using Nado.App.Nana.Models;
using Nado.Core;
using Nado.Core.Commands;
using Nado.Core.Models;
using Nado.Core.Utils;

namespace Nado.App.Nana.Commands.Dice;

public class DiceCommand : Command
{

    public override string Name => "dice";
    
    protected Regex _regexLax = new ("((\\d+) ?\\+ ?)?((\\d*) )?r?(\\d*)d(\\d+)( ?\\+ ?(\\d+))?=?", RegexOptions.IgnoreCase);
    
    
    public override Task<CommandTestInfo> Test(Message input, CommandTestOptions options)
    {
        if (input is TextMessage text)
        {
            if (_regexLax.IsMatch(text.Content)) return Task.FromResult(FullConfidence);
        }
        return Task.FromResult(NoConfidence);
    }

    public override async Task<CommandResult> Execute(NadoBot bot, Message input, CommandTestInfo testInfo)
    {
        var msg = input as TextMessage;
        if (msg == null) return Failed;
        
        var lines = msg.Content.Split('\n');
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
        if (!msg.IsPersonal && reply.Contains('\n')) {
            reply = '\n' + reply;
        }

        await bot.ReplyTextMessage(msg, reply);
        // ActionLog.log(this.type, msg, reply);
        return Executed;
    }
    
}