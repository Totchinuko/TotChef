using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;

namespace tot_lib;

public interface ITotCommand
{
    string Command { get; }
    string Description { get; }
}

public interface ITotCommandInvoked
{
    Task<int> InvokeAsync(IServiceProvider provider, CancellationToken token);
}

public interface ITotCommandOptions
{
    IEnumerable<Option> GetOptions();
}

public interface ITotCommandArguments
{
    IEnumerable<Argument> GetArguments();
}

public interface ITotCommandSubCommands
{
    IEnumerable<ITotCommand> GetSubCommands();
}