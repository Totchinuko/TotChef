namespace Tot;

public static class Constants
{
    public const string CmdBinary = "UE4Editor-Cmd.exe";
    public const string EditorBinary = "UE4Editor.exe";
    public const string PakBinary = "UnrealPak.exe";

    public const string GraniteSdkEnvKey = "GRANITESDK_PATH";
    public const string CookInfoHeader = "[/CookInfo]";
    public const string CookLogArg = "-abslog";
    public const string ExcludePrefix = "UnselectedFiles=";
    public const string IncludePrefix = "FilesToCook=";

    public const string ActiveFile = "active.txt";
    public const string ModInfosFile = "modinfo.json";
    public const string CookInfosFile = "CookInfo.ini";
    public const string VersionFile = "version.txt";
    public const string UProject = "ConanSandbox.uproject";

    public const string PakExt = ".pak";
    public const string TxtExt = ".txt";
    public const string UAssetExt = ".uasset";
    public const string UMapExt = ".umap";
    public const string BackupAddedName = ".backup";

    public const string LocalDirDkGraniteSdk = "Engine/Source/ThirdParty/GraniteSDK";
    public const string LocalDirDkConanSandbox = "Games/ConanSandbox";
    public const string LocalDirDkContent = "Content";
    public const string LocalDirDkSaved = "Saved";
    public const string LocalDirDkMods = "Mods";
    public const string LocalDirDkModsShared = "ModsShared";
    public const string LocalDirDkCooking = "EditorCooked/WindowsNoEditor/ConanSandbox/Content";
    public const string LocalDirDkBin = "Engine/Binaries/Win64";

    public const string LocalDirModShared = "Shared";
    public const string LocalDirModLocal = "Local";
    public const string LocalDirModContent = "Content";

    public const string LocalDirTmpCookedMods = "Saved/Mods/CookedMods";
    public const string LocalDirTmpLogs = "Saved/Mods/Logs";
    public const string LocalDirTmpModFiles = "Saved/Mods/ModFiles";

    public static readonly string[] CookArgs =
    [
        "-installed", "-ModDevKit", "-run=cookmod", "targetplatform=WindowsNoEditor", "-iterate", "-compressed",
        "-stdout", "-unattended", "-fileopenlog"
    ];

    public static readonly string[] EditorArgs = ["-ModDevKit", "-Installed"];
    
    public const string GitCommitVersionMessage = "Bump version to {0}.{1}.{2}";
    public const string GitCommitDevKitVersionMessage = "Bump Devkit version to {0}.{1}";
    public const string GitCommitCookinfoMessage = "Update cooking infos";
    public const string GitCommitDescriptionMessage = "Update mod description";
    
    public const string ConfigFileName = "config.json";

}