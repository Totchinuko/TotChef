using System.CommandLine;
using tot_lib;

namespace Tot.Commands;

public class NoteAddCommand : ITotCommand, ITotCommandSubCommands
{
    public string Command => "add";
    public string Description => "Add a new line to the patch note";

    public IEnumerable<ITotCommand> GetSubCommands()
    {
        yield return new NoteAddAdditionCommand();
        yield return new NoteAddImprovementCommand();
        yield return new NoteAddFixCommand();
    }
}