using CommandLine.Text;
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
        [Option('e', "exclude", HelpText = "Swap files to the exclude list")]
        public bool exclude { get; set; }

        [Option('f', "filter", HelpText = "Folder filtering, accept * wildcards on file name")]
        public string? filter { get; set; }

        [Option('r', "recursive", HelpText = "Include files from subfolder")]
        public bool recursive { get; set; }

        public CommandCode Execute()
        {
            if (!KitchenClerk.CreateClerk(ModName, out KitchenClerk clerk))
                return clerk.LastError;

            if (filter == null)
                filter = "";

            if (!string.IsNullOrEmpty(filter) && !filter.PosixFullName().StartsWith(clerk.DevKitContent.PosixFullName()))
                filter = Path.Join(clerk.DevKitContent.FullName, filter);

            List<string> included;
            List<string> excluded;
            clerk.GetCookInfo(out included, out excluded);

            List<string> added = new List<string>();

            if (File.Exists(filter))
            {
                if (exclude)
                    added = clerk.SwapFilesInLists(new List<string> { filter.PosixFullName() }, ref excluded, ref included);
                else
                    added = clerk.SwapFilesInLists(new List<string> { filter.PosixFullName() }, ref included, ref excluded);
            }
            else if (Directory.Exists(filter))
            {
                DirectoryInfo directory = new DirectoryInfo(filter).GetProperCasedDirectoryInfo();
                List<string> files = Directory.GetFiles(directory.FullName, $"*.{KitchenClerk.AssetExt}", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList();

                if (exclude)
                    added = clerk.SwapFilesInLists(files, ref excluded, ref included);
                else
                    added = clerk.SwapFilesInLists(files, ref included, ref excluded);
            }
            else if (filter.Contains("*"))
            {
                FileInfo fileInfo = new FileInfo(filter);
                if (fileInfo.Directory == null || !fileInfo.Directory.Exists)
                    return CommandCode.NotFound(fileInfo);

                List<string> files = Directory.GetFiles(fileInfo.Directory.FullName, filter.Substring(fileInfo.Directory.FullName.Length + 1), recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList();
                if (exclude)
                    added = clerk.SwapFilesInLists(files, ref excluded, ref included);
                else
                    added = clerk.SwapFilesInLists(files, ref included, ref excluded);
            }
            else
                return CommandCode.NotFound(new DirectoryInfo(filter));

            if (!clerk.SetCookInfo(included, excluded))
                return clerk.LastError;
            clerk.DumpChange(added, exclude ? ConsoleColor.Red : ConsoleColor.Green);
            return CommandCode.Success($"{added.Count} files added to the {(exclude ? "exclude" : "include")} list");
        }
    }
}