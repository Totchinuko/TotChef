using System.CommandLine;
using tot_lib;
using tot.Services;

namespace Tot.Commands;

public class ConflictCommand : Command<ConfigCommandOptions, ConfigCommandHandler>
{
    public ConflictCommand() : base("conflict", "Process a mod list to highlight common files")
    {
        var arg = new Argument<string>("path", "Path to the mod list");
        AddArgument(arg);
    }
}

public class ConflictCommandOptions : ICommandOptions
{
    public string Path { get; set; } = string.Empty;
}

internal class ConflictCommandHandler(IConsole console, KitchenClerk clerk)
    : ICommandOptionsHandler<ConflictCommandOptions>
{
    public async Task<int> HandleAsync(ConflictCommandOptions options, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(options.Path))
            return await console.OutputCommandError(CommandCode.MissingArg(nameof(options.Path)));

        var modlistFile = new FileInfo(options.Path).GetProperCasedFileInfo();
        var modlist = (await File.ReadAllLinesAsync(modlistFile.FullName, cancellationToken)).ToList();

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