using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace Nado.App.Nana.Storage;

public static class DbUtil
{
    private static MongoClient s_client;
    private static IMongoDatabase s_db;
    public static IMongoDatabase Db => s_db;
    
    public static void Connect(string uri)
    {
        var settings = MongoClientSettings.FromConnectionString(uri);
        // settings.LinqProvider = LinqProvider.V3;
        
        s_client = new MongoClient(settings);
        s_db = s_client.GetDatabase("nana");
        
        var pack = new ConventionPack { new CamelCaseElementNameConvention() };
        ConventionRegistry.Register("camel case", pack, t => true);
    }
    
}

public class Collection<TDocument>
{

    protected string _collectionName;
    
    protected IMongoCollection<TDocument>? _col;
    public IMongoCollection<TDocument> Col
    {
        get
        {
            if (_col == null)
                _col = DbUtil.Db.GetCollection<TDocument>(_collectionName);
            return _col;
        }
    }

    public Collection(string collectionName)
    {
        _collectionName = collectionName;
        DbUtil.Db.GetCollection<TDocument>(_collectionName);
    }

}