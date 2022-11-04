using Nananet.Core.Commands;
using Nananet.Core.Storage;

namespace Nananet.Core.Models;

public class InitOptions
{
    public IStorage? Storage { get; set; }
 
    public List<Command> Commands { get; set; } 
}