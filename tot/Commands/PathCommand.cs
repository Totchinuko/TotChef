using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using tot_lib;
using tot_lib.CommandLine;
using tot.Services;

namespace Tot.Commands;

public class PathCommand : ICommand<PathCommand>
{
    public static Command Command = CommandBuilder
        .Create<PathCommand>("path", "Return a path to be used with cd")
        .SubCommands.Add(PathModCommand.Command)
        .SubCommands.Add(PathPakCommand.Command)
        .SubCommands.Add(PathSharedCommand.Command)
        .BuildCommand();

}

