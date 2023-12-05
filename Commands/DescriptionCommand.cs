using CommandLine;
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
        [Option('w', "write")]
        public bool write { get; set; } = false;

        public CommandCode Execute()
        {
            if (!KitchenClerk.CreateClerk(ModName, out KitchenClerk clerk))
                return clerk.LastError;

            if (!clerk.GetModInfos(out ModinfoData modinfo))
                return clerk.LastError;

            if (!clerk.GetTemporaryFile(clerk.ModName + ".txt", out string path))
                return clerk.LastError;

            if (write)
            {
                if (!File.Exists(path))
                    return CommandCode.Error("File not found: " + path);
                string description = File.ReadAllText(path);
                description = description.Replace("\"", "\\\"");
                description = description.Replace("\n", "\\n");

                modinfo.Description = description;
                if (!clerk.SetModInfos(modinfo))
                    return clerk.LastError;
            }
            else
            {
                if (!File.Exists(path))
                    return CommandCode.Error("File not found: " + path);
                string description = modinfo.Description;
                description = description.Replace("\\\"", "\"");
                description = description.Replace("\\n", "\n");
                File.WriteAllText(path, description);

                using (Process fileOpener = new Process())
                {
                    fileOpener.StartInfo.FileName = "explorer";
                    fileOpener.StartInfo.Arguments = $"\"{path}\"";
                    fileOpener.Start();
                }
            }

            return CommandCode.Success();
        }
    }
}