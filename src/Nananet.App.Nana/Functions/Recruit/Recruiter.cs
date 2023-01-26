using Nananet.App.Nana.Models;
using Nananet.App.Nana.Models.Ak;
using Nananet.Core.Utils;

namespace Nananet.App.Nana.Functions.Recruit;

public class Recruiter
{
    private static Recruiter? s_instance;

    public static Recruiter Instance
    {
        get
        {
            if (s_instance == null)
                s_instance = new Recruiter();
            return s_instance;
        }
    }

    private Recruiter()
    {
        if (s_instance != null)
            throw new Exception("Instance already exists.");
    }

    private readonly List<string> _classes = new() { "先锋", "狙击", "医疗", "术师", "近卫", "重装", "辅助", "特种" };
    private readonly List<string> _classesFormal = new();
    public List<string> Tags { get; } = new();


    public async Task Refresh()
    {
        var tags = await AkMisc.FindByName<string[]>("tags");
        Tags.Clear();
        Tags.AddRange(tags!);
        Tags.AddRange(new[] { "资深干员", "高级资深干员", "资深", "高资", "高姿", "支援机械" });
        Tags.AddRange(new[] { "远程位", "近战位", "远程", "近战" });
        Tags.AddRange(_classes);
        _classesFormal.Clear();
        _classes.ForEach(s => _classesFormal.Add(s + "干员"));
        Tags.AddRange(_classesFormal);
    }

    public async Task<List<RecruitResult>> Calculate(List<string> userTags)
    {
        // 别名替换
        for (var i = 0; i < userTags.Count; i++)
        {
            switch (userTags[i])
            {
                case "高姿":
                case "高资":
                    userTags[i] = "高级资深干员";
                    break;
                case "资深":
                    userTags[i] = "资深干员";
                    break;
                case "远程":
                    userTags[i] = "远程位";
                    break;
                case "近战":
                    userTags[i] = "近战位";
                    break;
                default:
                {
                    if (_classesFormal.Contains(userTags[i]))
                        userTags[i] = _classes[_classesFormal.IndexOf(userTags[i])];
                    break;
                }
            }
        }

        // 去重
        userTags = userTags.Distinct().ToList();
        Logger.L.Debug($"Recruit user tags: {string.Join(", ", userTags)}");

        List<RecruitResult> results = new();
        // 针对单tag、2tag、3tag分别判断是否有超过4星的干员
        var allClasses = await AkMisc.FindByName<Dictionary<string, string>>("classes");
        for (var j = 1; j <= Math.Min(userTags.Count, 3); j++)
        {
            var combs = CommonUtil.Combinations(userTags, j); // tag的指定数量组合
            foreach (var combTags in combs)
            {
                List<int> rarities = new();
                List<string> positions = new();
                List<string> classes = new();
                List<string> tags = new();
                var isConflict = false;
                // tag分类
                foreach (var tag in combTags)
                {
                    if (tag == "高级资深干员")
                    {
                        rarities.Add(5);
                        if (rarities.Count > 1)
                        {
                            // 组合中稀有度tag数量不可超过1
                            isConflict = true;
                            break;
                        }

                        continue;
                    }

                    if (tag == "资深干员")
                    {
                        rarities.Add(4);
                        if (rarities.Count > 1)
                        {
                            // 组合中稀有度tag数量不可超过1
                            isConflict = true;
                            break;
                        }

                        continue;
                    }

                    if (tag == "支援机械")
                    {
                        rarities.Add(0);
                        if (rarities.Count > 1)
                        {
                            // 组合中稀有度tag数量不可超过1
                            isConflict = true;
                            break;
                        }

                        continue;
                    }

                    if (tag == "远程位")
                    {
                        positions.Add("RANGED");
                        if (positions.Count > 1)
                        {
                            // 组合中位置tag数量不可超过1
                            isConflict = true;
                            break;
                        }

                        continue;
                    }

                    if (tag == "近战位")
                    {
                        positions.Add("MELEE");
                        if (positions.Count > 1)
                        {
                            // 组合中位置tag数量不可超过1
                            isConflict = true;
                            break;
                        }

                        continue;
                    }

                    // 职业tag
                    var tagIsClass = false;
                    foreach (var key in allClasses!.Keys)
                    {
                        if (allClasses[key] == tag)
                        {
                            if (classes.Count > 0)
                            {
                                // 组合中职业tag数量不可超过1
                                isConflict = true;
                                break;
                            }

                            classes.Add(key);
                            tagIsClass = true;
                            break;
                        }
                    }

                    if (tagIsClass)
                        continue;

                    // 其余类型tag
                    tags.Add(tag);
                }

                if (isConflict)
                    // 如果当前组合存在冲突则跳过
                    continue;

                // 执行查询
                int? paramRarity = rarities.Count > 0 ? rarities[0] : null;
                var chars = await Character.FindByRecruit(paramRarity, positions.GetElemSafe(0)!, classes.GetElemSafe(0)!, tags);
                // 期望效果为仅保留必出、小车及4星以上情况
                // 如果结果中包含3、2星则排除
                var shouldExclude = false;
                var has6 = combTags.Contains("高级资深干员");
                for (var i = chars.Count - 1; i >= 0; i--)
                {
                    var item = chars[i];
                    if (item.Rarity == 1 || item.Rarity == 2)
                    {
                        // 排除低星
                        shouldExclude = true;
                        break;
                    }

                    if (!has6 && item.Rarity == 5)
                        chars.RemoveAt(i); // 没有高资tag，移除6星
                }

                if (shouldExclude || chars.Count == 0)
                    continue;

                // 干员按星级排序
                chars.Sort((a, b) => b.Rarity - a.Rarity);
                combTags.Sort((a, b) => b.Length - a.Length);
                // 返回tag组合和对应查询结果
                results.Add(new RecruitResult
                {
                    CombineTags = combTags,
                    Characters = chars
                });
            }
        }

        // 排序
        results.Sort((a, b) =>
        {
            var na = 0;
            var nb = 0;
            if (a.CombineTags.Contains("高级资深干员"))
                na++;

            if (b.CombineTags.Contains("高级资深干员"))
                nb++;

            if (na != nb)
                return nb - na;

            if (a.CombineTags.Contains("资深干员"))
                na++;

            if (b.CombineTags.Contains("资深干员"))
                nb++;

            if (na != nb)
                return nb - na;

            na = 0;
            foreach (var it in a.Characters)
            {
                if (it.Rarity > na)
                    na = it.Rarity;
            }

            nb = 0;
            foreach (var it in b.Characters)
            {
                if (it.Rarity > nb)
                    nb = it.Rarity;
            }

            if (na != nb)
                return nb - na;

            return b.CombineTags.Count - a.CombineTags.Count;
        });

        return results;
    }

    public string BeautifyRecruitResults(List<RecruitResult> results)
    {
        if (results.Count == 0)
        {
            return Sentence.GetOne("recruitNoResult")!;
        }

        var text = "找到以下包含稀有干员的标签组合：";
        foreach (var item in results)
        {
            text += "\n------------------------\n";
            foreach (var tag in item.CombineTags)
            {
                text += tag + "   ";
            }

            text += "\n";
            foreach (var it in item.Characters)
            {
                text += GetStarText(it.Rarity) + it.Name + "  ";
            }
        }

        return text;
    }

    public string GetStarText(int rarity)
    {
        return rarity switch
        {
            5 => "[6★]",
            4 => "[5★]",
            3 => "[4☆]",
            _ => "[☆]"
        };
    }
}

public class RecruitResult
{
    public List<string> CombineTags;
    public List<Character> Characters;
}