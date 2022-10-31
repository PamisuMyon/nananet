using Nado.Core.Models;

namespace Nado.App.Nana.Models;

public class MongoAppSettings : AppSettings
{
    public string MongoDbUri { get; set; }
    
    public string MongoDbName { get; set; }
}