using Nado.Core.Models;

namespace Nado.Core.Commands;

public abstract class Command
{
    public abstract string Name { get; }

    public virtual Task Init(NadoBot bot)
    {
        return Task.CompletedTask;
    }
    
    public abstract Task<CommandTestInfo> Test(Message input, CommandTestOptions options);

    public abstract Task<CommandResult> Execute(NadoBot bot, Message input, CommandTestInfo testInfo);

    public delegate Task<CommandTestInfo?> CommandPickFunc(IEnumerable<Command> commands, Message input, CommandTestOptions options);

    public static async Task<CommandTestInfo?> PickO1(IEnumerable<Command> commands, Message input, CommandTestOptions options)
    {
        var index = 0;
        foreach (var command in commands)
        {
            var t = await command.Test(input, options);
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
    
    public static readonly CommandTestInfo NoConfidence = new()
    {
        Confidence = 0,
    };
    
    public static readonly CommandTestInfo HalfConfidence = new()
    {
        Confidence = .5f,
    };
    
    public static readonly CommandTestInfo FullConfidence = new()
    {
        Confidence = 1,
    };
    
    public static CommandResult Executed = new()
    {
        Success = true,
    };

    public static CommandResult Failed = new()
    {
        Success = false,
    };
    
}
