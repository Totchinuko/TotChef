using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using tot_lib;
using tot_lib.CommandLine;
using tot.Services;

namespace Tot.Commands;

public class ValidateCommand(KitchenFiles files, IColoredConsole console, GitHandler git) : IInvokableCommand<ValidateCommand>
{
    public static Command Command = CommandBuilder
        .CreateInvokable<ValidateCommand>("validate", "Validate git repositories for the cooking process")
        .SetServiceConfiguration(Program.ConfigureServices)
        .Options.AddModName((c,v) => c.ModName = v)
        .BuildCommand();
    public string ModName { get; set; } = string.Empty;

    public async Task<int> InvokeAsync(CancellationToken token)
    {
        try
        {
            files.SetModName(ModName);

            if (await git.IsGitRepoInvalidOrDirty(files.ModSharedFolder))
                throw new CommandException(CommandCode.RepositoryIsDirty, "ModsShared repo is dirty");
            if (await git.IsGitRepoInvalidOrDirty(files.ModFolder))
                throw new CommandException(CommandCode.RepositoryIsDirty, $"Mod {files.ModName} repo is dirty");
            return 0;
        }
        catch (CommandException ex)
        {
            return await console.OutputCommandError(ex);
        }
    }
}