using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Entities;
using Nananet.App.Nana.Storage;
using Newtonsoft.Json;

namespace Nananet.App.Nana.Models.Ak;

[Collection("roguelike-items")]
public class RoguelikeItem : Entity
{
    [BsonElement("id")][JsonProperty("id")]
    public string ItemId { get; set; }
    public string RogueId { get; set; }
    public string Description { get; set; }
    public string IconId { get; set; }
    public string Name { get; set; }
    public string ObtainApproach { get; set; }
    public string Rarity { get; set; }
    public int SortId { get; set; }
    public string SubType { get; set; }
    public string Type { get; set; }
    public string UnlockCondDesc { get; set; }
    public string Usage { get; set; }
    public int Value { get; set; }
    public bool CanSacrifice { get; set; }

    public static Task<RoguelikeItem?> FindOneByName(string name, bool fuzzy)
    {
        return DbUtil.FindOneByField<RoguelikeItem>("name", name, fuzzy, true);
    }
    
}
