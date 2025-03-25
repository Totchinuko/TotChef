using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using tot_lib;
using tot.Services;

namespace Tot.Commands;

public class StatusCommand : ModBasedCommand, ITotCommand
{
    public string Command => "status";
    public string Description => "List the content of the cookinfo.ini with file status";
    
    public bool Raw { get; set; }
    public string SearchPattern { get; set; } = string.Empty;

    public override IEnumerable<Option> GetOptions()
    {
        foreach (var option in base.GetOptions())
            yield return option;
        
        var opt = new TotOption<bool>("--raw", "Display the raw list of the cookinfo.ini");
        opt.AddAlias("-r");
        opt.AddSetter(x => Raw = x);
        yield return opt;
        var opt2 = new TotOption<string>("--search-pattern", "Get details on a specific folder");
        opt2.AddSetter(x => SearchPattern = x ?? string.Empty);
        opt2.AddAlias("-s");
        yield return opt2;
    }

    public override async Task<int> InvokeAsync(IServiceProvider provider, CancellationToken token)
    {
        var console = provider.GetRequiredService<IColoredConsole>();
        var clerk = provider.GetRequiredService<KitchenClerk>();
        
        try
        {
            await base.InvokeAsync(provider, token);

            if (Raw)
            {
                await ExecuteRawList(clerk, console);
                return 0;
            }

            var kFiles = provider.GetRequiredService<KitchenFiles>();
            await ExecuteFriendly(kFiles, clerk, console);
            return 0;
        }
        catch (CommandException ex)
        {
            return await console.OutputCommandError(ex);
        }
    }

    private async Task ExecuteFriendly(KitchenFiles kFiles, KitchenClerk clerk, IColoredConsole console)
    {
        var filter = SearchPattern;
        if (!string.IsNullOrEmpty(SearchPattern))
        {
            if (!filter.PosixFullName().StartsWith(kFiles.DevKitContent.PosixFullName()))
                filter = Path.Join(kFiles.DevKitContent.FullName, filter);
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

        var files = Directory.GetFiles(kFiles.ModFolder.FullName, $"*{Constants.UAssetExt}",
            SearchOption.AllDirectories).ToList();
        files.AddRange(Directory.GetFiles(kFiles.ModsShared.FullName, $"*{Constants.UAssetExt}",
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
            var file = dir.Substring(kFiles.DevKitContent.FullName.Length);
            if (dir.StartsWith(kFiles.ModsShared.FullName))
                console.Write("Shared:" + file.PosixFullName() + " [");
            else if (dir.StartsWith(kFiles.ModLocalFolder.FullName))
                console.Write("Local:" + file.PosixFullName() + " ");
            else if (dir.StartsWith(kFiles.ModContentFolder.FullName))
                console.Write("Override:" + file.PosixFullName() + " ");
            else
                console.Write("Other:" + file.PosixFullName() + " ");

            if (includedDir.TryGetValue(dir, out var value))
                console.Write(ConsoleColor.Green, $"+{value.Count}");
            if (excludedDir.TryGetValue(dir, out var value1))
                console.Write(ConsoleColor.Red, $"-{value1.Count}");
            if (absentDir.TryGetValue(dir, out var value2))
                console.Write(ConsoleColor.Yellow, $"!{value2.Count}");
            if (notFounDir.TryGetValue(dir, out var value3))
                console.Write(ConsoleColor.Magenta, $"?{value3.Count}");
            console.Write("]\n");

            if (!string.IsNullOrEmpty(filter))
            {
                if (includedDir.TryGetValue(dir, out var value4))
                    console.WriteLines(ConsoleColor.Green, [.. value4]);
                if (excludedDir.TryGetValue(dir, out var value5))
                    console.WriteLines(ConsoleColor.Red, [.. value5]);
                if (absentDir.TryGetValue(dir, out var value6))
                    console.WriteLines(ConsoleColor.Yellow, [.. value6]);
                if (notFounDir.TryGetValue(dir, out var value7))
                    console.WriteLines(ConsoleColor.Magenta, [.. value7]);
            }
        }
    }
    
    private async Task ExecuteRawList(KitchenClerk clerk, IColoredConsole console)
    {
        var cookInfo = await clerk.GetCookInfo();
        foreach (var file in cookInfo.Included)
            console.WriteLine("+" + file);
        foreach (var file in cookInfo.Excluded)
            console.WriteLine("-" + file);
    }
}