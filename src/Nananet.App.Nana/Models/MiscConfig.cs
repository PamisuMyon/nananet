using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Entities;
using Nananet.Core.Utils;
using Newtonsoft.Json.Linq;

namespace Nananet.App.Nana.Models;


[Collection("configs")]
public class MiscConfig : Entity
{
    public string Name { get; set; }

    public BsonDocument Value { get; set; }

    public static async Task<T?> FindByName<T>(string name)
    {
        var doc = await DB.Find<MiscConfig>()
            .Match(d => d.Name == name)
            .ExecuteFirstAsync();
        return doc != default ? BsonSerializer.Deserialize<T>(doc.Value) : default;
    }

    public static async Task Save(string name, object value)
    {
        var doc = new MiscConfig
        {
            Name = name,
            Value = BsonDocument.Create(value)
        };
        await doc.SaveAsync();
    }

    public static async Task Upsert(string name, object value)
    {
        await DB.Update<MiscConfig>()
            .Match(d => d.Name == name)
            .Modify(d => d.Value, BsonDocument.Parse(JsonUtil.ToJson(value)))
            .Option(options => options.IsUpsert = true)
            .ExecuteAsync();
    }
    
}
