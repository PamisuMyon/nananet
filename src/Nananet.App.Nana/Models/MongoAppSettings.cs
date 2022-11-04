using Nananet.Core.Models;

namespace Nananet.App.Nana.Models;

public class MongoAppSettings : AppSettings
{
    public string MongoDbUri { get; set; }
    
    public string MongoDbName { get; set; }
}