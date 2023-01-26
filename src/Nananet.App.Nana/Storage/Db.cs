using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using MongoDB.Entities;
using Nananet.Core.Utils;

namespace Nananet.App.Nana.Storage;

public static class DbUtil
{
    public static async Task Connect(string uri, string dbName)
    {
        await DB.InitAsync(dbName, MongoClientSettings.FromConnectionString(uri));
        Logger.L.Info($"Database connected: {dbName}");
        
        var pack = new ConventionPack { new CamelCaseElementNameConvention() };
        ConventionRegistry.Register("camel case", pack, t => true);
    }
    
    public static async Task<T?> FindOneByField<T>(string field, string value, bool fuzzy = false, bool lengthLimitWhenFuzzy = false) where T : IEntity
    {
        if (fuzzy && lengthLimitWhenFuzzy)
        {
            var length = new Regex("[a-zA-Z]").IsMatch(value) ? 3 : 2;
            if (value.Length < length) return default;
        }
        
        var query = new BsonDocument
            {
                { field, fuzzy? new BsonDocument{
                    { "$regex", value },
                    { "$options", "i"}
                } : value }
            };
        var find = DB.Find<T?>()
            .Match(query);
        if (fuzzy)
        {
            var execute = await find.ExecuteAsync();
            if (execute != null && execute.Count > 0)
                return execute.RandomElem();
            return default;
        }
        return await find.ExecuteFirstAsync();
    }
    
}
