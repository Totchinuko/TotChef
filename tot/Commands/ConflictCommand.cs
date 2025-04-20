using System.CommandLine;
using System.Reflection.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Pastel;
using tot_lib;
using tot_lib.CommandLine;
using tot.Services;

namespace Tot.Commands;

public class ConflictCommand(IConsole console, ILogger<ConflictCommand> logger, KitchenClerk clerk) : IInvokableCommand<ConflictCommand>
{
    public static Command Command = CommandBuilder
        .CreateInvokable<ConflictCommand>("conflict", "Process a mod list to highlight common files")
        .SetServiceConfiguration(Program.ConfigureServices)
        .Arguments.Create<string>("path")
        .AddSetter((c, v) => c.Path = v ?? string.Empty)
        .BuildArgument()
        .BuildCommand();
    
    public string Path { get; set; } = string.Empty;
    
    public async Task<int> InvokeAsync(CancellationToken token)
    {
        if (string.IsNullOrEmpty(Path))
        {
            logger.LogError("Missing argument {arg}", "path");
            return 1;
        }

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
                    logger.LogInformation("Parsing:{file}", mod.FullName);
                    var pakOut = await clerk.QueryPakFile(mod);
                    listings.Add(new PakListing(pakOut, mod.Name));
                }
                else
                {
                    logger.LogWarning("Not Found:{file}", mod.FullName);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Conflict scan failed");
        }

        logger.LogInformation("Report:");
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
                console.WriteLine(keyValuePair.Value[0].path.Pastel(Constants.ColorOrange));
                foreach (var pakedFile in keyValuePair.Value)
                    console.WriteLine($"{pakedFile.sha} - {pakedFile.pakName}" + 
                                      $"({pakedFile.size} bytes)".Pastel(Constants.ColorGrey));
            }
        }

        return 0;
    }
}