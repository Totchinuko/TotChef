using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using tot_lib;
using tot_lib.CommandLine;
using tot.Services;

namespace Tot.Commands;

public class SearchCommand(IColoredConsole console, KitchenClerk clerk) : IInvokableCommand<SearchCommand>
{
    public static Command Command = CommandBuilder
        .CreateInvokable<SearchCommand>("search", "Process a mod list to highlight common files")
        .SetServiceConfiguration(Program.ConfigureServices)
        .Arguments.Create<string>("mod-list", "Path to the mod list")
        .AddSetter((c,v) => c.ModList = v ?? string.Empty).BuildArgument()
        .Arguments.Create<string>("search-pattern", "File name pattern to look for")
        .AddSetter((c,v) => c.SearchPattern = v ?? string.Empty).BuildArgument()
        .BuildCommand();
    
    public string ModList { get; set; } = string.Empty;
    public string SearchPattern { get; set; } = string.Empty;
    
    public async Task<int> InvokeAsync(CancellationToken token)
    {
        try
        {
            List<string> modlist;
            if (File.Exists(ModList))
            {
                var modlistFile = new FileInfo(ModList).GetProperCasedFileInfo();
                modlist = (await File.ReadAllLinesAsync(modlistFile.FullName, token)).ToList();
            }
            else if (Directory.Exists(ModList))
            {
                modlist = Directory.GetFiles(ModList, "*.pak", SearchOption.AllDirectories).ToList();
            }
            else
            {
                throw new CommandException("Invalid mod list");
            }

            List<PakListing> listings = [];
            foreach (var line in modlist)
            {
                var path = line;
                if (path.StartsWith("*"))
                    path = path.Substring(1);
                var mod = new FileInfo(path);
                if (mod.Exists)
                {
                    console.WriteLine($"Parsing:{mod.FullName}");
                    var pakList = await clerk.QueryPakFile(mod);
                    listings.Add(new PakListing(pakList, mod.Name));
                }
                else
                {
                    console.WriteLine($"Not Found:{mod.FullName}");
                }
            }

            foreach (var listing in listings)
            foreach (var pakedFile in listing.pakedFiles)
                if (pakedFile.path.Contains(SearchPattern))
                    console.WriteLine(listing.pakName);
        }
        catch (CommandException ex)
        {
            return await console.OutputCommandError(ex);
        }

        return 0;
    }
}