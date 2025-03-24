using System.Diagnostics;
using tot.Services;

namespace Tot.Commands;

public class OpenCommand : ModBasedCommand<OpenCommandOption, OpenCommandHandler>
{
    public OpenCommand() : base("open", "Open the folder containing the pak files")
    {
    }
}

public class OpenCommandOption : ModBasedCommandOptions
{
}

public class OpenCommandHandler(KitchenFiles kitchenFiles) : ModBasedCommandHandler<OpenCommandOption>(kitchenFiles)
{
    private readonly KitchenFiles _kitchenFiles = kitchenFiles;

    public override async Task<int> HandleAsync(OpenCommandOption options, CancellationToken cancellationToken)
    {
        await base.HandleAsync(options, cancellationToken);
        Process.Start("explorer.exe", _kitchenFiles.ModPakFolder.FullName);
        return 0;
    }
}