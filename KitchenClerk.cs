using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace Tot
{
    internal class KitchenClerk
    {
        public DirectoryInfo TempFolder => new DirectoryInfo(Path.Join(Path.GetTempPath(), "../ConanSandbox")).GetProperCasedDirectoryInfo();
        public DirectoryInfo DevKit => new DirectoryInfo(config.DevKitPath).GetProperCasedDirectoryInfo();
        public DirectoryInfo DevKitContent => new DirectoryInfo(Path.Join(DevKit.FullName, "Games/ConanSandbox/Content")).GetProperCasedDirectoryInfo();
        public DirectoryInfo ModsFolder => new DirectoryInfo(Path.Join(DevKitContent.FullName, "Mods")).GetProperCasedDirectoryInfo();
        public DirectoryInfo ModsShared => new DirectoryInfo(Path.Join(DevKitContent.FullName, "ModsShared")).GetProperCasedDirectoryInfo();
        public DirectoryInfo CookingFolder => new DirectoryInfo(Path.Join(DevKit.FullName, "Games/ConanSandbox/Saved/EditorCooked/WindowsNoEditor/ConanSandbox/Content")).GetProperCasedDirectoryInfo();
        public DirectoryInfo LogFiles => new DirectoryInfo(Path.Join(TempFolder.FullName, "Saved/Mods/Logs")).GetProperCasedDirectoryInfo();
        public DirectoryInfo PakFiles => new DirectoryInfo(Path.Join(TempFolder.FullName, "Saved/Mods/ModFiles")).GetProperCasedDirectoryInfo();
        public DirectoryInfo CookedFiles => new DirectoryInfo(Path.Join(TempFolder.FullName, "Saved/Mods/CookedMods")).GetProperCasedDirectoryInfo();

        public DirectoryInfo ModFolder => new DirectoryInfo(Path.Join(ModsFolder.FullName, ModName)).GetProperCasedDirectoryInfo();
        public DirectoryInfo ModLocalFolder => new DirectoryInfo(Path.Join(ModFolder.FullName, "Local")).GetProperCasedDirectoryInfo();
        public DirectoryInfo ModContentFolder => new DirectoryInfo(Path.Join(ModFolder.FullName, "Content")).GetProperCasedDirectoryInfo();
        public DirectoryInfo ModPakFolder => new DirectoryInfo(Path.Join(PakFiles.FullName, ModName)).GetProperCasedDirectoryInfo();
        public DirectoryInfo ModCookedFolder => new DirectoryInfo(Path.Join(CookedFiles.FullName, ModName)).GetProperCasedDirectoryInfo();

        public FileInfo UE4CMD => new FileInfo(Path.Join(DevKit.FullName, CMDBinary)).GetProperCasedFileInfo();
        public FileInfo UProject => new FileInfo(Path.Join(DevKit.FullName, "Games/ConanSandbox/ConanSandbox.uproject")).GetProperCasedFileInfo();
        public FileInfo UnrealPak => new FileInfo(Path.Join(DevKit.FullName, "Engine/Binaries/Win64/UnrealPak.exe")).GetProperCasedFileInfo();
        public FileInfo UE4Editor => new FileInfo(Path.Join(DevKit.FullName, "Engine/Binaries/Win64/UE4Editor.exe")).GetProperCasedFileInfo();

        public FileInfo CookLogFile => new FileInfo(Path.Join(LogFiles.FullName, ModName + ".txt")).GetProperCasedFileInfo();
        public FileInfo ModPakFile => new FileInfo(Path.Join(ModPakFolder.FullName, ModName + ".pak")).GetProperCasedFileInfo();
        public FileInfo ModPakFileBackup => new FileInfo(Path.Join(ModPakFolder.FullName, ModName + ".backup.pak")).GetProperCasedFileInfo();
        public FileInfo ModCookInfo => new FileInfo(Path.Join(ModLocalFolder.FullName, "CookInfo.ini")).GetProperCasedFileInfo();
        public FileInfo ModInfo => new FileInfo(Path.Join(ModFolder.FullName, "modinfo.json")).GetProperCasedFileInfo();
        public string ModName => new DirectoryInfo(Path.Join(ModsFolder.FullName, modName)).GetProperCasedDirectoryInfo().Name;

        public bool IsValidMod => ModInfo.Exists;
        public bool IsValidDevKit => !string.IsNullOrEmpty(config?.DevKitPath) && UE4CMD.Exists;

        private Config config;
        private string modName;

        public const string CMDBinary = "Engine/Binaries/Win64/UE4Editor-Cmd.exe";
        public const string IncludePrefix = "FilesToCook=";
        public const string ExcludePrefix = "UnselectedFiles=";
        public const string ActiveFile = "active.txt";
        public const string CookInfoHeader = "[/CookInfo]";
        public const string AssetExt = "uasset";
        public const string CookLogArg = "-abslog";

        public readonly string[] CookArgs = { "-installed", "-ModDevKit", "-run=cookmod", "targetplatform=WindowsNoEditor", "-iterate", "-compressed", "-stdout", "-unattended", "-fileopenlog" };

        public readonly string[] EditorArgs = { "-ModDevKit", "-Installed" };

        private CommandCode lastError;

        public CommandCode LastError => lastError;

        public KitchenClerk(Config config, string modName = "") 
        {
            this.config = config;
            if(IsValidDevKit)
                this.modName = string.IsNullOrEmpty(modName) ? GetCurrentDirectoryMod() ?? "" : modName;
            else
                this.modName = modName;
        }

        public static bool CreateClerk(string? modName, out KitchenClerk clerk)
        {
            clerk = new KitchenClerk(Config.LoadConfig(), modName ?? "");
            if(!clerk.IsValidDevKit)
            {
                clerk.lastError = new CommandCode { code = CommandCode.DevKitPathInvalid, message = "Dev Kit path is invalid" };
                return false;
            }

            if(!clerk.IsValidMod)
            {
                clerk.lastError = new CommandCode { code = CommandCode.ModNameIsInvalid, message = $"Mod Name {modName} is invalid" };
                return false;
            }
            return true;
        }

        public static bool CreateDevKitClerk(out KitchenClerk clerk)
        {
            clerk = new KitchenClerk(Config.LoadConfig());
            if (!clerk.IsValidDevKit)
            {
                clerk.lastError = new CommandCode { code = CommandCode.DevKitPathInvalid, message = "Dev Kit path is invalid" };
                return false;
            }
            return true;
        }

        public bool DeleteAnyActive()
        {
            if(!ModsFolder.Exists)
            {
                lastError = CommandCode.NotFound(ModsFolder);
                return false;
            }
            foreach(DirectoryInfo dir in ModsFolder.GetDirectories())
            {
                FileInfo file = new FileInfo(Path.Join(dir.FullName, ActiveFile));
                if(file.Exists)
                {
                    file.Delete();
                }
            }
            return true;
        }

        public bool CreateActive(string modName)
        {
            FileInfo file = new FileInfo(Path.Join(ModsFolder.FullName, modName, ActiveFile)).GetProperCasedFileInfo();
            try
            {
                File.WriteAllText(file.FullName, "");
                return true;
            }
            catch (Exception ex)
            {
                lastError = new CommandCode { code = CommandCode.UnknownError, message = ex.Message };
                return false;
            }
        }

        public bool SwitchActive()
        {
            return DeleteAnyActive() && CreateActive(ModName);
        }

        public void GetCookInfo(out List<string> included, out List<string> excluded)
        {
            included = new List<string>();
            excluded = new List<string>();

            if (!ModCookInfo.Exists) return;

            string[] cookinfo = File.ReadAllLines(ModCookInfo.FullName);

            foreach(string line  in cookinfo)
            {
                if (line.StartsWith(IncludePrefix) && !string.IsNullOrEmpty(line.Substring(IncludePrefix.Length)))
                    included.Add(line.Substring(IncludePrefix.Length));
                else if (line.StartsWith(ExcludePrefix) && !string.IsNullOrEmpty(line.Substring(ExcludePrefix.Length)))
                    excluded.Add(line.Substring(ExcludePrefix.Length));
            }
        }

        public bool SetCookInfo(List<string> included, List<string> excluded)
        {
            if(!ModCookInfo.Exists)
            {
                lastError = CommandCode.NotFound(ModCookInfo);
                return false;
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(CookInfoHeader);
            included.Sort();
            excluded.Sort();

            foreach (string line in excluded)
                sb.AppendLine(ExcludePrefix+line);
            foreach( string line in included)
                sb.AppendLine(IncludePrefix+line);

            try
            {
                File.WriteAllText(ModCookInfo.FullName, sb.ToString());
                return true;
            }
            catch (Exception ex)
            {
                lastError = new CommandCode { code = CommandCode.UnknownError, message = ex.Message };
                return false;
            }
        }

        public List<string> RemoveMissingFiles(ref List<string> included, ref List<string> excluded)
        {
            List<string> change = TrimFileNotFound(ref included);
            change.AddRange(TrimFileNotFound(ref excluded));
            return change;
        }

        public List<string>? UpdateIncludedCookInfo(DirectoryInfo directory, ref List<string> included, List<string> excluded)
        {
            if (!directory.Exists)
            {
                lastError = CommandCode.NotFound(directory);
                return null;
            }
            string[] files = Directory.GetFiles(directory.FullName, $"*.{AssetExt}", SearchOption.AllDirectories);
            List<string> added = new List<string>();
            foreach (string file in files)
            {
                FileInfo info = new FileInfo(file);
                if(!included.Contains(info.PosixFullName()) && !excluded.Contains(info.PosixFullName()))
                {
                    included.Add(info.PosixFullName());
                    added.Add(info.PosixFullName());
                }
            }
            return added;
        }

        public List<string> SwapFilesInLists(List<string> files, ref List<string> addTo, ref List<string> removeFrom)
        {
            List<string> added = new List<string>();
            foreach (string file in files)
            {
                FileInfo info = new FileInfo(file);
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

        public List<string> TrimFileNotFound(ref List<string> list)
        {
            List<string> removed = new List<string>();
            foreach (string file in list.ToList())
                if(!new FileInfo(file).Exists)
                {
                    list.Remove(file);
                    removed.Add(file);
                }
            return removed;
        }

        public bool IsGitRepoDirty(DirectoryInfo directory)
        {
            if(Repository.IsValid(directory.FullName))
                using(Repository repo = new Repository(directory.FullName))
                {
                    return repo.RetrieveStatus().IsDirty;
                }
            return false;
        }

        public bool HasDedicatedModsSharedBranch()
        {
            if(Repository.IsValid(ModsShared.FullName))
                using (Repository repo = new Repository(ModsShared.FullName))
                {
                    foreach (Branch branch in repo.Branches)
                    {
                        if(branch.FriendlyName == ModName) return true;
                    }
                }
            return false;
        }

        public bool IsModsSharedBranchValid()
        {
            using(Repository repo = new Repository(ModsShared.FullName))
            {
                return repo.Head.FriendlyName == ModName || (!HasDedicatedModsSharedBranch() && repo.Head.FriendlyName == "master");
            }
        }

        public void CreateModPakBackup()
        {
            if(ModPakFile.Exists)
            {
                if(ModPakFileBackup.Exists) 
                    ModPakFileBackup.Delete();
                ModPakFile.MoveTo(ModPakFileBackup.FullName);
            }
        }

        public void DumpChange(List<string> changes, ConsoleColor foreground, ConsoleColor background = ConsoleColor.Black)
        {
            Console.ForegroundColor = foreground;
            Console.BackgroundColor = background;
            foreach (string change in changes)
            {
                Console.WriteLine(change.Substring(DevKitContent.FullName.Length));
            }
            Console.ResetColor();
        }

        public bool QueryPakFile(FileInfo file, out string output)
        {
            output = "";
            if(!file.Exists)
            {
                lastError = CommandCode.NotFound(file);
                return false;
            }
            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = UnrealPak.FullName;
            p.StartInfo.Arguments = string.Join(" ", new string[]
            {
                    file.FullName,
                    "-List"
                });
            p.Start();
            output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            return true;
        }

        public List<string> ConvertToCookingFolder(List<string> files)
        {
            List<string> result = new List<string>();
            foreach (string file in files)
            {
                string converted = file;
                if (file.StartsWith(ModLocalFolder.PosixFullName()))
                    converted = Path.Join("Mods", ModName, file.RemoveRootFolder(ModLocalFolder)).PosixFullName().RemoveExtension();
                else if (file.StartsWith(ModContentFolder.PosixFullName()))
                    converted = file.RemoveRootFolder(ModContentFolder).PosixFullName().RemoveExtension();
                else if (file.StartsWith(ModsShared.PosixFullName()))
                    converted = Path.Join("ModsShared", file.RemoveRootFolder(ModsShared)).PosixFullName().RemoveExtension();

                result.Add(converted);
            }
            return result;
        }

        public string? GetCurrentDirectoryMod()
        {
            DirectoryInfo current = new DirectoryInfo(Directory.GetCurrentDirectory());
            if(current.FullName.StartsWith(ModsFolder.FullName))
            {
                string local = current.FullName.RemoveRootFolder(ModsFolder.FullName);
                string modName = local.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries)[0];
                if (File.Exists(Path.Join(ModsFolder.FullName, modName, "modinfo.json")))
                    return modName;
            }
            return null;
        }

        public void CleanCookedFolder()
        {
            if (ModCookedFolder.Exists && (ModCookedFolder.GetFiles().Length != 0 || ModCookedFolder.GetDirectories().Length != 0))
            {
                foreach (FileInfo fileInfo in ModCookedFolder.GetFiles())
                    fileInfo.Delete();
                foreach (DirectoryInfo directory in ModCookedFolder.GetDirectories())
                    directory.Delete(true);
            }
        }

        public bool CopyAndFilter(bool verbose)
        {
            GetCookInfo(out List<string> included, out List<string> excluded);
            List<string> cookedFiles = Directory.GetFiles(CookingFolder.FullName, "*", SearchOption.AllDirectories).ToList();
            HashSet<string> validFiles = ConvertToCookingFolder(included).ToHashSet();
            HashSet<string> checkedFiles = new HashSet<string>();

            for (int i = 0; i < cookedFiles.Count; i++)
            {
                string file = cookedFiles[i];
                string localFile = file.RemoveRootFolder(CookingFolder).PosixFullName().RemoveExtension();

                if (!validFiles.Contains(localFile))
                {
                    Tools.WriteColoredLine($"Ignoring: {file}", ConsoleColor.Yellow);
                    continue;
                }
                checkedFiles.Add(localFile);

                FileInfo from = new FileInfo(file);
                FileInfo to = new FileInfo(Path.Join(ModCookedFolder.FullName, localFile) + Path.GetExtension(file));
                if (to.Directory == null)
                {
                    Tools.WriteColoredLine($"Ignoring: {file}", ConsoleColor.Yellow);
                    continue;
                }

                if (verbose)
                    Tools.WriteColoredLine("Copy: " + from.FullName, ConsoleColor.DarkGray);

                Directory.CreateDirectory(to.Directory.FullName);
                if (to.Exists)
                {
                    lastError = new CommandCode { code = CommandCode.UnknownError, message = $"Could not copy the following file, already exists\n{to.FullName}" };
                    return false;
                }
                from.CopyTo(to.FullName, true);
            }

            if (checkedFiles.Count != validFiles.Count)
            {
                foreach (string file in validFiles.Except(checkedFiles))
                    Tools.WriteColoredLine(file, ConsoleColor.Red);
                lastError = new CommandCode { code = CommandCode.UnknownError, message = $"Cooking encountered an error, {checkedFiles.Count} files cooked, but {included.Count} were expected" };
                return false;
            }

            return true;
        }

        public bool CheckoutModsSharedBranch(out string branchName)
        {
            branchName = "master";
            if (!Repository.IsValid(ModsShared.FullName))
                return true;
            if (IsGitRepoDirty(ModsShared))
            {
                lastError = new CommandCode { code = CommandCode.RepositoryIsDirty, message = $"Shared repository is dirty" };
                return false;
            }

            using (Repository repo = new Repository(ModsShared.FullName))
            {
                try
                {
                    Branch? branch = null;
                    foreach (Branch b in repo.Branches)
                        if (b.FriendlyName == ModName)
                            branch = b;

                    if (branch == null)
                        branch = repo.Branches["master"];

                    branchName = branch.FriendlyName;
                    if (repo.Head.CanonicalName == branch.CanonicalName)
                        return true;
                    LibGit2Sharp.Commands.Checkout(repo, branch);
                    return true;
                }
                catch (Exception ex)
                {
                    lastError = new CommandCode { code = CommandCode.UnknownError, message = ex.Message };
                    return false;
                }
            }
        }
    }
}
