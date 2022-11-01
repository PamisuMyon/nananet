using MongoDB.Entities;

namespace Nado.App.Nana.Models;

[Collection("conversations")]
public class Conversation : Entity
{
    public string Type { get; set; }
    public string Condition { get; set; }
    public string Priority { get; set; }
    public string[] Q { get; set; }
    public string[] A { get; set; }
    
    public static readonly string TypeSentence = "sentence";
    
    public static readonly string TypeRegex = "regex";
    
    protected static EntityCache<Conversation> _cache = new ();
    
    public static EntityCache<Conversation> Cache => _cache;
    
}