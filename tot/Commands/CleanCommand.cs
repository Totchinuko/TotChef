using System.CommandLine;
using tot_lib;
using tot.Services;

namespace Tot.Commands;

public class CleanCommand : Command<ModBasedCommandOptions, CleanCommandHandler>
{
    public CleanCommand() : base("clean", "Clean any missing file from the cookinfo.ini")
    {
    }
}

public class CleanCommandHandler(IConsole console, KitchenClerk clerk, KitchenFiles kitchenFiles)
    : ModBasedCommandHandler<ModBasedCommandOptions>(kitchenFiles)
{
    public IConsole Console { get; } = console;
    public KitchenClerk Clerk { get; } = clerk;
    public KitchenFiles KitchenFiles { get; } = kitchenFiles;

    public override async Task<int> HandleAsync(ModBasedCommandOptions options, CancellationToken token)
    {
        await base.HandleAsync(options, token);

        try
        {
            var cookInfos = await Clerk.GetCookInfo();
            var changes = Clerk.RemoveMissingFiles(cookInfos);
            await Clerk.SetCookInfo(cookInfos);
            foreach (var file in changes)
                Console.WriteLine(file);
            Console.WriteLine($"{changes.Count} missing file(s) removed from {KitchenFiles.ModName} cookinfo.ini");
        }
        catch (CommandException ex)
        {
            return await Console.OutputCommandError(ex);
        }

        return 0;
    }
}