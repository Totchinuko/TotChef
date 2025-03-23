using CommandLine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tot.Commands
{
    [Verb("devkit", HelpText = "Open the devkit for the targeted mod")]
    internal class DevKitCommand : ModBasedCommand, ICommand
    {
        public CommandCode Execute()
        {
            if (!KitchenClerk.CreateClerk(ModName, out KitchenClerk clerk))
                return clerk.LastError;

            if(!clerk.CheckoutModsSharedBranch(out string branch))
                return clerk.LastError;
            Tools.WriteColoredLine($"{branch} branch is now active on Shared repository", ConsoleColor.Cyan);

            if(!clerk.SwitchActive())
                return clerk.LastError;
            Tools.WriteColoredLine($"Set {clerk.ModName} as active", ConsoleColor.Cyan);
            Process.Start(clerk.UE4Editor.FullName, string.Join(" ", clerk.UProject.FullName, string.Join(" ", clerk.EditorArgs)));

            return CommandCode.Success();
        }
    }
}
