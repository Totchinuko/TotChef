using LibGit2Sharp;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

namespace TotChef
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if(args.Length == 0)
            {
                HelpCMD(args);
                return;
            }

            string methodName = args[0];
            Type type = typeof(Program);
            MethodInfo? info = type.FindMethodCaseInsensitive(methodName+"CMD", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);

            if (info != null)
            {
                info.Invoke(null, new object[] { args.Skip(1).ToArray() });
            } else
            {
                Tools.ExitError($"Unknown command {methodName}, use \"totchef help\" to get the list of commands");
            }

            Environment.Exit(0);
        }

        [Description("Setup the devkit path and save it to config\nsetup C:/DevKit/Path")]
        static void SetupCMD(string[] args)
        {
            Tools.ValidateArgs(args, "Path");
            string path = string.Join(" ", args);
            KitchenClerk clerk = new KitchenClerk(path);
            if (!clerk.Validate(true)) return;

            Config config = new Config() { DevKitPath = clerk.DevKit.PosixFullName() };
            config.SaveConfig();
            Tools.WriteColoredLine("Setup successful", ConsoleColor.Green);
            return;
        }

        [Description("List all available mods")]
        static void ModsCMD(string[] args)
        {
            Config config = Config.LoadConfig();
            KitchenClerk clerk = new KitchenClerk(config.DevKitPath);
            if (!clerk.IsValidDevKit)
            {
                Tools.ExitError("Invalid DevKit Path");
                return;
            }

            Tools.WriteColoredLine("Mod list:", ConsoleColor.Cyan);
            foreach(DirectoryInfo directory in clerk.ModsFolder.GetDirectories())
            {
                Console.WriteLine(directory.Name);
            }
        }

        [Description("Cook a mod\ncook ModName")]
        static void CookCMD(string[] args)
        {
            Tools.ValidateArgs(args, "Mod Name");
            Config config = Config.LoadConfig();
            KitchenClerk clerk = config.MakeClerk(args[0]);
            if (!clerk.Validate()) return;

            bool verbose = args.Contains("-v");

            if (clerk.IsGitRepoDirty(clerk.ModsShared))
                Tools.ExitError("ModsShared repo is dirty");
            if (clerk.IsGitRepoDirty(clerk.ModFolder))
                Tools.ExitError($"Mod {clerk.ModName} repo is dirty");
            if (!clerk.IsModsSharedBranchValid())
                Tools.ExitError("Dedicated ModsShared branch is not checked out");

            clerk.SwitchActive();
            Tools.WriteColoredLine($"Set {clerk.ModName} as active", ConsoleColor.Cyan);

            clerk.GetCookInfo(out List<string> included, out List<string> excluded);
            List<string> change = clerk.UpdateIncludedCookInfo(clerk.ModLocalFolder, ref included, excluded);
            clerk.SetCookInfo(included, excluded);
            if(change.Count > 0)
            {
                Tools.WriteColoredLine($"Added {change.Count} missing local mod files to cooking", ConsoleColor.Cyan);
                clerk.DumpChange(change, ConsoleColor.Yellow);
            }                

            Tools.WriteColoredLine("Cooking...", ConsoleColor.Cyan);
            Stove stove = new Stove(clerk, verbose);
            stove.StartCooking();

            if(!stove.wasSuccess)
                Tools.ExitError($"Cooking failed. {stove.errors} Error(s)");
            Tools.WriteColoredLine($"Cooking is successful", ConsoleColor.Green);


            Tools.WriteColoredLine("Moving and filtering files", ConsoleColor.Cyan);
            if (clerk.ModCookedFolder.Exists && (clerk.ModCookedFolder.GetFiles().Length != 0 || clerk.ModCookedFolder.GetDirectories().Length != 0)) 
            {
                foreach (FileInfo fileInfo in clerk.ModCookedFolder.GetFiles())
                    fileInfo.Delete();
                foreach (DirectoryInfo directory in clerk.ModCookedFolder.GetDirectories())
                    directory.Delete(true);
            }           

            List<string> cookedFiles = Directory.GetFiles(clerk.CookingFolder.FullName, "*", SearchOption.AllDirectories).ToList();
            HashSet<string> validFiles = clerk.ConvertToCookingFolder(included).ToHashSet();
            HashSet<string> checkedFiles = new HashSet<string>();

            for (int i = 0; i < cookedFiles.Count; i++)
            {
                string file = cookedFiles[i];
                string localFile = file.RemoveRootFolder(clerk.CookingFolder).PosixFullName().RemoveExtension();

                if(!validFiles.Contains(localFile))
                {
                    Tools.WriteColoredLine($"Ignoring: {file}", ConsoleColor.Yellow);
                    continue;
                }
                checkedFiles.Add(localFile);

                FileInfo from = new FileInfo(file);
                FileInfo to = new FileInfo(Path.Join(clerk.ModCookedFolder.FullName, localFile));
                if (to.Directory == null) continue;

                if(verbose)
                    Tools.WriteColoredLine("Copy: " + from.FullName, ConsoleColor.DarkGray);
                Directory.CreateDirectory(to.Directory.FullName);
                from.CopyTo(to.FullName, true);
            }

            if (checkedFiles.Count != validFiles.Count)
            {
                foreach(string file in validFiles.Except(checkedFiles))
                    Tools.WriteColoredLine(file, ConsoleColor.Red);
                Tools.ExitError($"Moving and filtering encountered an error {checkedFiles.Count} moved, but {included.Count} expected");
            }
            else
                Tools.WriteColoredLine("Moving and filtering done", ConsoleColor.Green);
        }

        [Description("Clean any files from cookinfo.ini that does not exists anymore\nclean ModName")]
        static void CleanCMD(string[] args)
        {
            Tools.ValidateArgs(args, "Mod Name");
            Config config = Config.LoadConfig();
            KitchenClerk clerk = config.MakeClerk(args[0]);
            if (!clerk.Validate()) return;

            clerk.GetCookInfo(out List<string> included, out List<string> excluded);

            List<string> change = clerk.TrimFileNotFound(ref included);
            Tools.WriteColoredLine($"Removing {change.Count} missing included files", ConsoleColor.Cyan);
            clerk.DumpChange(change, ConsoleColor.Magenta);
            Console.WriteLine();

            change = clerk.TrimFileNotFound(ref excluded);
            Tools.WriteColoredLine($"Removing {change.Count} missing excluded files", ConsoleColor.Cyan);
            clerk.DumpChange(change, ConsoleColor.Magenta);
            Console.WriteLine();
            clerk.SetCookInfo(included, excluded);
        }

        [Description("Interactive update of the cookinfo a given mod\nupdate ModName")]
        static void UpdateCMD(string[] args)
        {
            Tools.ValidateArgs(args, "Mod Name");
            Config config = Config.LoadConfig();
            KitchenClerk clerk = config.MakeClerk(args[0]);
            if (!clerk.Validate()) return;

            CleanCMD(args);

            clerk.GetCookInfo(out List<string> included, out List<string> excluded);

            List<string> change = clerk.UpdateIncludedCookInfo(clerk.ModLocalFolder, ref included, excluded);
            Tools.WriteColoredLine($"Adding {change.Count} new mod local files", ConsoleColor.Cyan);
            clerk.DumpChange(change, ConsoleColor.Green);
            Console.WriteLine();

            Console.Write("Update Content folder ? (E)xclude, (I)nclude, (S)kip>");
            string answer = Console.ReadLine() ?? "";

            switch (answer.ToLower())
            {
                case "e":
                    change = clerk.UpdateExcludedCookInfo(clerk.ModContentFolder, included, ref excluded);
                    Tools.WriteColoredLine($"Adding {change.Count} mod content files to the excluded files", ConsoleColor.Cyan);
                    clerk.DumpChange(change, ConsoleColor.Red);
                    Console.WriteLine();
                    break;
                case "i":
                    change = clerk.UpdateIncludedCookInfo(clerk.ModContentFolder, ref included, excluded);
                    Tools.WriteColoredLine($"Adding {change.Count} mod content files to the included files", ConsoleColor.Cyan);
                    clerk.DumpChange(change, ConsoleColor.Green);
                    Console.WriteLine();
                    break;
            }

            Console.Write("Update ModsShared folder ? (E)xclude, (I)nclude, (S)kip>");
            answer = Console.ReadLine() ?? "";

            switch (answer.ToLower())
            {
                case "e":
                    change = clerk.UpdateExcludedCookInfo(clerk.ModsShared, included, ref excluded);
                    Tools.WriteColoredLine($"Adding {change.Count} Mod Shared files to the excluded files", ConsoleColor.Cyan);
                    clerk.DumpChange(change, ConsoleColor.Red);
                    Console.WriteLine();
                    break;
                case "i":
                    change = clerk.UpdateIncludedCookInfo(clerk.ModsShared, ref included, excluded);
                    Tools.WriteColoredLine($"Adding {change.Count} Mod Shared files to the included files", ConsoleColor.Cyan);
                    clerk.DumpChange(change, ConsoleColor.Green);
                    Console.WriteLine();
                    break;
            }

            clerk.SetCookInfo(included, excluded);
            Tools.WriteColoredLine($"{clerk.ModCookInfo.PosixFullName()} updated", ConsoleColor.Cyan);
        }

        [Description("Pak the cooked files of a mod\npak ModName [Options]\n-c Compress pak file")]
        static void PakCMD(string[] args)
        {
            Tools.ValidateArgs(args, "Mod Name");
            Config config = Config.LoadConfig();
            KitchenClerk clerk = config.MakeClerk(args[0]);
            if (!clerk.Validate()) return;

            clerk.ModInfo.Check();
            clerk.CreateModPakBackup();

            foreach(FileInfo file in clerk.ModFolder.GetFiles())
                if(!file.Name.StartsWith(".") && file.Name != "active.txt")
                    file.CopyTo(Path.Join(clerk.ModCookedFolder.FullName, file.Name), true);


            Tools.WriteColoredLine("Starting pak process", ConsoleColor.Cyan);
            Process p = Process.Start(
                clerk.UnrealPak.FullName, 
                string.Join(" ", 
                    clerk.ModPakFile.FullName,
                    "-Create=" + clerk.ModCookedFolder.FullName,
                    args.Contains("-c") ? "-compress" : ""
                ));
            p.WaitForExit();
            Tools.WriteColoredLine("Pak process is over", ConsoleColor.Green);
        }

        [Description("Open the folder containing the mod paks\nopen ModName")]
        static void OpenCMD(string[] args)
        {
            Tools.ValidateArgs(args, "Mod Name");
            Config config = Config.LoadConfig();
            KitchenClerk clerk = config.MakeClerk(args[0]);
            if (!clerk.Validate()) return;

            Process.Start("explorer.exe", clerk.ModPakFolder.FullName);
        }

        [Description("Swap the active.txt file to the given mod\nactivate ModName")]
        static void ActivateCMD(string[] args)
        {
            Tools.ValidateArgs(args, "Mod Name");
            Config config = Config.LoadConfig();
            KitchenClerk clerk = config.MakeClerk(args[0]);
            if (!clerk.Validate()) return;

            clerk.SwitchActive();
            Tools.WriteColoredLine($"Set {clerk.ModName} as active", ConsoleColor.Cyan);
        }

        [Description("Open the devkit for the given mod\ndevkit ModName")]
        static void DevKitCMD(string[] args)
        {
            Tools.ValidateArgs(args, "Mod Name");
            Config config = Config.LoadConfig();
            KitchenClerk clerk = config.MakeClerk(args[0]);
            if (!clerk.Validate()) return;

            clerk.SwitchActive();
            Tools.WriteColoredLine($"Set {clerk.ModName} as active", ConsoleColor.Cyan);

            Process.Start(clerk.UE4Editor.FullName, string.Join(" ", clerk.UProject.FullName, string.Join(" ", clerk.EditorArgs)));
        }

        [Description("Exclude files from cooking\nexclude ModName [/Path/Mask/*.*] [Options]\n-r Recursive dir search")]
        static void ExcludeCMD(string[] args)
        {
            Tools.ValidateArgs(args, "Mod Name", "Folder");
            Config config = Config.LoadConfig();
            KitchenClerk clerk = config.MakeClerk(args[0]);
            if (!clerk.Validate()) return;

            string path = args[1];
            if (!path.PosixFullName().StartsWith(clerk.DevKitContent.PosixFullName()))
                path = Path.Join(clerk.DevKitContent.FullName, path);

            clerk.GetCookInfo(out List<string> included, out List<string> excluded);
            List<string> added = new List<string>();

            if (File.Exists(path))
            {
                added = clerk.SwapFilesInLists(new List<string> { path.PosixFullName() }, ref excluded, ref included);
            }
            else if (Directory.Exists(path))
            {
                DirectoryInfo directory = new DirectoryInfo(path).GetProperCasedDirectoryInfo();
                List<string> files = Directory.GetFiles(directory.FullName, $"*.{clerk.AssetExt}", args.Contains("-r") ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList();
                added = clerk.SwapFilesInLists(files, ref excluded, ref included);
            }
            else if (path.Contains("*"))
            {
                FileInfo fileInfo = new FileInfo(path);
                if (fileInfo.Directory == null || !fileInfo.Directory.Exists)
                {
                    Tools.ExitError($"FileNotFound: {fileInfo.FullName}");
                    return;
                }
                List<string> files = Directory.GetFiles(fileInfo.Directory.FullName, path.Substring(fileInfo.Directory.FullName.Length+1), args.Contains("-r") ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList();
                added = clerk.SwapFilesInLists(files, ref excluded, ref included);
            }
            else
                Tools.ExitError($"FileNotFound: {path}");

            clerk.SetCookInfo(included, excluded);
            clerk.DumpChange(added, ConsoleColor.Red);
            Tools.WriteColoredLine(added.Count + " files added to the exclude list", ConsoleColor.Green);
        }

        [Description("Include files for cooking\ninclude ModName [/Path/Mask/*.*] [Options]\n-r Recursive dir search")]
        static void IncludeCMD(string[] args)
        {
            Tools.ValidateArgs(args, "Mod Name", "Folder");
            Config config = Config.LoadConfig();
            KitchenClerk clerk = config.MakeClerk(args[0]);
            if (!clerk.Validate()) return;

            string path = args[1];
            if (!path.PosixFullName().StartsWith(clerk.DevKitContent.PosixFullName()))
                path = Path.Join(clerk.DevKitContent.FullName, path);

            clerk.GetCookInfo(out List<string> included, out List<string> excluded);
            List<string> added = new List<string>();

            if (File.Exists(path))
            {
                added = clerk.SwapFilesInLists(new List<string> { path.PosixFullName() }, ref included, ref excluded);
            }
            else if(Directory.Exists(path))
            {
                DirectoryInfo directory = new DirectoryInfo(path).GetProperCasedDirectoryInfo();
                List<string> files = Directory.GetFiles(directory.FullName, $"*.{clerk.AssetExt}", args.Contains("-r") ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList();
                added = clerk.SwapFilesInLists(files, ref included, ref excluded);
            }
            else if (path.Contains("*"))
            {
                FileInfo fileInfo = new FileInfo(path);
                if (fileInfo.Directory == null || !fileInfo.Directory.Exists)
                {
                    Tools.ExitError($"FileNotFound: {fileInfo.FullName}");
                    return;
                }
                List<string> files = Directory.GetFiles(fileInfo.Directory.FullName, path.Substring(fileInfo.Directory.FullName.Length+1), args.Contains("-r") ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList();
                added = clerk.SwapFilesInLists(files, ref included, ref excluded);
            }
            else
                Tools.ExitError($"FileNotFound: {path}");

            clerk.SetCookInfo(included, excluded);
            clerk.DumpChange(added, ConsoleColor.Green);
            Tools.WriteColoredLine(added.Count + " files added to the include list", ConsoleColor.Cyan);
        }

        [Description("List the cookinfo.ini\nlist ModName")]
        static void ListCMD(string[] args)
        {
            Tools.ValidateArgs(args, "Mod Name");
            Config config = Config.LoadConfig();
            KitchenClerk clerk = config.MakeClerk(args[0]);
            if (!clerk.Validate()) return;

            clerk.GetCookInfo(out List<string> included, out List<string> excluded);
            Console.ForegroundColor = ConsoleColor.Green;
            foreach(string file in included)
                Console.WriteLine(file);
            Console.ForegroundColor = ConsoleColor.Red;
            foreach (string file in excluded)
                Console.WriteLine(file);
            Console.ResetColor();
        }

        [Description("List the folders of the mod, displaying files status\nstatus ModName [/Folder/Filtering]")]
        static void StatusCMD(string[] args)
        {
            Tools.ValidateArgs(args, "Mod Name");
            Config config = Config.LoadConfig();
            KitchenClerk clerk = config.MakeClerk(args[0]);
            if (!clerk.Validate()) return;

            string filter = "";
            if(args.Length > 1)
            {
                if (!args[1].PosixFullName().StartsWith(clerk.DevKitContent.PosixFullName()))
                    args[1] = Path.Join(clerk.DevKitContent.FullName, args[1]);
                if(Directory.Exists(args[1]))
                    filter = new DirectoryInfo(args[1]).GetProperCasedDirectoryInfo().PosixFullName();
            }

            clerk.GetCookInfo(out List<string> included, out List<string> excluded);
            Dictionary<string, List<string>> includedDir = new Dictionary<string, List<string>>();
            Dictionary<string, List<string>> excludedDir = new Dictionary<string, List<string>>();
            Dictionary<string, List<string>> absentDir = new Dictionary<string, List<string>>();
            Dictionary<string, List<string>> notFounDir = new Dictionary<string, List<string>>();
            List<string> directories = new List<string>();

            foreach(string file in included)
            {
                if (!string.IsNullOrEmpty(filter) && !file.StartsWith(filter))
                    continue;

                FileInfo info = new FileInfo(file);
                if (info.Directory == null) continue;

                if(!directories.Contains(info.Directory.FullName))
                    directories.Add(info.Directory.FullName);
                if(includedDir.ContainsKey(info.Directory.FullName))
                    includedDir[info.Directory.FullName].Add(file);
                else includedDir.Add(info.Directory.FullName, new List<string>{ file });

                if (!info.Exists)
                    if (notFounDir.ContainsKey(info.Directory.FullName))
                        notFounDir[info.Directory.FullName].Add(file);
                    else notFounDir.Add(info.Directory.FullName, new List<string>{ file });
            }

            foreach(string file in excluded)
            {
                if (!string.IsNullOrEmpty(filter) && !file.StartsWith(filter))
                    continue;

                FileInfo info = new FileInfo(file);
                if (info.Directory == null) continue;

                if (!directories.Contains(info.Directory.FullName))
                    directories.Add(info.Directory.FullName);
                if (excludedDir.ContainsKey(info.Directory.FullName))
                    excludedDir[info.Directory.FullName].Add(file);
                else excludedDir.Add(info.Directory.FullName, new List<string> { file });

                if(!info.Exists)
                    if(notFounDir.ContainsKey(info.Directory.FullName))
                        notFounDir[info.Directory.FullName].Add(file);
                    else notFounDir.Add(info.Directory.FullName, new List<string> { file });
            }

            List<string> files = Directory.GetFiles(clerk.ModFolder.FullName, $"*.{clerk.AssetExt}", SearchOption.AllDirectories).ToList();
            files.AddRange(Directory.GetFiles(clerk.ModsShared.FullName, $"*.{clerk.AssetExt}", SearchOption.AllDirectories));
            foreach (string file in files)
            {
                if (!string.IsNullOrEmpty(filter) && !file.PosixFullName().StartsWith(filter))
                    continue;

                FileInfo info = new FileInfo(file);
                if (info.Directory == null) continue;

                if (included.Contains(info.PosixFullName()) || excluded.Contains(info.PosixFullName())) continue;
                if (!directories.Contains(info.Directory.FullName))
                    directories.Add(info.Directory.FullName);
                if (absentDir.ContainsKey(info.Directory.FullName))
                    absentDir[info.Directory.FullName].Add(file);
                else absentDir.Add(info.Directory.FullName, new List<string>{ file });
            }

            directories.Sort();
            foreach(string  dir in directories)
            {
                string file = dir.Substring(clerk.DevKitContent.FullName.Length);
                if(dir.StartsWith(clerk.ModsShared.FullName))
                    Tools.WriteColored(file.PosixFullName() + " ", ConsoleColor.Cyan);
                else if(dir.StartsWith(clerk.ModLocalFolder.FullName))
                    Tools.WriteColored(file.PosixFullName() + " ", ConsoleColor.White);
                else if(dir.StartsWith(clerk.ModContentFolder.FullName))
                    Tools.WriteColored(file.PosixFullName() + " ", ConsoleColor.Blue);
                else
                    Tools.WriteColored(file.PosixFullName() + " ", ConsoleColor.White);

                if (includedDir.ContainsKey(dir))
                    Tools.WriteColored($"(+{includedDir[dir].Count})", ConsoleColor.Green);
                if (excludedDir.ContainsKey(dir))
                    Tools.WriteColored($"(-{excludedDir[dir].Count})", ConsoleColor.Red);
                if (absentDir.ContainsKey(dir))
                    Tools.WriteColored($"(!{absentDir[dir].Count})", ConsoleColor.Yellow);
                if (notFounDir.ContainsKey(dir))
                    Tools.WriteColored($"(?{notFounDir[dir].Count})", ConsoleColor.Magenta);
                Console.Write("\n");

                if(!string.IsNullOrEmpty(filter))
                {
                    if (includedDir.ContainsKey(dir))
                        clerk.DumpChange(includedDir[dir], ConsoleColor.Green);
                    if (excludedDir.ContainsKey(dir))
                        clerk.DumpChange(excludedDir[dir], ConsoleColor.Red);
                    if (absentDir.ContainsKey(dir))
                        clerk.DumpChange(absentDir[dir], ConsoleColor.Yellow);
                    if (notFounDir.ContainsKey(dir))
                        clerk.DumpChange(notFounDir[dir], ConsoleColor.Magenta);
                }
            }
        }

        [Description("List files from a the last cooked pak\nlistpak ModList")]
        static void ListPakCMD(string[] args)
        {
            Tools.ValidateArgs(args, "Mod Name");
            Config config = Config.LoadConfig();
            KitchenClerk clerk = config.MakeClerk(args[0]);
            if (!clerk.Validate()) return;

            PakListing listing = new PakListing(clerk.QueryPakFile(clerk.ModPakFile), clerk.ModPakFile.Name);
            listing.pakedFiles.ForEach(x => Console.WriteLine(x));            
            Tools.WriteColoredLine(listing.rapport, ConsoleColor.Cyan);
        }

        [Description("Compare the pak files inside a modlist and evaluate the conflicts\nconflict /path/to/modlist.txt")]
        static void ConflictCMD(string[] args)
        {
            Tools.ValidateArgs(args, "Mod List");
            Config config = Config.LoadConfig();
            KitchenClerk clerk = new KitchenClerk(config.DevKitPath);
            if (!clerk.Validate(true)) return;

            FileInfo modlistFile = new FileInfo(args[0]).GetProperCasedFileInfo();
            List<string> modlist = File.ReadAllLines(modlistFile.FullName).ToList();

            List<PakListing> listings = new List<PakListing>();
            foreach(string line  in modlist)
            {
                string path = line;
                if(path.StartsWith("*"))
                    path = path.Substring(1);
                FileInfo mod = new FileInfo(path);
                if(mod.Exists)
                {
                    Tools.WriteColoredLine($"Parsing {mod.FullName}...", ConsoleColor.Cyan);
                    listings.Add(new PakListing(clerk.QueryPakFile(mod), mod.Name));
                }                    
            }

            Dictionary<string, List<PakedFile>> folded = new Dictionary<string, List<PakedFile>>();
            foreach(PakListing listing in listings)
            {
                foreach(PakedFile pakedFile in listing.pakedFiles)
                {
                    if (folded.ContainsKey(pakedFile.path))
                        folded[pakedFile.path].Add(pakedFile);
                    else folded.Add(pakedFile.path, new List<PakedFile> { pakedFile});
                }                
            }

            foreach(KeyValuePair<string, List<PakedFile>> keyValuePair in folded)
            {
                if(!keyValuePair.Key.Contains("/"))
                    continue;

                if(keyValuePair.Value.Count > 1 && !keyValuePair.Value.AreShaIdentical())
                {
                    Console.WriteLine(keyValuePair.Value[0].path);
                    foreach (PakedFile pakedFile in keyValuePair.Value)
                        Tools.WriteColoredLine($"{pakedFile.sha} - {pakedFile.pakName} ({pakedFile.size} bytes)", ConsoleColor.DarkGray);
                }
            }
        }

        [Description("Return the path of a given mod folder\npathto ModName")]
        static void PathToCMD(string[] args)
        {
            Tools.ValidateArgs(args, "Mod Name");
            Config config = Config.LoadConfig();
            KitchenClerk clerk = config.MakeClerk(args[0]);
            if (!clerk.Validate()) return;

            Console.Write(clerk.ModFolder.PosixFullName());
        }

        [Description("Checkout the branch corresponding the mod name on the ModsShared repository or checkout master\ncheckout ModName")]
        static void CheckoutCMD(string[] args)
        {
            Tools.ValidateArgs(args, "Mod Name");
            Config config = Config.LoadConfig();
            KitchenClerk clerk = config.MakeClerk(args[0]);
            if (!clerk.Validate()) return;

            if (!Repository.IsValid(clerk.ModsShared.FullName)) Environment.Exit(0);
            if (clerk.IsGitRepoDirty(clerk.ModsShared)) Tools.ExitError("ModsShared repository is dirty");

            using (Repository repo = new Repository(clerk.ModsShared.FullName))
            {
                Branch? branch = null;
                foreach (Branch b in repo.Branches)
                    if (b.FriendlyName == clerk.ModName)
                        branch = b;

                if (branch == null)
                    branch = repo.Branches["master"];

                if (repo.Head.CanonicalName == branch.CanonicalName) Environment.Exit(0);
                Commands.Checkout(repo, branch);
                Tools.WriteColoredLine($"ModsShared repository is now on {branch.FriendlyName}", ConsoleColor.Cyan);
                Environment.Exit(0);
            }
        }

        [Description("Validate a git repository and see if it is ready for cooking\nvalidategit ModName")]
        static void ValidateCMD(string[] args)
        {
            Tools.ValidateArgs(args, "Mod Name");
            Config config = Config.LoadConfig();
            KitchenClerk clerk = config.MakeClerk(args[0]);
            if (!clerk.Validate()) return;

            if (clerk.IsGitRepoDirty(clerk.ModsShared))
                Tools.ExitError("ModsShared repo is dirty");
            if (clerk.IsGitRepoDirty(clerk.ModFolder))
                Tools.ExitError($"Mod {clerk.ModName} repo is dirty");
        }

        static void HelpCMD(string[] args)
        {
            Tools.WriteColoredLine("Tot!Chef 1.0.0", ConsoleColor.Cyan);
            Tools.WriteColoredLine("==============", ConsoleColor.Cyan);
            Type type = typeof(Program);
            foreach (MethodInfo method in type.GetMethods(BindingFlags.NonPublic | BindingFlags.Static))
            {
                if (method.Name.EndsWith("CMD"))
                {
                    DescriptionAttribute? description = method.GetCustomAttribute<DescriptionAttribute>();
                    Tools.WriteColoredLine($"totchef {method.Name[..^3].ToLower()}", ConsoleColor.White);
                    if (description != null)
                        Tools.WriteColoredLine(description.Description, ConsoleColor.DarkGray);
                }
            }
            return;
        }
    }
}