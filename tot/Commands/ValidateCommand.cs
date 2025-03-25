using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using tot_lib;
using tot.Services;

namespace Tot.Commands;

public class ValidateCommand : ITotCommandInvoked, ITotCommandOptions, ITotCommand
{
    public string Command => "validate";
    public string Description => "Validate git repositories for the cooking process";

    public string ModName { get; set; } = string.Empty;

    public IEnumerable<Option> GetOptions()
    {
        yield return Utils.GetModNameOption(x => ModName = x);
    }
    
    public async Task<int> InvokeAsync(IServiceProvider provider, CancellationToken token)
    {
        var kFiles = provider.GetRequiredService<KitchenFiles>();
        var console = provider.GetRequiredService<IColoredConsole>();
        var git = provider.GetRequiredService<GitHandler>();
        
        try
        {
            kFiles.SetModName(ModName);

            if (git.IsGitRepoDirty(kFiles.ModSharedFolder))
                throw new CommandException(CommandCode.RepositoryIsDirty, "ModsShared repo is dirty");
            if (git.IsGitRepoDirty(kFiles.ModFolder))
                throw new CommandException(CommandCode.RepositoryIsDirty, $"Mod {kFiles.ModName} repo is dirty");
            return 0;
        }
        catch (CommandException ex)
        {
            return await console.OutputCommandError(ex);
        }
    }
}