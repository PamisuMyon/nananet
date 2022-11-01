using MongoDB.Entities;

namespace Nado.App.Nana.Models.Ak;

[Collection("gacha-info")]
public class GachaInfo : Entity
{
    public string UserId { get; set; }
    
    public WaterLevel[] WaterLevels { get; set; }
    
    public class WaterLevel
    {
        public string Type { get; set; }
        
        public int Value { get; set; }
    }
    
    public int GetWaterLevel(string type)
    {
        return (from it in WaterLevels 
                where it.Type == type 
                select it.Value)
            .FirstOrDefault();
    }

    public void UpdateWaterLevel(string type, int value)
    {
        foreach (var it in WaterLevels)
        {
            if (it.Type == type)
            {
                it.Value = value;
                return;
            }
        }
    }
    
    public static GachaInfo Create(string id, string type)
    {
        return new GachaInfo
        {
            UserId = id,
            WaterLevels = new WaterLevel[]
            {
                new()
                {
                    Type = type,
                    Value = 0,
                }
            }
        };
    }
    
    public static async Task<GachaInfo?> FindById(string id)
    {
        return await DB.Find<GachaInfo>()
            .Match(g => g.UserId == id)
            .ExecuteFirstAsync();
    }

}