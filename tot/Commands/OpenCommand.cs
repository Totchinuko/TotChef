using CommandLine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tot.Commands
{
    [Verb("open", HelpText = "Open the folder containing the pak files")]
    internal class OpenCommand : ModBasedCommand, ICommand
    {
        public CommandCode Execute()
        {
            if (!KitchenClerk.CreateClerk(ModName, out KitchenClerk clerk))
                return clerk.LastError;

            Process.Start("explorer.exe", clerk.ModPakFolder.FullName);
            return CommandCode.Success();
        }
    }
}
