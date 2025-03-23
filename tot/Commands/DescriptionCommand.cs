using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tot.Commands
{
    [Verb("description", HelpText = "Edit the mod description")]
    internal class DescriptionCommand : ModBasedCommand, ICommand
    {

        public CommandCode Execute()
        {
            if (!KitchenClerk.CreateClerk(ModName, out KitchenClerk clerk))
                return clerk.LastError;

            if (!clerk.GetModInfos(out ModinfoData modinfo))
                return clerk.LastError;

            if (!clerk.GetTemporaryFile(clerk.ModName + ".txt", out string path))
                return clerk.LastError;
            
            if (!File.Exists(path))
                return CommandCode.Error("File not found: " + path);
            string description = modinfo.Description;
            File.WriteAllText(path, description);

            using (Process fileOpener = new Process())
            {
                fileOpener.StartInfo.FileName = "nano";
                fileOpener.StartInfo.Arguments = $"\"{path}\"";
                fileOpener.StartInfo.UseShellExecute = false;
                fileOpener.Start();
                fileOpener.WaitForExit();
            }
            
            if (!File.Exists(path))
                return CommandCode.Error("File not found: " + path);
            description = File.ReadAllText(path);

            description = description.Trim();
            if(modinfo.Description == description)
                return CommandCode.Success();
            modinfo.Description = description;
            Tools.WriteColoredLine($"Commiting changes", ConsoleColor.Cyan);
            if (!clerk.SetModInfos(modinfo, "Update mod description"))
                return clerk.LastError;

            return CommandCode.Success();
        }

        public void WaitForFile(string path)
        {
            FileStream? file = null;
            while (true)
            {
                try
                {
                    file = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                }
                catch (IOException)
                {
                    if(file is not null) file.Dispose();   
                    Thread.Sleep(500);
                    continue;
                }
                break;
            }
        }
    }
}