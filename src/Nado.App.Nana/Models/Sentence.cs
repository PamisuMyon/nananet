using MongoDB.Entities;
using Nado.Core.Utils;

namespace Nado.App.Nana.Models;

[Collection("sentences")]
public class Sentence : Entity
{
    public string Name { get; set; }
    
    public string[] Contents { get; set; }

    public static async Task<string[]?> FindByName(string name)
    {
        var doc = await DB.Find<Sentence>()
            .Match(d => d.Name == name)
            .ExecuteFirstAsync();
        return doc?.Contents;
    }
    
    public static async Task<string[]> Get(string name)
    {
        var contents = await FindByName(name);
        if (contents != null)
            return contents;
        return new [] { "" };
    }

    public static async Task<string> GetOne(string name)
    {
        var contents = await FindByName(name);
        if (contents != null)
            return contents.RandomElem();
        return "";
    }
}