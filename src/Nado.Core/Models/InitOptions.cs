using Nado.Core.Commands;
using Nado.Core.Storage;

namespace Nado.Core.Models;

public class InitOptions
{
    public IStorage? Storage { get; set; }
 
    public List<Command> Commands { get; set; } 
}