using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Tot.Commands
{
    [Verb("pak", HelpText = "Pak the previously cooked files")]
    internal class PakCommand : ModBasedCommand, ICommand
    {
        [Option('c', "compress", HelpText = "Compress the files to reduce the final mod size")]
        public bool Compress { get; set; }

        public CommandCode Execute()
        {
            if(!KitchenClerk.CreateClerk(ModName, out KitchenClerk clerk))
                return clerk.LastError;

            if (!clerk.ModInfo.Exists)
                return CommandCode.NotFound(clerk.ModInfo);
            clerk.CreateModPakBackup();

            foreach (FileInfo file in clerk.ModFolder.GetFiles())
                if (!file.Name.StartsWith(".") && file.Name != "active.txt")
                    file.CopyTo(Path.Join(clerk.ModCookedFolder.FullName, file.Name), true);

            Tools.WriteColoredLine($"Paking {clerk.ModName}..", ConsoleColor.Cyan);
            Process p = Process.Start(
                clerk.UnrealPak.FullName,
                string.Join(" ",
                    clerk.ModPakFile.FullName,
                    "-Create=" + clerk.ModCookedFolder.FullName,
                    Compress ? "-compress" : ""
                ));
            p.WaitForExit();
            return CommandCode.Success($"{clerk.ModName} has been paked successfuly.. !");
        }
    }
}
