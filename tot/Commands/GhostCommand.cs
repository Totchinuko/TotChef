using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tot.Commands
{
    [Verb("ghost", HelpText = "List invalid files and clean them")]
    internal class GhostCommand : ModBasedCommand, ICommand
    {
        [Option('c', "clean", Required = false, HelpText = "Clean invalid files")]
        public bool Clean { get; set; }

        public CommandCode Execute()
        {
            if (!KitchenClerk.CreateClerk(ModName, out KitchenClerk clerk))
                return clerk.LastError;

            DirectoryInfo modDir = clerk.ModFolder;
            List<string> files = Directory.GetFiles(modDir.FullName, "*.*", SearchOption.AllDirectories).ToList();

            Tools.WriteColoredLine("Invalid files:", ConsoleColor.Cyan);
            foreach (string file in files)
            {
                FileInfo info = new FileInfo(file);
                if (info.Extension != ".uasset" && info.Extension != ".umap") continue;
                if (info.Length >= 4 * 1024) continue;
                if (!clerk.FileContain(info, "ObjectRedirector") && info.Length >= 1024) continue;
                Tools.WriteColoredLine($"{file}", ConsoleColor.Red);
                if (Clean)
                    info.Delete();
            }

            return CommandCode.Success();
        }
    }
}