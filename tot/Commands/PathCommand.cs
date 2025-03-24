using System.CommandLine;
using tot_lib;
using tot.Services;

namespace Tot.Commands;

public class PathCommand : ModBasedCommand<PathCommandOptions, PathCommandHandler>
{
    public PathCommand() : base("path", "Return a path to be used with cd")
    {
        var opt = new Option<bool>("--mods-shared", "Return the path to the shared folder");
        opt.AddAlias("-s");
        AddOption(opt);
        opt = new Option<bool>("--pak-file", "Return the path to the cooked pak file");
        opt.AddAlias("-p");
        AddOption(opt);
    }
}

public class PathCommandOptions : ModBasedCommandOptions
{
    public bool ModsShared { get; set; }
    public bool PakFile { get; set; }
}

public class PathCommandHandler(IConsole console, KitchenFiles kitchenFiles)
    : ModBasedCommandHandler<PathCommandOptions>(kitchenFiles)
{
    private readonly KitchenFiles _kitchenFiles = kitchenFiles;

    public override async Task<int> HandleAsync(PathCommandOptions options, CancellationToken cancellationToken)
    {
        await base.HandleAsync(options, cancellationToken);

        try
        {
            var path = "";
            if (!string.IsNullOrEmpty(_kitchenFiles.ModName))
            {
                if (!_kitchenFiles.IsModPathValid())
                    throw CommandCode.NotFound(_kitchenFiles.ModFolder);
                path = _kitchenFiles.ModFolder.PosixFullName();
            }
            else
            {
                if (options.ModsShared)
                    path = _kitchenFiles.ModsShared.PosixFullName();
                else if (options.PakFile)
                    path = _kitchenFiles.ModPakFile.PosixFullName();
                else
                    throw CommandCode.MissingArg(nameof(options.ConanMod));
            }

            console.Write(path);
        }
        catch (CommandException ex)
        {
            return await console.OutputCommandError(ex);
        }

        return 0;
    }
}