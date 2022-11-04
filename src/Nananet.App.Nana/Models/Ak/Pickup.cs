using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace Nananet.App.Nana.Models.Ak;

// Deserialize from BsonDocument
public class Pickup
{
    public string Name { get; set; }
    public string Type { get; set; }
    public string[]? Extra { get; set; }
    public string[]? Ignore { get; set; }
    public bool Is6UpOnly { get; set; }
    public bool Is5UpOnly { get; set; }
    public bool Is4UpOnly { get; set; }
    
    [BsonElement("4")] [JsonProperty("4")]
    public string[] Four { get; set; }
    [BsonElement("5")] [JsonProperty("5")]
    public string[] Five { get; set; }
    [BsonElement("6")] [JsonProperty("6")]
    public string[] Six { get; set; }
}
