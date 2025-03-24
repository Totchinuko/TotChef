using System.CommandLine;
using tot_lib;
using tot.Services;

namespace Tot.Commands;

public class StatusCommand : ModBasedCommand<StatusCommandOptions, StatusCommandHandler>
{
    public StatusCommand() : base("status", "List the content of the cookinfo.ini with file status")
    {
        var opt = new Option<bool>("--raw", "Display the raw list of the cookinfo.ini");
        opt.AddAlias("-r");
        AddOption(opt);
        var opt2 = new Option<string>("--search-pattern", "Get details on a specific folder");
        opt2.AddAlias("-s");
        AddOption(opt2);
    }
}

public class StatusCommandOptions : ModBasedCommandOptions
{
    public bool Raw { get; set; }
    public string SearchPattern { get; set; } = string.Empty;
}

public class StatusCommandHandler(IConsole console, KitchenFiles kitchenFiles, KitchenClerk clerk)
    : ModBasedCommandHandler<StatusCommandOptions>(kitchenFiles)
{
    private readonly KitchenFiles _kitchenFiles = kitchenFiles;

    public override async Task<int> HandleAsync(StatusCommandOptions options, CancellationToken cancellationToken)
    {
        await base.HandleAsync(options, cancellationToken);

        try
        {
            if (options.Raw)
            {
                await ExecuteRawList();
                return 0;
            }

            await ExecuteFriendly(options);
            return 0;
        }
        catch (CommandException ex)
        {
            return await console.OutputCommandError(ex);
        }
    }

    private async Task ExecuteFriendly(StatusCommandOptions options)
    {
        var filter = options.SearchPattern;
        if (!string.IsNullOrEmpty(options.SearchPattern))
        {
            if (!filter.PosixFullName().StartsWith(_kitchenFiles.DevKitContent.PosixFullName()))
                filter = Path.Join(_kitchenFiles.DevKitContent.FullName, filter);
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

        var files = Directory.GetFiles(_kitchenFiles.ModFolder.FullName, $"*{Constants.UAssetExt}",
            SearchOption.AllDirectories).ToList();
        files.AddRange(Directory.GetFiles(_kitchenFiles.ModsShared.FullName, $"*{Constants.UAssetExt}",
            SearchOption.AllDirectories));
        foreach (var file in files)
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
            var file = dir.Substring(_kitchenFiles.DevKitContent.FullName.Length);
            if (dir.StartsWith(_kitchenFiles.ModsShared.FullName))
                console.Write("Shared:" + file.PosixFullName() + " [");
            else if (dir.StartsWith(_kitchenFiles.ModLocalFolder.FullName))
                console.Write("Local:" + file.PosixFullName() + " ");
            else if (dir.StartsWith(_kitchenFiles.ModContentFolder.FullName))
                console.Write("Override:" + file.PosixFullName() + " ");
            else
                console.Write("Other:" + file.PosixFullName() + " ");

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
                    DumpList(value4, "+");
                if (excludedDir.TryGetValue(dir, out var value5))
                    DumpList(value5, "-");
                if (absentDir.TryGetValue(dir, out var value6))
                    DumpList(value6, "!");
                if (notFounDir.TryGetValue(dir, out var value7))
                    DumpList(value7, "?");
            }
        }
    }

    public void DumpList(List<string> list, string prefix)
    {
        foreach (var file in list)
            console.WriteLine(prefix + file);
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