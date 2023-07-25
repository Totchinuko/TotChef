using CommandLine.Text;
using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tot.Commands
{
    [Verb("swap", HelpText = "Swap files in the cookinfo.ini")]
    internal class SwapCommand : ModBasedCommand, ICommand
    {
        [Option('f', "filter", HelpText = "Folder filtering, accept * wildcards on file name")]
        public string? filter { get; set; }
        [Option('r', "recursive", HelpText = "Include files from subfolder")]
        public bool recursive { get; set; }
        [Option('e', "exclude", HelpText = "Swap files to the exclude list")]
        public bool exclude { get; set; }

        public CommandCode Execute()
        {
            if (!KitchenClerk.CreateClerk(ModName, out KitchenClerk clerk))
                return clerk.LastError;

            if (filter == null)
                filter = "";

            if (!string.IsNullOrEmpty(filter) && !filter.PosixFullName().StartsWith(clerk.DevKitContent.PosixFullName()))
                filter = Path.Join(clerk.DevKitContent.FullName, filter);

            List<string> from;
            List<string> to;
            if(exclude)
                clerk.GetCookInfo(out from, out to);
            else
                clerk.GetCookInfo(out to, out from);

            List<string> added = new List<string>();

            if (File.Exists(filter))
            {
                added = clerk.SwapFilesInLists(new List<string> { filter.PosixFullName() }, ref to, ref from);
            }
            else if (Directory.Exists(filter))
            {
                DirectoryInfo directory = new DirectoryInfo(filter).GetProperCasedDirectoryInfo();
                List<string> files = Directory.GetFiles(directory.FullName, $"*.{KitchenClerk.AssetExt}", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList();
                added = clerk.SwapFilesInLists(files, ref to, ref from);
            }
            else if (filter.Contains("*"))
            {
                FileInfo fileInfo = new FileInfo(filter);
                if (fileInfo.Directory == null || !fileInfo.Directory.Exists)
                    return CommandCode.NotFound(fileInfo);

                List<string> files = Directory.GetFiles(fileInfo.Directory.FullName, filter.Substring(fileInfo.Directory.FullName.Length + 1), recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList();
                added = clerk.SwapFilesInLists(files, ref to, ref from);
            }
            else
                return CommandCode.NotFound(new DirectoryInfo(filter));

            if (!clerk.SetCookInfo(from, to))
                return clerk.LastError;
            clerk.DumpChange(added, ConsoleColor.Red);
            return CommandCode.Success($"{added.Count} files added to the {(exclude ? "exclude" : "include")} list");
        }
    }
}
