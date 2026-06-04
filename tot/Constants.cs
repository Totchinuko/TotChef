namespace Tot;

public static class Constants
{
    public const string CmdBinary = "UnrealEditor-Cmd.exe";
    public const string EditorBinary = "UnrealEditor.exe";
    public const string PakBinary = "UnrealPak.exe";

    public const string CookInfoHeader = "[/CookInfo]";
    public const string CookLogArg = "-abslog";
    public const string CookProjectArg = "-Project";
    public const string CookScriptDirArg = "-ScriptDir";
    public const string CookOutputDirArg = "-Output";
    public const string CookModArg = "-Mod";
    public const string ExcludePrefix = "UnselectedFiles=";
    public const string IncludePrefix = "FilesToCook=";

    public const string ActiveFile = "active.txt";
    public const string ModInfosFile = "modinfo.json";
    public const string ModStatusFile = "modstatus.json";
    public const string CookInfosFile = "CookInfo.ini";
    public const string VersionFile = "version.txt";
    public const string UProject = "ConanSandbox.uproject";
    public const string RunUat = "RunUAT.bat";

    public const string PakExt = ".pak";
    public const string TxtExt = ".txt";
    public const string UAssetExt = ".uasset";
    public const string UMapExt = ".umap";
    public const string BackupAddedName = ".backup";

    public const string LocalDirDkConanSandbox = "UE4";
    public const string LocalDirDkContent = "Content";
    public const string LocalDirDkSaved = "Saved";
    public const string LocalDirDkMods = "Mods";
    public const string LocalDirDkOutput = "Output";
    public const string LocalDirDkModsShared = "ModsShared";
    public const string LocalDirDkBin = "Engine/Binaries/Win64";
    public const string LocalDirDkBatch = "Engine/Build/BatchFiles";

    public const string LocalDirModShared = "Shared";
    public const string LocalDirModLocal = "Local";
    public const string LocalDirModContent = "Content";

    public const string LocalDirTmpCookedMods = "Saved/Mods/CookedMods";
    public const string LocalDirTmpLogs = "Saved/Mods/Logs";
    public const string LocalDirTmpModFiles = "Saved/Mods/ModFiles";

    public static readonly string[] CookArgsFirstPass = [
        "-NoCompile", "BuildMod"
    ];
    public static readonly string[] CookArgsSecondPass =
    [
        "-Cook", "-Pak", "-FinalPak", "-Compress"
    ];

    public static readonly string[] EditorArgs = ["-ModDevKit"];
    
    public const string GitCommitVersionMessage = "Bump version to {0}.{1}.{2}";
    public const string GitCommitDevKitVersionMessage = "Bump Devkit version to {0}.{1}";
    public const string GitCommitCookinfoMessage = "Update cooking infos";
    public const string GitCommitDescriptionMessage = "Update mod description";
    
    public const string ConfigFileName = "config_enhanced.json";

    public const string PatchNoteFile = ".patch";

    public const string ColorRed = "#e93519";
    public const string ColorGreen = "#68c355";
    public const string ColorYellow = "#f1d22e";
    public const string ColorBlue = "#1b96ec";
    public const string ColorPurple = "#8f65c8";
    public const string ColorOrange = "#ff9800";
    public const string ColorGrey = "#babbb9";
    public const string ColorAccent = "#A5E5FA";

}