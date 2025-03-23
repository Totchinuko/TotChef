using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace Tot.Commands
{
    [Verb("status", HelpText = "List the content of the cookinfo.ini with file status")]
    internal class StatusCommand : ModBasedCommand, ICommand
    {
        [Option('r', "raw", HelpText = "Display the raw list of the cookinfo.ini")]
        public bool raw { get; set; }

        [Option('f', "folder", HelpText = "Get details on a specific folder")]
        public string? filter { get; set; }


        public CommandCode Execute()
        {
            if (!KitchenClerk.CreateClerk(ModName, out KitchenClerk clerk))
                return clerk.LastError;

            if (raw)
                return ExecuteRawList(clerk);
            return ExecuteFriendly(clerk);
        }

        private CommandCode ExecuteFriendly(KitchenClerk clerk)
        {
            if (!string.IsNullOrEmpty(filter))
            {
                if (!filter.PosixFullName().StartsWith(clerk.DevKitContent.PosixFullName()))
                    filter = Path.Join(clerk.DevKitContent.FullName, filter);
                if (Directory.Exists(filter))
                    filter = new DirectoryInfo(filter).GetProperCasedDirectoryInfo().PosixFullName();
                else
                    filter = null;
            }

            clerk.GetCookInfo(out List<string> included, out List<string> excluded);
            Dictionary<string, List<string>> includedDir = new Dictionary<string, List<string>>();
            Dictionary<string, List<string>> excludedDir = new Dictionary<string, List<string>>();
            Dictionary<string, List<string>> absentDir = new Dictionary<string, List<string>>();
            Dictionary<string, List<string>> notFounDir = new Dictionary<string, List<string>>();
            List<string> directories = new List<string>();

            foreach (string file in included)
            {
                if (!string.IsNullOrEmpty(filter) && !file.StartsWith(filter))
                    continue;

                FileInfo info = new FileInfo(file);
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

            foreach (string file in excluded)
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

                if (!info.Exists)
                    if (notFounDir.ContainsKey(info.Directory.FullName))
                        notFounDir[info.Directory.FullName].Add(file);
                    else notFounDir.Add(info.Directory.FullName, new List<string> { file });
            }

            List<string> files = Directory.GetFiles(clerk.ModFolder.FullName, $"*.{KitchenClerk.AssetExt}", SearchOption.AllDirectories).ToList();
            files.AddRange(Directory.GetFiles(clerk.ModsShared.FullName, $"*.{KitchenClerk.AssetExt}", SearchOption.AllDirectories));
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
                else absentDir.Add(info.Directory.FullName, new List<string> { file });
            }

            directories.Sort();
            foreach (string dir in directories)
            {
                string file = dir.Substring(clerk.DevKitContent.FullName.Length);
                if (dir.StartsWith(clerk.ModsShared.FullName))
                    Tools.WriteColored(file.PosixFullName() + " ", ConsoleColor.Cyan);
                else if (dir.StartsWith(clerk.ModLocalFolder.FullName))
                    Tools.WriteColored(file.PosixFullName() + " ", ConsoleColor.White);
                else if (dir.StartsWith(clerk.ModContentFolder.FullName))
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

                if (!string.IsNullOrEmpty(filter))
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
            return CommandCode.Success();
        }

        private CommandCode ExecuteRawList(KitchenClerk clerk) 
        {
            clerk.GetCookInfo(out List<string> included, out List<string> excluded);
            Console.ForegroundColor = ConsoleColor.Green;
            foreach (string file in included)
                Console.WriteLine(file);
            Console.ForegroundColor = ConsoleColor.Red;
            foreach (string file in excluded)
                Console.WriteLine(file);
            Console.ResetColor();
            return CommandCode.Success();
        }
    }
}
