using System.Text.RegularExpressions;
using MongoDB.Entities;
using Nananet.App.Nana.Models;
using Nananet.App.Nana.Models.Ak;
using Nananet.Core;
using Nananet.Core.Commands;
using Nananet.Core.Models;

namespace Nananet.App.Nana.Functions.Gacha;

public class GachaCommand : Command
{
    public override string Name => "gacha";

    protected Regex _regex = new("(寻访十次|尋訪十次|寻访十连|尋訪十連|十次寻访|十次尋訪|十连|十連|抽十次|寻访|尋訪|单抽|單抽) *(.*)");
    
    public override Task<CommandTestInfo> Test(Message input, CommandTestOptions options)
    {
        if (!input.HasContent()) return Task.FromResult(NoConfidence);
        if (_regex.IsMatch(input.Content))
        {
            var m = _regex.Match(input.Content);
            return Task.FromResult(new CommandTestInfo
            {
                Confidence = 1,
                Data = m
            });
        }

        return Task.FromResult(NoConfidence);
    }

    public override async Task<CommandResult> Execute(IBot bot, Message input, CommandTestInfo testInfo)
    {
        if (testInfo.Data is not Match m) return Failed;
        var cmd = m.Groups[1].Value;
        var pool = GachaMan.GetPool(m.Groups[2].Value);
        var times = cmd.Contains('十')? 10 : 1;

        var waterLevel = 0;
        // var targetId = input.IsPersonal ? input.AuthorId : input.ChannelId;
        var targetId = input.AuthorId;
        var gachaInfo = await GachaInfo.FindById(targetId);
        if (gachaInfo != null)
            waterLevel = gachaInfo.GetWaterLevel(pool.Type);
        else
            gachaInfo = GachaInfo.Create(targetId, pool.Type);

        var roll = GachaMan.Roll(pool.Name, times, waterLevel);
        var reply = GachaMan.BeautifyRollResults(roll.results, pool.Name);

        gachaInfo.UpdateWaterLevel(pool.Type, roll.waterLevel);
        await gachaInfo.SaveAsync();

        reply += $"\n距离上次抽到6★: {roll.waterLevel}次寻访";

        await bot.ReplyTextMessage(input, reply);
        await ActionLog.Log(Name, bot, input, reply);
        return Executed;
    }

}