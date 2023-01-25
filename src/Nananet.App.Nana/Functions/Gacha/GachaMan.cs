using System.Dynamic;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Entities;
using Nananet.App.Nana.Models;
using Nananet.App.Nana.Models.Ak;
using Nananet.Core.Utils;

namespace Nananet.App.Nana.Functions.Gacha;

public static class GachaMan
{
    public static Dictionary<int, int> Rarities = new Dictionary<int, int>()
    {
        { 1, 2 },
        { 41, 3 },
        { 91, 4 },
        { 99, 5 },
    };

    public struct Operator
    {
        public string Name { get; set; }
        public int Rarity { get; set; }
    }

    public struct Pool
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string[] Ignore { get; set; }
        public string[] Extra { get; set; }
        public List<Operator> Operators { get; set; }
        public List<string> PickupNames { get; set; }
        public Pickup Pickup { get; set; }
    }

    private static Dictionary<string, Pool> _pools = new();

    public static async Task Refresh()
    {
        var allOps = new List<Operator>();
        var notOps = new List<Operator>();
        var availOps = new List<Operator>();
        _pools.Clear();

        Logger.L.Info("Updating gacha pools...");

        var pickups = await AkMisc.FindByName<List<Pickup>>("pickups");
        if (pickups == null || pickups.Count == 0)
        {
            Logger.L.Error("No pickups found, please check collection \"ak-misc\"");
            return;
        }

        var classes = await AkMisc.FindByName("classes");
        var special = await AkMisc.FindByName("specialOperators");
        if (classes == null || special == null)
        {
            Logger.L.Error("Configs of classes or special not found.");
            return;
        }

        var charsCur = await DB.Find<Character>().ExecuteCursorAsync();
        await charsCur.ForEachAsync(c =>
        {
            if (!classes.Contains(c.Profession))
            {
                notOps.Add(new Operator
                {
                    Name = c.Name,
                    Rarity = c.Rarity
                });
                return;
            }

            var op = new Operator
            {
                Name = c.Name.Trim(),
                Rarity = c.Rarity
            };
            allOps.Add(op);

            if (IsOperatorAvailable(special, op.Name))
                availOps.Add(op);
        });

        foreach (var pickup in pickups)
        {
            var pool = new Pool
            {
                Name = pickup.Name,
                Type = pickup.Type,
                Ignore = pickup.Ignore ?? Array.Empty<string>(),
                Extra = pickup.Extra ?? Array.Empty<string>(),
                Operators = new List<Operator>(),
                PickupNames = new List<string>(),
                Pickup = pickup,
            };
            pool.PickupNames.AddRange(pickup.Six);
            pool.PickupNames.AddRange(pickup.Five);
            pool.PickupNames.AddRange(pickup.Four);

            foreach (var it in availOps)
            {
                if (it.Rarity < 2) continue;
                if (pool.Ignore != null && pool.Ignore.Contains(it.Name)) continue;
                if (pool.PickupNames.Contains(it.Name)) continue;
                if (it.Rarity == 5 && pickup.Is6UpOnly) continue;
                if (it.Rarity == 4 && pickup.Is5UpOnly) continue;
                if (it.Rarity == 3 && pickup.Is4UpOnly) continue;

                pool.Operators.Add(it);
            }

            _pools.Add(pool.Name, pool);
        }

        Logger.L.Info("Gacha pools updated!");
    }

    public static bool IsOperatorAvailable(BsonDocument special, string opName)
    {
        if (special["special"].AsBsonArray.Any(it => it.AsString == opName))
            return false;
        return true;
    }

    public static List<Operator> GetOperatorsByRarity(IEnumerable<Operator> operators, int rarity)
    {
        return operators.Where(it => it.Rarity == rarity).ToList();
    }

    public static (List<Operator> results, int waterLevel) Roll(string poolName, int times, int waterLevel = 0)
    {
        var pool = _pools[poolName];
        var results = new List<Operator>();
        for (var i = 0; i < times; i++)
        {
            // 稀有度
            var num = CommonUtil.RandomInt(1, 100);
            var rarity = 0;

            foreach (var key in Rarities.Keys)
            {
                if (num >= key)
                    rarity = Rarities[key];
            }

            if (waterLevel > 50)
            {
                var up = 99 - (waterLevel - 50) * 2;
                if (num >= up)
                {
                    rarity = 5;
                }
            }

            if (rarity == 5)
            {
                waterLevel = 0;
            }
            else
            {
                waterLevel++;
            }

            // 是否位于pickup
            var ops = new List<Operator>();
            if (rarity == 5)
            {
                if (pool.Pickup.Six.Length > 0)
                {
                    var num6 = pool.Pickup.Is6UpOnly ? 100 : CommonUtil.RandomInt(1, 100);
                    var p = 51;
                    if (pool.Type.StartsWith("limited"))
                    {
                        p = 31; // 限定池6星up占70%
                    }

                    if (num6 >= p)
                    {
                        foreach (var item in pool.Pickup.Six)
                        {
                            ops.Add(new Operator
                            {
                                Name = item + "  ↑",
                                Rarity = rarity
                            });
                        }
                    }
                }
            }
            else if (rarity == 4)
            {
                if (pool.Pickup.Five.Length > 0)
                {
                    var num5 = pool.Pickup.Is5UpOnly ? 100 : CommonUtil.RandomInt(1, 100);
                    if (num5 >= 51)
                    {
                        foreach (var item in pool.Pickup.Five)
                        {
                            ops.Add(new Operator
                            {
                                Name = item + "  ↑",
                                Rarity = rarity
                            });
                        }
                    }
                }
            }
            else if (rarity == 3)
            {
                if (pool.Pickup.Four.Length > 0)
                {
                    var num4 = pool.Pickup.Is4UpOnly ? 100 : CommonUtil.RandomInt(1, 100);
                    if (num4 >= 81)
                    {
                        foreach (var item in pool.Pickup.Four)
                        {
                            ops.Add(new Operator
                            {
                                Name = item + "  ↑",
                                Rarity = rarity
                            });
                        }
                    }
                }
            }

            // 不位于pickup
            if (ops.Count == 0)
            {
                ops = GetOperatorsByRarity(pool.Operators, rarity);
                // 5倍权值 目前仅6星
                if (rarity == 5 && pool.Extra.Length > 0)
                {
                    for (var j = 0; j < 5; j++)
                    {
                        foreach (var item in pool.Extra)
                        {
                            ops.Add(new Operator
                            {
                                Name = item + "  ▲",
                                Rarity = rarity
                            });
                        }
                    }
                }
            }

            // 抽取
            results.Add(ops.RandomElem());
        }

        return (results, waterLevel);
    }

    public static string randomTicketPrefix()
    {
        return Sentence.GetOne("ticketPrefix");
    }

    public static string BeautifyRollResults(List<Operator> results, string poolName)
    {
        var text = "";
        if (results.Count == 1)
        {
            text += $"使用{randomTicketPrefix()}600合成玉进行了一次【{poolName}】寻访，结果：\n";
            text += GetStarText(results[0].Rarity) + results[0].Name;
        }
        else if (results.Count == 10)
        {
            text += $"使用{randomTicketPrefix()}寻访凭证进行了十次【{poolName}】寻访，结果：\n";
            foreach (var result in results)
            {
                text += GetStarText(result.Rarity) + result.Name;
                text += '\n';
            }
        }

        return text;
    }

    public static string GetStarText(int rarity)
    {
        if (rarity <= 0) return "[✧]";
        var text = "[";
        for (var i = 0; i < rarity + 1; i++)
        {
            if (rarity > 3)
            {
                text += "★";
            }
            else
            {
                text += "☆";
            }
        }

        text += "]";
        return text;
    }

    public static Pool GetPool(string? name)
    {
        if (name == null) return _pools.First().Value;
        foreach (var key in _pools.Keys)
        {
            // 匹配卡池名
            if (key.Contains(name))
            {
                return _pools[key];
            }

            // 匹配up干员
            foreach (var op in _pools[key].PickupNames)
            {
                if (op == name)
                {
                    return _pools[key];
                }
            }
        }

        return _pools["标准"];
    }
    
}