using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using tot_lib;
using tot.Services;

namespace Tot;

public class ModBasedCommand : ITotCommandInvoked, ITotCommandOptions
{
    public string ConanMod { get; protected set; } = string.Empty;
    
    public virtual IEnumerable<Option> GetOptions()
    {
        var option = new TotOption<string>("--conan-mod",
            "Specify the mod name you want to perform the action on");
        option.AddAlias("-m");
        option.SetDefaultValue(string.Empty);
        option.AddSetter(v => ConanMod = v ?? string.Empty);
        yield return option;
    }

    public virtual Task<int> InvokeAsync(IServiceProvider provider, CancellationToken token)
    {
        var kitchenFiles = provider.GetRequiredService<KitchenFiles>();
        
        kitchenFiles.SetModName(ConanMod);
        if (!kitchenFiles.IsDevkitPathValid())
            throw new CommandException(CommandCode.DirectoryNotFound,
                "Devkit directory could not be found, make sure the configuration is correct");
        if (!kitchenFiles.IsModPathValid())
            throw CommandCode.NotFound(kitchenFiles.ModFolder);

        return Task.FromResult(0);
    }
}
