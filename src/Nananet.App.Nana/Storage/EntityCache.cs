using MongoDB.Entities;

namespace Nananet.App.Nana.Storage;

public class EntityCache<T> where T : Entity
{
    protected List<T> _cache;
    public List<T> Value => _cache;
    
    public async Task Refresh()
    {
        _cache = await DB.Find<T>().ExecuteAsync();
    }
    
}