using System.CommandLine;
using Microsoft.Extensions.Logging;
using tot_lib;
using tot_lib.CommandLine;
using tot.Services;

namespace Tot.Commands;

public class StatusCommand(ILogger<StatusCommand> logger, IConsole console, KitchenClerk clerk, KitchenFiles files) : IInvokableCommand<StatusCommand>
{
    public static readonly Command Command = CommandBuilder
        .CreateInvokable<StatusCommand>("status", "List the content of the cookinfo.ini with file status")
        .SetServiceConfiguration(Program.ConfigureServices)
        .Options.Create<bool>("--raw", "Display the raw list of the cookinfo.ini").AddAlias("-r")
        .AddSetter((c,v) => c.Raw = v).BuildOption()
        .Options.Create<string>("--search-pattern", "Get details on a specific folder").AddAlias("-s")
        .AddSetter((c,v) => c.SearchPattern = v ?? string.Empty).BuildOption()
        .Options.AddModName((c,v) => c.ModName = v)
        .BuildCommand();
    
    public bool Raw { get; set; }
    public string SearchPattern { get; set; } = string.Empty;
    public string ModName { get; set; } = string.Empty;

    public async Task<int> InvokeAsync(CancellationToken token)
    {
        try
        {
            files.SetModName(ModName);

            if (Raw)
            {
                await ExecuteRawList();
                return 0;
            }

            await ExecuteFriendly();
            return 0;
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Failed to build status report");
            return ex.GetErrorCode();
        }
    }

    private async Task ExecuteFriendly()
    {
        var filter = SearchPattern;
        if (!string.IsNullOrEmpty(SearchPattern))
        {
            if (!filter.PosixFullName().StartsWith(files.DevKitContent.PosixFullName()))
                filter = Path.Join(files.DevKitContent.FullName, filter);
            if (Directory.Exists(filter))
                filter = new DirectoryInfo(filter).GetProperCasedDirectoryInfo().PosixFullName();
            else
                filter = null;
        }

        var cookInfo = await clerk.GetCookInfo();
        Dictionary<string, List<string>> includedDir = new Dictionary<string, List<string>>();
        Dictionary<string, List<string>> excludedDir = new Dictionary<string, List<string>>();
        Dictionary<string, List<string>> absentDir = new Dictionary<string, List<string>>();
        Dictionary<string, List<string>> notFounDir = new Dictionary<string, List<string>>();
        List<string> directories = new List<string>();

        foreach (var file in cookInfo.Included)
        {
            if (!string.IsNullOrEmpty(filter) && !file.StartsWith(filter))
                continue;

            var info = new FileInfo(file);
            if (info.Directory == null) continue;

            if (!directories.Contains(info.Directory.FullName))
                directories.Add(info.Directory.FullName);
            if (includedDir.ContainsKey(info.Directory.FullName))
                includedDir[info.Directory.FullName].Add(file);
            else includedDir.Add(info.Directory.FullName, new List<string> { file });

            if (!info.Exists)
                if (notFounDir.ContainsKey(info.Directory.FullName))
                    notFounDir[info.Directory.FullName].Add(file);
                else notFounDir.Add(info.Directory.FullName, new List<string> { file });
        }

        foreach (var file in cookInfo.Excluded)
        {
            if (!string.IsNullOrEmpty(filter) && !file.StartsWith(filter))
                continue;

            var info = new FileInfo(file);
            if (info.Directory == null) continue;

            if (!directories.Contains(info.Directory.FullName))
                directories.Add(info.Directory.FullName);
            if (excludedDir.ContainsKey(info.Directory.FullName))
                excludedDir[info.Directory.FullName].Add(file);
            else excludedDir.Add(info.Directory.FullName, new List<string> { file });

            if (!info.Exists)
                if (notFounDir.ContainsKey(info.Directory.FullName))
                    notFounDir[info.Directory.FullName].Add(file);
                else notFounDir.Add(info.Directory.FullName, new List<string> { file });
        }

        var fileList = Directory.GetFiles(files.ModFolder.FullName, $"*{Constants.UAssetExt}",
            SearchOption.AllDirectories).ToList();
        fileList.AddRange(Directory.GetFiles(files.ModsShared.FullName, $"*{Constants.UAssetExt}",
            SearchOption.AllDirectories));
        foreach (var file in fileList)
        {
            if (!string.IsNullOrEmpty(filter) && !file.PosixFullName().StartsWith(filter))
                continue;

            var info = new FileInfo(file);
            if (info.Directory == null) continue;

            if (cookInfo.Included.Contains(info.PosixFullName()) ||
                cookInfo.Excluded.Contains(info.PosixFullName())) continue;
            if (!directories.Contains(info.Directory.FullName))
                directories.Add(info.Directory.FullName);
            if (absentDir.ContainsKey(info.Directory.FullName))
                absentDir[info.Directory.FullName].Add(file);
            else absentDir.Add(info.Directory.FullName, new List<string> { file });
        }

        directories.Sort();
        foreach (var dir in directories)
        {
            var file = dir.Substring(files.DevKitContent.FullName.Length);
            if (dir.StartsWith(files.ModsShared.FullName))
                console.Write("Shared:" + file.PosixFullName() + " [");
            else if (dir.StartsWith(files.ModLocalFolder.FullName))
                console.Write("Local:" + file.PosixFullName() + " [");
            else if (dir.StartsWith(files.ModContentFolder.FullName))
                console.Write("Override:" + file.PosixFullName() + " [");
            else
                console.Write("Other:" + file.PosixFullName() + " [");

            if (includedDir.TryGetValue(dir, out var value))
                console.Write($"+{value.Count}");
            if (excludedDir.TryGetValue(dir, out var value1))
                console.Write($"-{value1.Count}");
            if (absentDir.TryGetValue(dir, out var value2))
                console.Write($"!{value2.Count}");
            if (notFounDir.TryGetValue(dir, out var value3))
                console.Write($"?{value3.Count}");
            console.Write("]\n");

            if (!string.IsNullOrEmpty(filter))
            {
                if (includedDir.TryGetValue(dir, out var value4))
                    foreach (var v in value4)
                        console.WriteLine($"+{v}");
                if (excludedDir.TryGetValue(dir, out var value5))
                    foreach (var v in value5)
                        console.WriteLine($"-{v}");
                if (absentDir.TryGetValue(dir, out var value6))
                    foreach (var v in value6)
                        console.WriteLine($"!{v}");
                if (notFounDir.TryGetValue(dir, out var value7))
                    foreach (var v in value7)
                        console.WriteLine($"?{v}");
            }
        }
    }
    
    private async Task ExecuteRawList()
    {
        var cookInfo = await clerk.GetCookInfo();
        foreach (var file in cookInfo.Included)
            console.WriteLine("+" + file);
        foreach (var file in cookInfo.Excluded)
            console.WriteLine("-" + file);
    }
}