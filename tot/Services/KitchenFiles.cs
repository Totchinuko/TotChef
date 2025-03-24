using System.Text.Json;
using tot_lib;
using Tot;

namespace tot.Services;

public class KitchenFiles(Config config)
{
    private string _modName = string.Empty;

    public DirectoryInfo CookedFiles =>
        new DirectoryInfo(Path.Join(TempFolder.FullName, Constants.LocalDirTmpCookedMods))
            .GetProperCasedDirectoryInfo();

    public DirectoryInfo EditorSavedFolder => new DirectoryInfo(Path.Join(DevKit.FullName,
            Constants.LocalDirDkConanSandbox, Constants.LocalDirDkSaved))
        .GetProperCasedDirectoryInfo();

    public DirectoryInfo CookingFolder =>
        new DirectoryInfo(Path.Join(EditorSavedFolder.FullName, Constants.LocalDirDkCooking))
            .GetProperCasedDirectoryInfo();

    public FileInfo CookLogFile =>
        new FileInfo(Path.Join(LogFiles.FullName, ModName + Constants.TxtExt)).GetProperCasedFileInfo();

    public DirectoryInfo DevKit => new DirectoryInfo(config.DevKitPath).GetProperCasedDirectoryInfo();

    public DirectoryInfo DevKitContent => new DirectoryInfo(Path.Join(DevKit.FullName, Constants.LocalDirDkConanSandbox,
            Constants.LocalDirDkContent))
        .GetProperCasedDirectoryInfo();

    public DirectoryInfo GraniteSdkDir => new(Path.Join(DevKit.FullName, Constants.LocalDirDkGraniteSdk));

    public DirectoryInfo LogFiles =>
        new DirectoryInfo(Path.Join(TempFolder.FullName, Constants.LocalDirTmpLogs)).GetProperCasedDirectoryInfo();

    public DirectoryInfo ModContentFolder =>
        new DirectoryInfo(Path.Join(ModFolder.FullName, Constants.LocalDirModContent)).GetProperCasedDirectoryInfo();

    public DirectoryInfo ModCookedFolder =>
        new DirectoryInfo(Path.Join(CookedFiles.FullName, ModName)).GetProperCasedDirectoryInfo();

    public FileInfo ModCookInfo =>
        new FileInfo(Path.Join(ModLocalFolder.FullName, Constants.CookInfosFile)).GetProperCasedFileInfo();

    public DirectoryInfo ModFolder =>
        new DirectoryInfo(Path.Join(ModsFolder.FullName, ModName)).GetProperCasedDirectoryInfo();

    public FileInfo ModInfo =>
        new FileInfo(Path.Join(ModFolder.FullName, Constants.ModInfosFile)).GetProperCasedFileInfo();

    public DirectoryInfo ModLocalFolder =>
        new DirectoryInfo(Path.Join(ModFolder.FullName, Constants.LocalDirModLocal)).GetProperCasedDirectoryInfo();

    public string ModName =>
        new DirectoryInfo(Path.Join(ModsFolder.FullName, _modName)).GetProperCasedDirectoryInfo().Name;

    public FileInfo ModPakFile =>
        new FileInfo(Path.Join(ModPakFolder.FullName, ModName + Constants.PakExt)).GetProperCasedFileInfo();

    public FileInfo ModPakFileBackup =>
        new FileInfo(Path.Join(ModPakFolder.FullName, ModName + Constants.BackupAddedName + Constants.PakExt))
            .GetProperCasedFileInfo();

    public DirectoryInfo ModPakFolder =>
        new DirectoryInfo(Path.Join(PakFiles.FullName, ModName)).GetProperCasedDirectoryInfo();

    public DirectoryInfo ModsFolder =>
        new DirectoryInfo(Path.Join(DevKitContent.FullName, Constants.LocalDirDkMods)).GetProperCasedDirectoryInfo();

    public DirectoryInfo ModSharedFolder =>
        new DirectoryInfo(Path.Join(ModFolder.FullName, Constants.LocalDirModShared)).GetProperCasedDirectoryInfo();

    public DirectoryInfo ModsShared =>
        new DirectoryInfo(Path.Join(DevKitContent.FullName, Constants.LocalDirDkModsShared))
            .GetProperCasedDirectoryInfo();

    public DirectoryInfo PakFiles => new DirectoryInfo(Path.Join(TempFolder.FullName, Constants.LocalDirTmpModFiles))
        .GetProperCasedDirectoryInfo();

    public DirectoryInfo TempFolder =>
        new DirectoryInfo(Path.Join(Path.GetTempPath(), "../ConanSandbox")).GetProperCasedDirectoryInfo();

    public FileInfo Ue4Cmd => new FileInfo(Path.Join(DevKit.FullName, Constants.LocalDirDkBin, Constants.CmdBinary))
        .GetProperCasedFileInfo();

    public FileInfo Ue4Editor =>
        new FileInfo(Path.Join(DevKit.FullName, Constants.LocalDirDkBin, Constants.EditorBinary))
            .GetProperCasedFileInfo();

    public FileInfo DevKitVersion => new FileInfo(Path.Join(DevKit.FullName, Constants.VersionFile))
        .GetProperCasedFileInfo();

    public FileInfo UnrealPak => new FileInfo(Path.Join(DevKit.FullName, Constants.LocalDirDkBin, Constants.PakBinary))
        .GetProperCasedFileInfo();

    public FileInfo UProject =>
        new FileInfo(Path.Join(DevKit.FullName, Constants.LocalDirDkConanSandbox, Constants.UProject))
            .GetProperCasedFileInfo();

    public void SetModName(string name)
    {
        if (IsDevkitPathValid())
            _modName = string.IsNullOrEmpty(name) ? GetCurrentDirectoryMod() ?? "" : name;
        else
            _modName = name;
    }

    public async Task<string[]> GetCookInfos()
    {
        return await File.ReadAllLinesAsync(ModCookInfo.FullName);
    }

    public async Task SetCookInfos(string content)
    {
        await File.WriteAllTextAsync(ModCookInfo.FullName, content);
    }

    public string? GetCurrentDirectoryMod()
    {
        var current = new DirectoryInfo(Directory.GetCurrentDirectory());
        if (!current.FullName.StartsWith(ModsFolder.FullName)) return null;

        var local = current.FullName.RemoveRootFolder(ModsFolder.FullName);
        var modName = local.Split([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
            StringSplitOptions.RemoveEmptyEntries)[0];
        var path = Path.Join(ModsFolder.FullName, modName, Constants.ModInfosFile);
        return File.Exists(path) ? modName : null;
    }

    public void CreateActive()
    {
        if (!IsModPathValid()) throw CommandCode.NotFound(ModFolder);
        var file = new FileInfo(Path.Join(ModsFolder.FullName, _modName, Constants.ActiveFile))
            .GetProperCasedFileInfo();
        File.WriteAllText(file.FullName, "");
    }

    public void DeleteAnyActive()
    {
        if (!ModsFolder.Exists) throw CommandCode.NotFound(ModFolder);

        foreach (var dir in ModsFolder.GetDirectories())
        {
            var file = new FileInfo(Path.Join(dir.FullName, Constants.ActiveFile));
            if (file.Exists)
                file.Delete();
        }
    }

    public async Task<bool> FileContain(FileInfo file, string content)
    {
        if (!file.Exists) return false;
        return (await File.ReadAllTextAsync(file.FullName)).Contains(content);
    }

    public void CreateModPakBackup()
    {
        if (ModPakFile.Exists)
        {
            if (ModPakFileBackup.Exists)
                ModPakFileBackup.Delete();
            ModPakFile.MoveTo(ModPakFileBackup.FullName);
        }
    }

    public async Task<ModinfoData> GetModInfos()
    {
        var json = await File.ReadAllTextAsync(ModInfo.FullName);
        return JsonSerializer.Deserialize(json, ModinfoDataJsonContext.Default.ModinfoData) ?? new ModinfoData();
    }

    public async Task SetModInfos(ModinfoData data)
    {
        var json = JsonSerializer.Serialize(data, ModinfoDataJsonContext.Default.ModinfoData);
        await File.WriteAllTextAsync(ModInfo.FullName, json);
    }

    public async Task<string> GetDevKitVersion()
    {
        return await File.ReadAllTextAsync(DevKitVersion.FullName);
    }

    public async Task<string> CreateTemporaryTextFile(string content)
    {
        var guid = Guid.NewGuid().ToString();
        var file = new FileInfo(Path.Join(Path.GetTempPath(), Constants.TotFolder, guid + Constants.TxtExt));
        if (file.Directory == null) throw new CommandException("Cannot create temporary text file");

        Directory.CreateDirectory(file.Directory.FullName);
        await File.WriteAllTextAsync(file.FullName, content);

        return file.FullName;
    }

    public bool IsDevkitPathValid()
    {
        return !string.IsNullOrEmpty(config?.DevKitPath) && Ue4Cmd.Exists;
    }

    public bool IsModPathValid()
    {
        return ModInfo.Exists;
    }
}