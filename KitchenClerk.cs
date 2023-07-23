using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace TotChef
{
    public class KitchenClerk
    {
        public DirectoryInfo TempFolder => new DirectoryInfo(Path.Join(Path.GetTempPath(), "../ConanSandbox")).GetProperCasedDirectoryInfo();
        public DirectoryInfo DevKit => new DirectoryInfo(devKitPath).GetProperCasedDirectoryInfo();
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

        public FileInfo UE4CMD => new FileInfo(Path.Join(DevKit.FullName, "Engine/Binaries/Win64/UE4Editor-Cmd.exe")).GetProperCasedFileInfo();
        public FileInfo UProject => new FileInfo(Path.Join(DevKit.FullName, "Games/ConanSandbox/ConanSandbox.uproject")).GetProperCasedFileInfo();
        public FileInfo UnrealPak => new FileInfo(Path.Join(DevKit.FullName, "Engine/Binaries/Win64/UnrealPak.exe")).GetProperCasedFileInfo();
        public FileInfo UE4Editor => new FileInfo(Path.Join(DevKit.FullName, "Engine/Binaries/Win64/UE4Editor.exe")).GetProperCasedFileInfo();

        public FileInfo CookLogFile => new FileInfo(Path.Join(LogFiles.FullName, ModName + ".txt")).GetProperCasedFileInfo();
        public FileInfo ModPakFile => new FileInfo(Path.Join(ModPakFolder.FullName, ModName + ".pak")).GetProperCasedFileInfo();
        public FileInfo ModPakFileBackup => new FileInfo(Path.Join(ModPakFolder.FullName, ModName + ".backup.pak")).GetProperCasedFileInfo();
        public FileInfo ModCookInfo => new FileInfo(Path.Join(ModLocalFolder.FullName, "CookInfo.ini")).GetProperCasedFileInfo();
        public FileInfo ModInfo => new FileInfo(Path.Join(ModFolder.FullName, "modinfo.json")).GetProperCasedFileInfo();

        public bool IsValidMod => ModFolder.Exists;

        public string ModName => new DirectoryInfo(Path.Join(ModsFolder.FullName, modName)).GetProperCasedDirectoryInfo().Name;

        public bool IsValidDevKit => UE4CMD.Exists;

        private string devKitPath;
        private string modName;

        public readonly string IncludePrefix = "FilesToCook=";
        public readonly string ExcludePrefix = "UnselectedFiles=";
        public readonly string ActiveFile = "active.txt";
        public readonly string CookInfoHeader = "[/CookInfo]";
        public readonly string AssetExt = "uasset";
        public readonly string CookLogArg = "-abslog";

        public readonly string[] CookArgs = { "-installed", "-ModDevKit", "-run=cookmod", "targetplatform=WindowsNoEditor", "-iterate", "-compressed", "-stdout", "-unattended", "-fileopenlog" };

        public readonly string[] EditorArgs = { "-ModDevKit", "-Installed" };


        public KitchenClerk(string devKitPath, string modName = "") 
        {
            if (string.IsNullOrEmpty(devKitPath))
                Tools.ExitError("DevKit path is Invalid, use totchef setup to configure it");
            this.devKitPath = devKitPath;
            this.modName = modName;
        }

        public bool Validate(bool onlyDevKit = false)
        {
            if(!IsValidDevKit)
            {
                Tools.WriteColoredLine("Invalid DevKit path, please setup again", ConsoleColor.Red);
                return false;
            }

            if(!IsValidMod && !onlyDevKit)
            {
                Tools.WriteColoredLine("Invalid mod name", ConsoleColor.Red);
                return false;
            }
            return true;
        }

        public void DeleteAnyActive()
        {
            ModsFolder.Check();
            foreach(DirectoryInfo dir in ModsFolder.GetDirectories())
            {
                FileInfo file = new FileInfo(Path.Join(dir.FullName, ActiveFile));
                if(file.Exists)
                {
                    file.Delete();
                }
            }
        }

        public void CreateActive(string modName)
        {
            FileInfo file = new FileInfo(Path.Join(ModsFolder.FullName, modName, ActiveFile)).GetProperCasedFileInfo();
            File.WriteAllText(file.FullName, "");
        }

        public void SwitchActive()
        {
            DeleteAnyActive();
            CreateActive(ModName);
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

        public void SetCookInfo(List<string> included, List<string> excluded)
        {
            ModCookInfo.Check();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(CookInfoHeader);
            included.Sort();
            excluded.Sort();

            foreach (string line in excluded)
                sb.AppendLine(ExcludePrefix+line);
            foreach( string line in included)
                sb.AppendLine(IncludePrefix+line);
            File.WriteAllText(ModCookInfo.FullName, sb.ToString());
        }

        public List<string> UpdateIncludedCookInfo(DirectoryInfo directory, ref List<string> included, List<string> excluded)
        {
            directory.Check();
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

        public List<string> UpdateExcludedCookInfo(DirectoryInfo directory, List<string> included, ref List<string> excluded)
        {
            directory.Check();
            string[] files = Directory.GetFiles(directory.FullName, $"*.{AssetExt}", SearchOption.AllDirectories);
            List<string> added = new List<string>();
            foreach (string file in files)
            {
                FileInfo info = new FileInfo(file);
                if (!included.Contains(info.PosixFullName()) && !excluded.Contains(info.PosixFullName()))
                {
                    excluded.Add(info.PosixFullName());
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

        public void CleanKitchen()
        {
            CookingFolder.Check();
            if (CookingFolder.GetFiles().Length == 0 && CookingFolder.GetDirectories().Length == 0) return;

            foreach (FileInfo fileInfo in CookingFolder.GetFiles())
                fileInfo.Delete();
            foreach (DirectoryInfo directory in CookingFolder.GetDirectories())
                directory.Delete(true);
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
                return repo.Head.FriendlyName == ModName || !HasDedicatedModsSharedBranch();
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

        public string QueryPakFile(FileInfo file)
        {
            file.Check();
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
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            return output;
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
    }
}
