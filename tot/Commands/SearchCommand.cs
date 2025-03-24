using System.CommandLine;
using tot_lib;
using tot.Services;

namespace Tot.Commands;

public class SearchCommand : Command<SearchCommandOptions, SearchCommandHandler>
{
    public SearchCommand() : base("search", "Process a mod list to highlight common files")
    {
        var opt = new Argument<string>("mod-list", "Path to the mod list");
        AddArgument(opt);
        opt = new Argument<string>("search-pattern", "File name pattern to look for");
        AddArgument(opt);
    }
}

public class SearchCommandOptions : ICommandOptions
{
    public string ModList { get; set; } = string.Empty;
    public string SearchPattern { get; set; } = string.Empty;
}

public class SearchCommandHandler(IConsole console, KitchenClerk clerk) : ICommandOptionsHandler<SearchCommandOptions>
{
    public async Task<int> HandleAsync(SearchCommandOptions options, CancellationToken cancellationToken)
    {
        try
        {
            List<string> modlist;
            if (File.Exists(options.ModList))
            {
                var modlistFile = new FileInfo(options.ModList).GetProperCasedFileInfo();
                modlist = (await File.ReadAllLinesAsync(modlistFile.FullName, cancellationToken)).ToList();
            }
            else if (Directory.Exists(options.ModList))
            {
                modlist = Directory.GetFiles(options.ModList, "*.pak", SearchOption.AllDirectories).ToList();
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
                if (pakedFile.path.Contains(options.SearchPattern))
                    console.WriteLine(listing.pakName);
        }
        catch (CommandException ex)
        {
            return await console.OutputCommandError(ex);
        }

        return 0;
    }
}