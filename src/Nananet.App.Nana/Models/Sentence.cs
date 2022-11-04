using MongoDB.Entities;
using Nananet.Core.Utils;
using Nananet.App.Nana.Storage;

namespace Nananet.App.Nana.Models;

[Collection("sentences")]
public class Sentence : Entity
{
    public string Name { get; set; }
    
    public string[] Contents { get; set; }


    protected static EntityCache<Sentence> _cache = new ();
    public static EntityCache<Sentence> Cache => _cache;

    public static async Task<string[]?> FindByName(string name)
    {
        var doc = await DB.Find<Sentence>()
            .Match(d => d.Name == name)
            .ExecuteFirstAsync();
        return doc?.Contents;
    }
    
    public static string[] Get(string name)
    {
        foreach (var it in _cache.Value)
        {
            if (it.Name == name)
                return it.Contents;
        }
        return new [] { "" };
    }

    public static string GetOne(string name)
    {
        var contents = Get(name);
        return contents.RandomElem();
    }
}