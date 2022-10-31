using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using MongoDB.Entities;
using Nado.Core.Utils;

namespace Nado.App.Nana.Storage;

public static class DbUtil
{
    public static async Task Connect(string uri, string dbName)
    {
        await DB.InitAsync(dbName, MongoClientSettings.FromConnectionString(uri));
        Logger.L.Info($"Database connected: {dbName}");
        
        var pack = new ConventionPack { new CamelCaseElementNameConvention() };
        ConventionRegistry.Register("camel case", pack, t => true);
    }
    
}
