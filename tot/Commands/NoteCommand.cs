using System.CommandLine;
using tot_lib;
using tot_lib.CommandLine;

namespace Tot.Commands;

public class NoteCommand : ICommand<NoteCommand>
{
    public static Command Command = CommandBuilder
        .Create<NoteCommand>("note", "Manage patch notes")
        .SubCommands.Add(NoteClearCommand.Command)
        .SubCommands.Add(NoteDisplayCommand.Command)
        .BuildCommand();
}