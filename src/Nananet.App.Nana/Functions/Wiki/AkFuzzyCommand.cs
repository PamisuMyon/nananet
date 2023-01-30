using Nananet.App.Nana.Commons;
using Nananet.App.Nana.Models;
using Nananet.App.Nana.Models.Ak;
using Nananet.Core;
using Nananet.Core.Commands;
using Nananet.Core.Models;
using Nananet.Core.Utils;

namespace Nananet.App.Nana.Functions.Wiki;

public class AkFuzzyCommand : Command
{
    public override string Name => "wiki/fuzzy";

    private AkFuzzySearcher[] _searchers =
    {
        new CharacterSearcher(),
        new EnemySearcher(),
        new ItemSearcher(),
        new RoguelikeItemSearcher()
    };

    public override async Task<CommandTestInfo> Test(Message input, CommandTestOptions options)
    {
        if (options.IsCommand) return NoConfidence;
        if (!input.HasContent() || string.IsNullOrEmpty(input.Content)) return NoConfidence;
        string? reply = null;
        // 精确
        foreach (var it in _searchers)
        {
            var result = await it.Search(input.Content, false);
            if (result.NotNullNorEmpty())
            {
                reply = result;
                break;
            }
        }

        if (reply.NotNullNorEmpty()) return new CommandTestInfo { Confidence = 1, Data = reply };

        // 模糊
        foreach (var it in _searchers)
        {
            var result = await it.Search(input.Content, true);
            if (result.NotNullNorEmpty())
            {
                reply = result;
                break;
            }
        }

        if (reply.NotNullNorEmpty()) return new CommandTestInfo { Confidence = 1, Data = reply };

        return NoConfidence;
    }

    public override async Task<CommandResult> Execute(IBot bot, Message input, CommandTestInfo testInfo)
    {
        if (testInfo.Data is not string reply) return Failed;
        await bot.ReplyTextMessage(input, reply);
        await ActionLog.Log(Name, bot, input, reply);
        return Executed;
    }
}

public abstract class AkFuzzySearcher
{
    public abstract Task<string?> Search(string content, bool isFuzzy = false);
}

public class CharacterSearcher : AkFuzzySearcher
{
    public override async Task<string?> Search(string content, bool isFuzzy = false)
    {
        var c = await Character.FindOneByName(content, isFuzzy);
        if (c == null) return null;

        var reply = "";
        if (c.ItemDesc.NotNullNorEmpty())
        {
            reply += "📄" + c.Name + NaUtil.GetRarityText(c.Rarity) + "\n";
            if (c.ItemUsage.NotNullNorEmpty())
                reply += c.ItemUsage + "\n";
            reply += c.ItemDesc;
        }
        else
        {
            if (c.Description.NotNullNorEmpty())
            {
                var desc = NaUtil.RemoveLabel(c.Description);
                if (!string.IsNullOrEmpty(desc))
                {
                    reply += "📃" + c.Name + NaUtil.GetRarityText(c.Rarity) + "\n";
                    reply += desc;
                }
            }
            else if (c.Name.NotNullNorEmpty())
            {
                reply += "📃" + c.Name + NaUtil.GetRarityText(c.Rarity) + "\n";
                reply += "暂无相关描述。";
            }
        }

        return reply;
    }
}

public class EnemySearcher : AkFuzzySearcher
{
    public override async Task<string?> Search(string content, bool isFuzzy = false)
    {
        var enemy = await Enemy.FindOneByName(content, isFuzzy);
        if (enemy == null) return null;

        var reply = "";
        reply += "📑" + enemy.Name;
        if (enemy.EnemyRace.NotNullNorEmpty())
            reply += " · " + enemy.EnemyRace;
        if (enemy.EnemyLevel.NotNullNorEmpty())
        {
            if (enemy.EnemyLevel == "ELITE")
                reply += " · 精英";
            else if (enemy.EnemyLevel == "BOSS")
                reply += " · 领袖";
        }

        reply += "\n";
        var fourD = $"耐久{enemy.Endure} 攻击{enemy.Attack} 防御{enemy.Defence} 法抗${enemy.Resistance}";
        reply += fourD;
        reply += "\n";
        if (enemy.Description.NotNullNorEmpty())
            reply += enemy.Description;
        else
            reply += "暂无相关描述。";
        if (enemy.Ability.NotNullNorEmpty())
        {
            reply += "\n";
            reply += NaUtil.RemoveLabel(enemy.Ability);
        }

        return reply;
    }
}

public class ItemSearcher : AkFuzzySearcher
{
    public override async Task<string?> Search(string content, bool isFuzzy = false)
    {
        var item = await Item.FindOneByName(content, isFuzzy);
        if (item == null) return null;

        var reply = "";
        var rarityStr = NaUtil.GetRarityText(item.Rarity);
        reply += "💎" + item.Name + rarityStr + "\n";
        if (item.Usage.NotNullNorEmpty())
            reply += $"{item.Usage}";
        if (item.Description.NotNullNorEmpty())
        {
            if (reply.NotNullNorEmpty())
                reply += "\n";
            reply += item.Description;
        }
        else if (item.Usage.NullOrEmpty())
            reply += "暂无相关描述。";

        if (item.ObtainApproach.NotNullNorEmpty())
            reply += $"\n获得方式：{item.ObtainApproach}";
        if (item.BuildingProductList is { Count: > 0 })
        {
            var product = item.BuildingProductList[0];
            // 加工材料
            if (product.RoomType == "WORKSHOP" && product.FormulaId.NotNullNorEmpty())
            {
                var formula = await WorkshopFormula.FindOneById(product.FormulaId);
                if (formula is { Costs.Count: > 0 })
                {
                    reply += "\n♻合成公式\n";
                    foreach (var cost in formula.Costs)
                    {
                        var subItem = await Item.FindOneById(cost.Id);
                        if (subItem != null)
                            reply += $"[{subItem.Name}x{cost.Count}] ";
                    }
                }
            }
        }

        return reply;
    }
}

public class RoguelikeItemSearcher : AkFuzzySearcher
{
    public override async Task<string?> Search(string content, bool isFuzzy = false)
    {
        var item = await RoguelikeItem.FindOneByName(content, isFuzzy);
        if (item == null) return null;

        var reply = "";
        reply += "💠" + item.Name;
        if (item.Usage != item.Name)
            reply += $"\n{item.Usage}";
        else if (item.ObtainApproach.NotNullNorEmpty())
            reply += $"\n{item.ObtainApproach}";

        if (item.Description.NotNullNorEmpty())
        {
            if (reply.NotNullNorEmpty())
                reply += "\n";
            reply += item.Description;
        }
        else if (item.Usage.NullOrEmpty())
            reply += "暂无相关描述。";

        if (item.Value > 0)
            reply += "\n售价：💰" + item.Value;

        if (item.UnlockCondDesc.NotNullNorEmpty())
            reply += "\n解锁条件：" + item.UnlockCondDesc;

        return reply;
    }
}
