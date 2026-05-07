using System.CommandLine;
using System.CommandLine.IO;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using tot_lib;
using Tot;
using Tot.Commands;

namespace tot.Services;

public class KitchenClerk(Config config, KitchenFiles files, GitHandler git) : ITotService
{
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
    }

    public async Task SetCookInfoAndCommit(CookInfos infos)
    {
        await SetCookInfo(infos);
        await git.CommitFile(files.ModFolder, files.ModCookInfo, Constants.GitCommitCookinfoMessage);
    }

    public async Task UpdateModDevKitVersion()
    {
        var infos = await files.GetModInfos();
        var devkit = await GetDevKitVersion();
        if (infos.RevisionNumber == devkit.Revision && infos.SnapshotId == devkit.SnapshotId) return;
        
        if (await git.IsGitRepoInvalidOrDirty(files.ModFolder))
            throw new Exception("Mod repository is dirty");

        infos.RevisionNumber = devkit.Revision;
        infos.SnapshotId = devkit.SnapshotId;
        await files.SetModInfos(infos);
        await git.CommitFile(files.ModFolder, files.ModInfo,
            string.Format(
                Constants.GitCommitDevKitVersionMessage, 
                devkit.Revision, devkit.SnapshotId));
    }

    public async Task AutoBumpBuild()
    {
        if (!config.AutoBumpBuild) return;

        if (await git.IsGitRepoInvalidOrDirty(files.ModFolder))
            throw new Exception("Mod repository is dirty");
        var data = await files.GetModInfos();
        
        data.VersionBuild += 1;
        var regex = VersionCommand.TitleVersionRegex();
        data.Name = regex.Replace(data.Name,
            $"{data.VersionMajor}.{data.VersionMinor}.{data.VersionBuild}");
        
        await files.SetModInfos(data);
        await git.CommitFile(files.ModFolder, files.ModInfo,
            string.Format(
                Constants.GitCommitVersionMessage, 
                data.VersionMajor, data.VersionMinor, data.VersionBuild));
    }

    public async Task<DevKitVersion> GetDevKitVersion()
    {
        var content = await files.GetDevKitVersion();
        var regex = new Regex(@"([0-9]+)\.([0-9]+)");
        var result = regex.Match(content);
        if (!result.Success)
            throw new Exception("Version is invalid");
        return new DevKitVersion
        {
            Revision = int.Parse(result.Groups[1].Value),
            SnapshotId = int.Parse(result.Groups[2].Value)
        };
    }

    public async Task<string> QueryPakFile(FileInfo file)
    {
        if (!file.Exists)
            throw new FileNotFoundException($"File not found: {file}");

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
        List<string> swapped = new List<string>();
        foreach (var file in fileList)
        {
            var info = new FileInfo(file);
            var path = info.PosixFullName().RemoveBaseDir(files.DevKitContent);
            if (removeFrom.Contains(path))
                removeFrom.Remove(path);

            if (!addTo.Contains(path))
            {
                addTo.Add(path);
                swapped.Add(path);
            }
        }

        return swapped;
    }

    public List<string> TrimFileNotFound(List<string> list)
    {
        List<string> removed = new List<string>();
        foreach (var file in list.ToList())
            if (!new FileInfo(Path.Join(files.DevKitContent.FullName, file)).Exists)
            {
                list.Remove(file);
                removed.Add(file);
            }

        return removed;
    }

    public List<string> UpdateIncludedCookInfo(DirectoryInfo directory, CookInfos cookInfos)
    {
        if (!directory.Exists)
            throw new DirectoryNotFoundException($"Directory not found: {directory}");

        string[] fileList
            = Directory.GetFiles(directory.FullName, $"*{Constants.UAssetExt}", SearchOption.AllDirectories);
        List<string> added = new List<string>();
        foreach (var file in fileList)
        {
            var info = new FileInfo(file);
            var path = info.PosixFullName().RemoveBaseDir(files.DevKitContent);
            if (!cookInfos.Included.Contains(path) &&
                !cookInfos.Excluded.Contains(path))
            {
                cookInfos.Included.Add(path);
                added.Add(path);
            }
        }

        return added;
    }
}