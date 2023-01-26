using MongoDB.Entities;
using Nananet.App.Nana.Storage;

namespace Nananet.App.Nana.Models.Ak;

public class Cost
{
    public string Id { get; set; }
    public int Count { get; set; }
    public string Type { get; set; }
}

public class ExtraOutcomeGroup
{
    public int Weight { get; set; }
    public string ItemId { get; set; }
    public int ItemCount { get; set; }
}

public class RequireRoom
{
    public string RoomId { get; set; }
    public int RoomLevel { get; set; }
    public int RoomCount { get; set; }
}

public class RequireStage
{
    public string StageId { get; set; }
    public int Rank { get; set; }
}

[Collection("workshop-formulas")]
public class WorkshopFormula : Entity
{
    public string FormulaId { get; set; }
    public int SortId { get; set; }
    public int ApCost { get; set; }
    public string BuffType { get; set; }
    public List<Cost>? Costs { get; set; }
    public int Count { get; set; }
    public List<ExtraOutcomeGroup> ExtraOutcomeGroup { get; set; }
    public double ExtraOutcomeRate { get; set; }
    public string FormulaType { get; set; }
    public int GoldCost { get; set; }
    public string ItemId { get; set; }
    public int Rarity { get; set; }
    public List<RequireRoom> RequireRooms { get; set; }
    public List<RequireStage> RequireStages { get; set; }
    
    public static Task<WorkshopFormula?> FindOneById(string formulaId)
    {
        return DbUtil.FindOneByField<WorkshopFormula>("formulaId", formulaId);
    }
    
}

