using System.CommandLine;
using Microsoft.Extensions.Logging;
using tot_lib;
using tot_lib.CommandLine;
using tot.Services;

namespace Tot.Commands;

public class SwapCommand(KitchenFiles files, KitchenClerk clerk, ILogger<SwapCommand> logger) : IInvokableCommand<SwapCommand>
{
    public static readonly Command Command = CommandBuilder
        .CreateInvokable<SwapCommand>("swap", "Swap files in the cookinfo.ini")
        .SetServiceConfiguration(Program.ConfigureServices)
        .Options.Create<bool>("--exclude", "Swap files to the exclude list").AddAlias("-e")
        .SetSetter((c,v) => c.Exclude = v).BuildOption()
        .Options.Create<bool>("--recursive", "Include files from subfolder").AddAlias("-r")
        .SetSetter((c,v) => c.Recursive = v).BuildOption()
        .Options.Create<string>("--search-pattern", "Folder filtering, accept * wildcards on file name").AddAlias("-s")
        .SetSetter((c,v) => c.SearchPattern = v ?? string.Empty).BuildOption()
        .Options.AddModName((c,v) => c.ModName = v)
        .BuildCommand();
    
    public bool Exclude { get; set; }
    public string SearchPattern { get; set; } = string.Empty;
    public bool Recursive { get; set; }
    public string ModName { get; set; } = string.Empty;

    public async Task<int> InvokeAsync(CancellationToken token)
    {
        try
        {
            files.SetModName(ModName);

            var filter = SearchPattern;
            if (!string.IsNullOrEmpty(filter) &&
                !filter.PosixFullName().StartsWith(files.DevKitContent.PosixFullName()))
                filter = Path.Join(files.DevKitContent.FullName, filter);

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
                var fileList = Directory.GetFiles(directory.FullName, $"*{Constants.UAssetExt}",
                    Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList();

                if (Exclude)
                    added = clerk.SwapFilesInLists(fileList, cookInfo.Excluded, cookInfo.Included);
                else
                    added = clerk.SwapFilesInLists(fileList, cookInfo.Included, cookInfo.Excluded);
            }
            else if (filter.Contains("*"))
            {
                var fileInfo = new FileInfo(filter);
                if (fileInfo.Directory == null || !fileInfo.Directory.Exists)
                    throw new FileNotFoundException($"File not found: {fileInfo.FullName}");
                var fileList = Directory.GetFiles(fileInfo.Directory.FullName,
                    filter.Substring(fileInfo.Directory.FullName.Length + 1),
                    Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList();

                if (Exclude)
                    added = clerk.SwapFilesInLists(fileList, cookInfo.Excluded, cookInfo.Included);
                else
                    added = clerk.SwapFilesInLists(fileList, cookInfo.Included, cookInfo.Excluded);
            }
            else
            {
                throw new Exception($"Invalid filter {filter}");
            }

            await clerk.SetCookInfo(cookInfo);

            foreach (var addedFile in added)
                logger.LogInformation(Exclude ? "-" : "+" + addedFile);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Failed to swap files");
            return ex.GetErrorCode();
        }

        return 0;
    }
}
