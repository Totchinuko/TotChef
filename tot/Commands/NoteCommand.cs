using tot_lib;

namespace Tot.Commands;

public class NoteCommand : ITotCommand, ITotCommandSubCommands
{
    public string Command => "note";
    public string Description => "Manage patch notes";
    public IEnumerable<ITotCommand> GetSubCommands()
    {
        yield return new NoteAddCommand();
        yield return new NoteDisplayCommand();
        yield return new NoteClearCommand();
    }
}