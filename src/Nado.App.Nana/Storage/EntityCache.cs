using MongoDB.Entities;

namespace Nado.App.Nana.Models;

public class EntityCache<T> where T : Entity
{
    protected List<T> _cache;
    public List<T> Value => _cache;
    
    public async Task Refresh()
    {
        _cache = await DB.Find<T>().ExecuteAsync();
    }
    
}