using System.Dynamic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Entities;
using Nado.Core.Utils;

namespace Nado.App.Nana.Models.Ak;

[Collection("ak-misc")]
public class AkMisc : Entity
{
    public string Name { get; set; }

    public object Value { get; set; }
    
    public static async Task<T?> FindByName<T>(string name)
    {
        var doc = await DB.Find<AkMisc>()
            .Match(d => d.Name == name)
            .ExecuteFirstAsync();
        return doc != default ? JsonUtil.FromJson<T>(JsonUtil.ToJson(doc.Value)) : default;
    }
    
    public static async Task<BsonDocument?> FindByName(string name)
    {
        var doc = await DB.Find<AkMisc>()
            .Match(d => d.Name == name)
            .ExecuteFirstAsync();
        return doc != default ? BsonDocument.Create(doc.Value) : default;
    }

}