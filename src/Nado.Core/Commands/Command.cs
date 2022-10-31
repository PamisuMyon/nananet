using Nado.Core.Models;

namespace Nado.Core.Commands;

public abstract class Command
{
    public abstract string Name { get; }

    public virtual Task Init(NadoBot bot)
    {
        return Task.CompletedTask;
    }
    
    public abstract Task<CommandTestInfo> Test(Message input);

    public abstract Task<CommandResult> Execute(NadoBot bot, Message input, CommandTestInfo testInfo);

    public delegate Task<CommandTestInfo?> CommandPickFunc(IEnumerable<Command> commands, Message input);

    public static async Task<CommandTestInfo?> PickO1(IEnumerable<Command> commands, Message input)
    {
        var index = 0;
        foreach (var command in commands)
        {
            var t = await command.Test(input);
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (t.Confidence == 1f)
            {
                t.CommandIndex = index;
                return t;
            }
            index++;
        }
        return default;
    }
    
    public static readonly CommandTestInfo NoConfidence = new CommandTestInfo
    {
        Confidence = 0,
    };
    
    public static readonly CommandTestInfo HalfConfidence = new CommandTestInfo
    {
        Confidence = .5f,
    };
    
    public static readonly CommandTestInfo FullConfidence = new CommandTestInfo
    {
        Confidence = 1,
    };
    
    public static CommandResult Executed = new CommandResult
    {
        Success = true,
    };

    public static CommandResult Failed = new CommandResult
    {
        Success = false,
    };
    
}
