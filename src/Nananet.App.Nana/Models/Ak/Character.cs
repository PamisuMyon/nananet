using MongoDB.Entities;
using Newtonsoft.Json.Linq;

namespace Nananet.App.Nana.Models.Ak;

public class AllSkillLvlup
{
    public UnlockCond UnlockCond { get; set; }
    public List<LvlUpCost> LvlUpCost { get; set; }
}

public class AttributeModifier
{
    public int AttributeType { get; set; }
    public int FormulaItem { get; set; }
    public int Value { get; set; }
    public bool LoadFromBlackboard { get; set; }
    public bool FetchBaseValueFromSourceEntity { get; set; }
}

public class Attributes
{
    public object AbnormalFlags { get; set; }
    public object AbnormalImmunes { get; set; }
    public object AbnormalAntis { get; set; }
    public object AbnormalCombos { get; set; }
    public object AbnormalComboImmunes { get; set; }
    public List<AttributeModifier> AttributeModifiers { get; set; }
}

public class AttributesKeyFrame
{
    public int Level { get; set; }
    public Data Data { get; set; }
}

public class Blackboard
{
    public string Key { get; set; }
    public double Value { get; set; }
}

public class Buff
{
    public Attributes Attributes { get; set; }
}

public class Candidate
{
    public UnlockCondition UnlockCondition { get; set; }
    public int RequiredPotentialRank { get; set; }
    public string PrefabKey { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public object RangeId { get; set; }
    public List<Blackboard> Blackboard { get; set; }
    public string OverrideDescripton { get; set; }
}

public class Data
{
    public int MaxHp { get; set; }
    public int Atk { get; set; }
    public int Def { get; set; }
    public int MagicResistance { get; set; }
    public int Cost { get; set; }
    public int BlockCnt { get; set; }
    public int MoveSpeed { get; set; }
    public int AttackSpeed { get; set; }
    public int BaseAttackTime { get; set; }
    public int RespawnTime { get; set; }
    public int HpRecoveryPerSec { get; set; }
    public int SpRecoveryPerSec { get; set; }
    public int MaxDeployCount { get; set; }
    public int MaxDeckStackCnt { get; set; }
    public int TauntLevel { get; set; }
    public int MassLevel { get; set; }
    public int BaseForceLevel { get; set; }
    public bool StunImmune { get; set; }
    public bool SilenceImmune { get; set; }
    public bool SleepImmune { get; set; }
    public bool FrozenImmune { get; set; }
    public bool LevitateImmune { get; set; }
}

public class EvolveCost
{
    public string Id { get; set; }
    public int Count { get; set; }
    public string Type { get; set; }
}

public class FavorKeyFrame
{
    public int Level { get; set; }
    public Data Data { get; set; }
}

public class LevelUpCost
{
    public string Id { get; set; }
    public int Count { get; set; }
    public string Type { get; set; }
}

public class LevelUpCostCond
{
    public UnlockCond UnlockCond { get; set; }
    public int LvlUpTime { get; set; }
    public List<LevelUpCost> LevelUpCost { get; set; }
}

public class LvlUpCost
{
    public string Id { get; set; }
    public int Count { get; set; }
    public string Type { get; set; }
}

public class Phase
{
    public string CharacterPrefabKey { get; set; }
    public string RangeId { get; set; }
    public int MaxLevel { get; set; }
    public List<AttributesKeyFrame> AttributesKeyFrames { get; set; }
    public List<EvolveCost> EvolveCost { get; set; }
}

public class PotentialRank
{
    public int Type { get; set; }
    public string Description { get; set; }
    public Buff Buff { get; set; }
    public object EquivalentCost { get; set; }
}

public class Skill
{
    public string SkillId { get; set; }
    public object OverridePrefabKey { get; set; }
    public object OverrideTokenKey { get; set; }
    public List<LevelUpCostCond> LevelUpCostCond { get; set; }
    public UnlockCond UnlockCond { get; set; }
}

public class Talent
{
    public List<Candidate> Candidates { get; set; }
}

public class Trait
{
    public List<Candidate> Candidates { get; set; }
}

public class UnlockCond
{
    public int Phase { get; set; }
    public int Level { get; set; }
}

public class UnlockCondition
{
    public int Phase { get; set; }
    public int Level { get; set; }
}

[Collection("characters")]
public class Character : Entity
{
    // public string CharID { get; set; }
    // public List<AllSkillLvlup> AllSkillLvlup { get; set; }
    // public string Appellation { get; set; }
    // public bool CanUseGeneralPotentialItem { get; set; }
    // public string Description { get; set; }
    // public string DisplayNumber { get; set; }
    // public List<FavorKeyFrame> FavorKeyFrames { get; set; }
    // public object GroupId { get; set; }
    // public bool IsNotObtainable { get; set; }
    // public bool IsSpChar { get; set; }
    // public string ItemDesc { get; set; }
    // public string ItemObtainApproach { get; set; }
    // public string ItemUsage { get; set; }
    // public int MaxPotentialLevel { get; set; }
    public string Name { get; set; }
    // public string NationId { get; set; }
    // public List<Phase> Phases { get; set; }
    // public string Position { get; set; }
    // public string PotentialItemId { get; set; }
    // public List<PotentialRank> PotentialRanks { get; set; }
    public string Profession { get; set; }
    public int Rarity { get; set; }
    // public List<Skill> Skills { get; set; }
    // public string SubProfessionId { get; set; }
    // public List<string> TagList { get; set; }
    // public List<Talent> Talents { get; set; }
    // public object TeamId { get; set; }
    // public string TokenKey { get; set; }
    // public Trait Trait { get; set; }


    public static Task<List<Character>> FindByRecruit(int? rarity, string? position, string? profession, List<string>? tags)
    {
        var query = new JObject();
        query["canRecruit"] = true;
        if (rarity != null)
            query["rarity"] = rarity.Value;
        if (position != null)
            query["position"] = position;
        if (profession != null)
            query["profession"] = profession;
        if (tags != null)
            query["tagList"] = new JObject
            {
                ["$all"] = JArray.FromObject(tags) 
            };
        return DB.Find<Character>().MatchString(query.ToString()).ExecuteAsync();
    }
    
}
