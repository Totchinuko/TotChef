using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using tot_lib;
using tot.Services;

namespace Tot.Commands;

public class SwapCommand : ModBasedCommand, ITotCommand
{
    public string Command => "swap";
    public string Description => "Swap files in the cookinfo.ini";
    
    public bool Exclude { get; set; }
    public string SearchPattern { get; set; } = string.Empty;
    public bool Recursive { get; set; }

    public override IEnumerable<Option> GetOptions()
    {
        foreach (var option in base.GetOptions())
            yield return option;
        
        var optb = new TotOption<bool>("--exclude", "Swap files to the exclude list");
        optb.AddAlias("-e");
        optb.AddSetter(x => Exclude = x);
        yield return optb;
        optb = new TotOption<bool>("--recursive", "Include files from subfolder");
        optb.AddAlias("-r");
        optb.AddSetter(x => Recursive = x);
        yield return optb;
        var opts = new TotOption<string>("--search-pattern", "Folder filtering, accept * wildcards on file name");
        opts.AddAlias("-s");
        opts.AddSetter(x => SearchPattern = x ?? string.Empty);
        yield return opts;
    }

    public override async Task<int> InvokeAsync(IServiceProvider provider, CancellationToken token)
    {
        await base.InvokeAsync(provider, token);
        var kFiles = provider.GetRequiredService<KitchenFiles>();
        var clerk = provider.GetRequiredService<KitchenClerk>();
        var console = provider.GetRequiredService<IColoredConsole>();
        
        try
        {
            var filter = SearchPattern;
            if (!string.IsNullOrEmpty(filter) &&
                !filter.PosixFullName().StartsWith(kFiles.DevKitContent.PosixFullName()))
                filter = Path.Join(kFiles.DevKitContent.FullName, filter);

            var cookInfo = await clerk.GetCookInfo();
            List<string> added;

            if (File.Exists(filter))
            {
                if (Exclude)
                    added = clerk.SwapFilesInLists([filter.PosixFullName()], cookInfo.Excluded, cookInfo.Included);
                else
                    added = clerk.SwapFilesInLists([filter.PosixFullName()], cookInfo.Included, cookInfo.Excluded);
            }
            else if (Directory.Exists(filter))
            {
                var directory = new DirectoryInfo(filter).GetProperCasedDirectoryInfo();
                var files = Directory.GetFiles(directory.FullName, $"*{Constants.UAssetExt}",
                    Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList();

                if (Exclude)
                    added = clerk.SwapFilesInLists(files, cookInfo.Excluded, cookInfo.Included);
                else
                    added = clerk.SwapFilesInLists(files, cookInfo.Included, cookInfo.Excluded);
            }
            else if (filter.Contains("*"))
            {
                var fileInfo = new FileInfo(filter);
                if (fileInfo.Directory == null || !fileInfo.Directory.Exists)
                    throw CommandCode.NotFound(fileInfo);
                var files = Directory.GetFiles(fileInfo.Directory.FullName,
                    filter.Substring(fileInfo.Directory.FullName.Length + 1),
                    Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList();

                if (Exclude)
                    added = clerk.SwapFilesInLists(files, cookInfo.Excluded, cookInfo.Included);
                else
                    added = clerk.SwapFilesInLists(files, cookInfo.Included, cookInfo.Excluded);
            }
            else
            {
                throw new CommandException($"Invalid filter {filter}");
            }

            await clerk.SetCookInfo(cookInfo);

            foreach (var addedFile in added)
                console.WriteLine(Exclude ? "-" : "+" + addedFile);
        }
        catch (CommandException ex)
        {
            return await console.OutputCommandError(ex);
        }

        return 0;
    }
}
