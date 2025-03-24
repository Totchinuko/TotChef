using System.CommandLine;
using tot_lib;
using tot.Services;

namespace Tot.Commands;

public class SwapCommand : ModBasedCommand<SwapCommandOptions, SwapCommandHandler>
{
    public SwapCommand() : base("swap", "Swap files in the cookinfo.ini")
    {
        var optb = new Option<bool>("--exclude", "Swap files to the exclude list");
        optb.AddAlias("-e");
        AddOption(optb);
        optb = new Option<bool>("--recursive", "Include files from subfolder");
        optb.AddAlias("-r");
        AddOption(optb);
        var opts = new Option<bool>("--search-pattern", "Folder filtering, accept * wildcards on file name");
        opts.AddAlias("-s");
        AddOption(opts);
    }
}

public class SwapCommandOptions : ModBasedCommandOptions
{
    public bool Exclude { get; set; }
    public string SearchPattern { get; set; } = string.Empty;
    public bool Recursive { get; set; }
}

public class SwapCommandHandler(IConsole console, KitchenFiles kitchenFiles, KitchenClerk clerk)
    : ModBasedCommandHandler<SwapCommandOptions>(kitchenFiles)
{
    private readonly KitchenFiles _kitchenFiles = kitchenFiles;

    public override async Task<int> HandleAsync(SwapCommandOptions options, CancellationToken cancellationToken)
    {
        await base.HandleAsync(options, cancellationToken);

        try
        {
            var filter = options.SearchPattern;
            if (!string.IsNullOrEmpty(filter) &&
                !filter.PosixFullName().StartsWith(_kitchenFiles.DevKitContent.PosixFullName()))
                filter = Path.Join(_kitchenFiles.DevKitContent.FullName, filter);

            var cookInfo = await clerk.GetCookInfo();
            List<string> added;

            if (File.Exists(filter))
            {
                if (options.Exclude)
                    added = clerk.SwapFilesInLists([filter.PosixFullName()], cookInfo.Excluded, cookInfo.Included);
                else
                    added = clerk.SwapFilesInLists([filter.PosixFullName()], cookInfo.Included, cookInfo.Excluded);
            }
            else if (Directory.Exists(filter))
            {
                var directory = new DirectoryInfo(filter).GetProperCasedDirectoryInfo();
                var files = Directory.GetFiles(directory.FullName, $"*{Constants.UAssetExt}",
                    options.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList();

                if (options.Exclude)
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
                    options.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList();

                if (options.Exclude)
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
                console.WriteLine(options.Exclude ? "-" : "+" + addedFile);
        }
        catch (CommandException ex)
        {
            return await console.OutputCommandError(ex);
        }

        return 0;
    }
}