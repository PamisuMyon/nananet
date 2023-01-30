using System.Text.RegularExpressions;
using Nananet.App.Nana.Models;
using Nananet.App.Nana.Models.Ak;
using Nananet.Core;
using Nananet.Core.Commands;
using Nananet.Core.Models;
using Nananet.Core.Utils;

namespace Nananet.App.Nana.Functions.Wiki;

public class AkOperatorEvolveCommand : Command
{
    public override string Name => "wiki/operatorEvolve";

    private Regex _regex = new("(.+?) *　*的?精(一|二|壹|貳|1|2|英|(化)?)(材料)?");
    private Dictionary<string, string> _classes = new();
    private string[][] _evolveGoldCost = Array.Empty<string[]>();

    public override async Task Init(IBot bot)
    {
        await base.Init(bot);
        var classes = await AkMisc.FindByName<Dictionary<string, string>>("classes");
        if (classes == null)
        {
            Logger.L.Error("No classes found in collection ak-misc.");
            return;
        }
        _classes = classes;
        var evolveGoldCost = await AkMisc.FindByName<string[][]>("evolveGoldCost");
        if (evolveGoldCost == null)
        {
            Logger.L.Error("No evolveGoldCost found in collection ak-misc.");
            return;
        }
        _evolveGoldCost = evolveGoldCost;
    }

    public override async Task<CommandTestInfo> Test(Message input, CommandTestOptions options)
    {
        if (!input.HasContent()) return NoConfidence;
        if (_regex.IsMatch(input.Content))
        {
            var m = _regex.Match(input.Content);
            var c = await Character.FindOneByName(m.Groups[1].Value, false);
            if (c != null)
                return new CommandTestInfo
                {
                    Confidence = 1,
                    Data = c
                };
        }
        return NoConfidence;
    }

    public override async Task<CommandResult> Execute(IBot bot, Message input, CommandTestInfo testInfo)
    {
        if (testInfo.Data is not Character c)
            return Failed;
        var reply = "";
        if (_classes.ContainsKey(c.Profession.ToLower())) {
            var phases = c.Phases;
            if (phases is { Count: > 1 }) {
                var goldCosts = _evolveGoldCost[c.Rarity];
                for (var i = 1; i < phases.Count; i++) {
                    if (i == 1) {
                        reply += "\n🔷精英化0→1\n";
                        if (goldCosts[0].NotNullNorEmpty())
                            reply += $"[龙门币x{goldCosts[0]}] ";
                    } else if (i == 2) {
                        reply += "\n🔶精英化1→2\n";
                        if (goldCosts[1].NotNullNorEmpty())
                            reply += $"[龙门币x{goldCosts[1]}] ";
                    }
                    foreach (var it in phases[i].EvolveCost) {
                        var item = await Item.FindOneById(it.Id);
                        if (item != null)
                            reply += $"[{item.Name}x{it.Count}] ";
                    }
                }
            } else {
                reply = $"干员{c.Name}没有精英化阶段。";
            }
        } else {
            reply = $"博士，{c.Name}可能不是干员哦。";
        }

        await bot.ReplyTextMessage(input, reply);
        await ActionLog.Log(Name, bot, input, reply);
        return Executed;
    }
}