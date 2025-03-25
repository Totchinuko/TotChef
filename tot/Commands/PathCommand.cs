using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using tot_lib;
using tot.Services;

namespace Tot.Commands;

public class PathCommand : ITotCommand, ITotCommandSubCommands
{
    public string Command => "path";
    public string Description => "Return a path to be used with cd";

    public IEnumerable<ITotCommand> GetSubCommands()
    {
        yield return new PathModCommand();
        yield return new PathPakCommand();
        yield return new PathSharedCommand();
    }
}

