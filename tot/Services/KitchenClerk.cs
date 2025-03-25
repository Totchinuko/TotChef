using System.CommandLine;
using System.CommandLine.IO;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using tot_lib;
using Tot;
using Tot.Commands;

namespace tot.Services;

public class KitchenClerk(Config config, KitchenFiles files, GitHandler git, IColoredConsole console) : ITotService
{
    public void CleanCookedFolder()
    {
        if (!files.ModCookedFolder.Exists || (files.ModCookedFolder.GetFiles().Length == 0 &&
                                              files.ModCookedFolder.GetDirectories().Length == 0)) return;
        foreach (var fileInfo in files.ModCookedFolder.GetFiles())
            fileInfo.Delete();
        foreach (var directory in files.ModCookedFolder.GetDirectories())
            directory.Delete(true);
    }

    public void CleanCookingFolder()
    {
        if (!files.EditorSavedFolder.Exists || (files.EditorSavedFolder.GetFiles().Length == 0 &&
                                                files.EditorSavedFolder.GetDirectories().Length == 0)) return;
        foreach (var fileInfo in files.EditorSavedFolder.GetFiles())
            fileInfo.Delete();
        foreach (var directory in files.EditorSavedFolder.GetDirectories())
            directory.Delete(true);
    }

    public List<string> ConvertToCookingFolder(List<string> fileList)
    {
        List<string> result = new List<string>();
        foreach (var file in fileList)
        {
            var converted = file;
            if (file.StartsWith(files.ModLocalFolder.PosixFullName()))
                converted = Path
                    .Join(Constants.LocalDirDkMods, files.ModName, file.RemoveRootFolder(files.ModLocalFolder))
                    .PosixFullName().RemoveExtension();
            else if (file.StartsWith(files.ModContentFolder.PosixFullName()))
                converted = file.RemoveRootFolder(files.ModContentFolder).PosixFullName().RemoveExtension();
            else if (file.StartsWith(files.ModSharedFolder.PosixFullName()))
                converted = Path.Join(Constants.LocalDirDkModsShared, file.RemoveRootFolder(files.ModSharedFolder))
                    .PosixFullName().RemoveExtension();
            else if (file.StartsWith(files.ModsShared.PosixFullName()))
                converted = Path.Join(Constants.LocalDirDkModsShared, file.RemoveRootFolder(files.ModsShared))
                    .PosixFullName().RemoveExtension();

            result.Add(converted);
        }

        return result;
    }

    public async Task CopyAndFilter(bool verbose)
    {
        var cookInfos = await GetCookInfo();
        List<string> cookedFiles = Directory.GetFiles(files.CookingFolder.FullName, "*", SearchOption.AllDirectories)
            .ToList();
        HashSet<string> validFiles = ConvertToCookingFolder(cookInfos.Included).ToHashSet();
        HashSet<string> checkedFiles = new HashSet<string>();

        for (var i = 0; i < cookedFiles.Count; i++)
        {
            var file = cookedFiles[i];
            var localFile = file.RemoveRootFolder(files.CookingFolder).PosixFullName().RemoveExtension();

            if (!validFiles.Contains(localFile))
            {
                console.WriteLine($"Ignoring:{file}");
                continue;
            }

            checkedFiles.Add(localFile);

            FileInfo from = new(file);
            FileInfo to = new(Path.Join(files.ModCookedFolder.FullName, localFile) + Path.GetExtension(file));
            if (to.Directory == null)
            {
                console.WriteLine($"Ignoring:{file}");
                continue;
            }

            if (verbose)
                console.WriteLine("Copy:" + from.FullName);

            Directory.CreateDirectory(to.Directory.FullName);
            if (to.Exists)
                throw new CommandException(
                    $"Could not copy the following file, already exists\n{to.FullName}");

            from.CopyTo(to.FullName, true);
        }

        if (checkedFiles.Count != validFiles.Count)
        {
            foreach (var file in validFiles.Except(checkedFiles))
                console.Error.WriteLine("Error:" + file);
            throw new CommandException("CookInfos contain invalid files");
        }
    }

    public async Task<CookInfos> GetCookInfo()
    {
        var cookInfo = new CookInfos();

        if (!files.ModCookInfo.Exists) return cookInfo;

        var lines = await files.GetCookInfos();
        foreach (var line in lines)
            if (line.StartsWith(Constants.IncludePrefix) &&
                !string.IsNullOrEmpty(line.Substring(Constants.IncludePrefix.Length)))
                cookInfo.Included.Add(line.Substring(Constants.IncludePrefix.Length));
            else if (line.StartsWith(Constants.ExcludePrefix) &&
                     !string.IsNullOrEmpty(line.Substring(Constants.ExcludePrefix.Length)))
                cookInfo.Excluded.Add(line.Substring(Constants.ExcludePrefix.Length));
        return cookInfo;
    }

    public async Task SetCookInfo(CookInfos infos)
    {
        var sb = new StringBuilder();
        sb.AppendLine(Constants.CookInfoHeader);
        infos.Included.Sort();
        infos.Excluded.Sort();

        foreach (var line in infos.Excluded)
            sb.AppendLine(Constants.ExcludePrefix + line);
        foreach (var line in infos.Included)
            sb.AppendLine(Constants.IncludePrefix + line);

        await files.SetCookInfos(sb.ToString());
        git.CommitFile(files.ModFolder, files.ModCookInfo, "Update mod cookinfos");
    }

    public async Task UpdateModDevKitVersion()
    {
        if (git.IsGitRepoDirty(files.ModFolder))
            throw new CommandException(CommandCode.RepositoryIsDirty, "Mod repository is dirty");

        var infos = await files.GetModInfos();
        var devkit = await GetDevKitVersion();
        if (infos.RevisionNumber == devkit.Revision && infos.SnapshotId == devkit.SnapshotId) return;

        infos.RevisionNumber = devkit.Revision;
        infos.SnapshotId = devkit.SnapshotId;
        await files.SetModInfos(infos);
        git.CommitFile(files.ModFolder, files.ModInfo,
            $"Bump DevKit version to {devkit.Revision}.{devkit.SnapshotId}");
    }

    public async Task AutoBumpBuild()
    {
        if (!config.AutoBumpBuild) return;

        if (git.IsGitRepoDirty(files.ModFolder))
            throw new CommandException(CommandCode.RepositoryIsDirty, "Mod repository is dirty");
        var data = await files.GetModInfos();
        
        data.VersionBuild += 1;
        var regex = VersionCommand.TitleVersionRegex();
        data.Name = regex.Replace(data.Name,
            $"{data.VersionMajor}.{data.VersionMinor}.{data.VersionBuild}");
        
        await files.SetModInfos(data);
        git.CommitFile(files.ModFolder, files.ModInfo,
            $"Bump to {data.VersionMajor}.{data.VersionMinor}.{data.VersionBuild}");
    }

    public async Task<DevKitVersion> GetDevKitVersion()
    {
        var content = await files.GetDevKitVersion();
        var regex = new Regex(@"([0-9]+)\.([0-9]+)");
        var result = regex.Match(content);
        if (!result.Success)
            throw new CommandException("Version is invalid");
        return new DevKitVersion
        {
            Revision = int.Parse(result.Groups[1].Value),
            SnapshotId = int.Parse(result.Groups[2].Value)
        };
    }

    public async Task<string> QueryPakFile(FileInfo file)
    {
        if (!file.Exists)
            throw CommandCode.NotFound(file);

        var p = new Process();
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.FileName = files.UnrealPak.FullName;
        p.StartInfo.Arguments = string.Join(" ", new[]
        {
            $"\"{file.FullName}\"",
            "-List"
        });
        p.Start();
        var output = await p.StandardOutput.ReadToEndAsync();
        await p.WaitForExitAsync();
        return output;
    }

    public List<string> RemoveMissingFiles(CookInfos cookInfo)
    {
        List<string> change = TrimFileNotFound(cookInfo.Included);
        change.AddRange(TrimFileNotFound(cookInfo.Excluded));
        return change;
    }

    public List<string> SwapFilesInLists(List<string> fileList, List<string> addTo, List<string> removeFrom)
    {
        List<string> added = new List<string>();
        foreach (var file in fileList)
        {
            var info = new FileInfo(file);
            if (removeFrom.Contains(info.PosixFullName()))
                removeFrom.Remove(info.PosixFullName());

            if (!addTo.Contains(info.PosixFullName()))
            {
                addTo.Add(info.PosixFullName());
                added.Add(info.PosixFullName());
            }
        }

        return added;
    }

    public List<string> TrimFileNotFound(List<string> list)
    {
        List<string> removed = new List<string>();
        foreach (var file in list.ToList())
            if (!new FileInfo(file).Exists)
            {
                list.Remove(file);
                removed.Add(file);
            }

        return removed;
    }

    public List<string> UpdateIncludedCookInfo(DirectoryInfo directory, CookInfos cookInfos)
    {
        if (!directory.Exists)
            throw CommandCode.NotFound(directory);

        string[] fileList
            = Directory.GetFiles(directory.FullName, $"*{Constants.UAssetExt}", SearchOption.AllDirectories);
        List<string> added = new List<string>();
        foreach (var file in fileList)
        {
            var info = new FileInfo(file);
            if (!cookInfos.Included.Contains(info.PosixFullName()) &&
                !cookInfos.Excluded.Contains(info.PosixFullName()))
            {
                cookInfos.Included.Add(info.PosixFullName());
                added.Add(info.PosixFullName());
            }
        }

        return added;
    }
}