using System.CommandLine;
using tot_lib;
using tot.Services;

namespace Tot.Commands;

public class ValidateCommand : ModBasedCommand<ValidateCommandOptions, ValidateCommandHandler>
{
    public ValidateCommand() : base("validate", "Validate git repositories for the cooking process")
    {
    }
}

public class ValidateCommandOptions : ModBasedCommandOptions
{
}

public class ValidateCommandHandler(IConsole console, GitHandler git, KitchenFiles kitchenFiles)
    : ModBasedCommandHandler<ValidateCommandOptions>(kitchenFiles)
{
    private readonly KitchenFiles _kitchenFiles = kitchenFiles;

    public override async Task<int> HandleAsync(ValidateCommandOptions options, CancellationToken cancellationToken)
    {
        await base.HandleAsync(options, cancellationToken);

        try
        {
            if (git.IsGitRepoDirty(_kitchenFiles.ModSharedFolder))
                throw new CommandException(CommandCode.RepositoryIsDirty, "ModsShared repo is dirty");
            if (git.IsGitRepoDirty(_kitchenFiles.ModFolder))
                throw new CommandException(CommandCode.RepositoryIsDirty, $"Mod {_kitchenFiles.ModName} repo is dirty");
            return 0;
        }
        catch (CommandException ex)
        {
            return await console.OutputCommandError(ex);
        }
    }
}