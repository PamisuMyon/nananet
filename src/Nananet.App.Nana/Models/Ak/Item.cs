using MongoDB.Entities;
using Nananet.App.Nana.Storage;

namespace Nananet.App.Nana.Models.Ak;

public class BuildingProductList
{
    public string RoomType { get; set; }
    public string FormulaId { get; set; }
}

public class StageDropList
{
    public string StageId { get; set; }
    public string OccPer { get; set; }
}

[Collection("items")]
public class Item : Entity
{
    public string ItemId { get; set; }
    public List<BuildingProductList>? BuildingProductList { get; set; }
    public string ClassifyType { get; set; }
    public string DataSource { get; set; }
    public string Description { get; set; }
    public string IconId { get; set; }
    public string ItemType { get; set; }
    public string Name { get; set; }
    public string ObtainApproach { get; set; }
    public string OverrideBkg { get; set; }
    public int Rarity { get; set; }
    public int SortId { get; set; }
    public string? StackIconId { get; set; }
    // public List<StageDropList>? StageDropList { get; set; }
    public string Usage { get; set; }
    
    public static Task<Item?> FindOneById(string itemId)
    {
        return DbUtil.FindOneByField<Item>("itemId", itemId);
    }
    
    public static Task<Item?> FindOneByName(string name, bool fuzzy)
    {
        return DbUtil.FindOneByField<Item>("name", name, fuzzy, true);
    }
    
}
