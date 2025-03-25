using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using tot_lib;
using tot.Services;

namespace Tot.Commands;

public class PathPakCommand : ITotCommand, ITotCommandInvoked, ITotCommandArguments
{
    public string Command => "pak";
    public string Description => "Print out the path of a mod pak file";

    public string ModName { get; set; } = string.Empty;
    public async Task<int> InvokeAsync(IServiceProvider provider, CancellationToken token)
    {
        var kFiles = provider.GetRequiredService<KitchenFiles>();
        var console = provider.GetRequiredService<IColoredConsole>();
        try
        {
            kFiles.SetModName(ModName);
            console.Write(kFiles.ModPakFile.PosixFullName());
            return 0;
        }
        catch (CommandException ex)
        {
            return await console.OutputCommandError(ex);
        }
    }

    public IEnumerable<Argument> GetArguments()
    {
        var arg = new TotArgument<string>("mod-name");
        arg.AddSetter(x => ModName = x ?? string.Empty);
        arg.SetDefaultValue(string.Empty);
        yield return arg;
    }
}