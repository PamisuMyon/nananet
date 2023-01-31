using MongoDB.Entities;
using Nananet.App.Nana.Storage;

namespace Nananet.App.Nana.Models.Ak;

public class OverrideKillCntInfos
{
}

[Collection("enemies")]
public class Enemy : Entity
{
    public string EnemyId { get; set; }
    public string Ability { get; set; }
    public string Attack { get; set; }
    public string AttackType { get; set; }
    public string Defence { get; set; }
    public string Description { get; set; }
    public string Endure { get; set; }
    public string EnemyIndex { get; set; }
    public string EnemyLevel { get; set; }
    public string EnemyRace { get; set; }
    public string[] EnemyTags { get; set; }
    public bool HideInHandbook { get; set; }
    public bool IsInvalidKilled { get; set; }
    public string Name { get; set; }
    // public OverrideKillCntInfos OverrideKillCntInfos { get; set; }
    public string Resistance { get; set; }
    public int SortId { get; set; }
    
    public static Task<Enemy?> FindOneByName(string name, bool fuzzy)
    {
        return DbUtil.FindOneByField<Enemy>("name", name, fuzzy, true);
    }
    
}

