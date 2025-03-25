using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using tot_lib;
using tot.Services;

namespace Tot.Commands;

public class ConflictCommand : ITotCommand, ITotCommandArguments, ITotCommandInvoked
{
    public string Path { get; set; } = string.Empty;
    
    public string Command => "conflict";
    public string Description => "Process a mod list to highlight common files";
    
    public IEnumerable<Argument> GetArguments()
    {
        var arg = new TotArgument<string>("path", "Path to the mod list");
        arg.AddSetter(x => Path = x ?? string.Empty);
        yield return arg;
    }
    
    public async Task<int> InvokeAsync(IServiceProvider provider, CancellationToken token)
    {
        var console = provider.GetRequiredService<IColoredConsole>();
        var clerk = provider.GetRequiredService<KitchenClerk>();
        
        if (string.IsNullOrEmpty(Path))
            return await console.OutputCommandError(CommandCode.MissingArg(nameof(Path)));

        var modlistFile = new FileInfo(Path).GetProperCasedFileInfo();
        var modlist = (await File.ReadAllLinesAsync(modlistFile.FullName, token)).ToList();

        List<PakListing> listings = new List<PakListing>();
        try
        {
            foreach (var line in modlist)
            {
                var path = line;
                if (path.StartsWith("*"))
                    path = path.Substring(1);
                var mod = new FileInfo(path);
                if (mod.Exists)
                {
                    console.WriteLine($"Parsing:{mod.FullName}");
                    var pakOut = await clerk.QueryPakFile(mod);
                    listings.Add(new PakListing(pakOut, mod.Name));
                }
                else
                {
                    console.WriteLine($"Not Found:{mod.FullName}");
                }
            }
        }
        catch (CommandException ex)
        {
            return await console.OutputCommandError(ex);
        }

        var folded = new Dictionary<string, List<PakedFile>>();
        foreach (var listing in listings)
        foreach (var pakedFile in listing.pakedFiles)
            if (folded.ContainsKey(pakedFile.path))
                folded[pakedFile.path].Add(pakedFile);
            else folded.Add(pakedFile.path, new List<PakedFile> { pakedFile });

        foreach (var keyValuePair in folded)
        {
            if (!keyValuePair.Key.Contains("/"))
                continue;

            if (keyValuePair.Value.Count > 1 && !keyValuePair.Value.AreShaIdentical())
            {
                Console.WriteLine(keyValuePair.Value[0].path);
                foreach (var pakedFile in keyValuePair.Value)
                    console.WriteLine($"{pakedFile.sha} - {pakedFile.pakName} ({pakedFile.size} bytes)");
            }
        }

        return 0;
    }
}