using System.Text.RegularExpressions;
using Nananet.App.Nana.Commons;
using Nananet.App.Nana.Models;
using Nananet.App.Nana.Models.Ak;
using Nananet.Core;
using Nananet.Core.Commands;
using Nananet.Core.Models;
using Nananet.Core.Utils;

namespace Nananet.App.Nana.Functions.Wiki;

public class AkOperatorSkillMasteryCommand : Command
{
    public override string Name => "wiki/operatorSkillMastery";

    private Regex _regex = new("(.+?) *　*的?第?([0-9零一二三四五六七八九十百千万亿兆京垓]+)个?技能的?(专精)?(材料)?");
    private Dictionary<string, string> _classes = new();

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
    }

    public override async Task<CommandTestInfo> Test(Message input, CommandTestOptions options)
    {
        if (!input.HasContent()) return NoConfidence;
        if (_regex.IsMatch(input.Content))
        {
            var m = _regex.Match(input.Content);
            var c = await Character.FindOneByName(m.Groups[1].Value, false);
            if (c != null)
            {
                var b = int.TryParse(m.Groups[2].Value, out var skillNum);
                if (!b)
                    skillNum = (int)ZhDigitToArabic.Convert(m.Groups[2].Value);
                return new CommandTestInfo
                {
                    Confidence = 1,
                    Data = (c, skillNum)
                };
            }
        }

        return NoConfidence;
    }

    public override async Task<CommandResult> Execute(IBot bot, Message input, CommandTestInfo testInfo)
    {
        if (testInfo.Data is not ValueTuple<Character, int> data) return Failed;
        var (c, skillNum) = data;

        var reply = "";
        if (_classes.ContainsKey(c.Profession.ToLower()))
        {
            var skills = c.Skills;
            if (skills is { Count: > 0 })
            {
                if (skillNum > 0 && skillNum <= skills.Count)
                {
                    var cond = skills[skillNum - 1].LevelUpCostCond;
                    if (cond is { Count: > 0 })
                    {
                        for (var i = 0; i < cond.Count; i++)
                        {
                            if (i == 0)
                                reply += "\n🟢专精等级Ⅰ\n";
                            else if (i == 1)
                                reply += "\n🔵专精等级Ⅱ\n";
                            else if (i == 2)
                                reply += "\n🟣专精等级Ⅲ\n";
                            foreach (var it in cond[i].LevelUpCost)
                            {
                                var item = await Item.FindOneById(it.Id);
                                if (item != null)
                                    reply += $"[{item.Name}x{it.Count}] ";
                            }
                        }
                    }
                    else
                    {
                        reply = $"干员{c.Name}的{skillNum}技能无法专精。";
                    }
                }
                else
                {
                    reply = $"博士，干员{c.Name}没有这个技能。";
                }
            }
            else
            {
                reply = $"干员{c.Name}没有技能哦。";
            }
        }
        else
        {
            reply = $"博士，{c.Name}可能不是干员哦。";
        }

        await bot.ReplyTextMessage(input, reply);
        await ActionLog.Log(Name, input, reply);
        return Executed;
    }
}