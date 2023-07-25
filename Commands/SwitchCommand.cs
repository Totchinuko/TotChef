using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tot.Commands
{
    [Verb("switch", HelpText = "Switch the active.txt to the selected mod")]
    internal class SwitchCommand : ModBasedCommand, ICommand
    {
        public CommandCode Execute()
        {
            if (!KitchenClerk.CreateClerk(ModName, out KitchenClerk clerk))
                return clerk.LastError;

            if(!clerk.SwitchActive())
                return clerk.LastError;
            return CommandCode.Success($"{clerk.ModName} is now active");
        }
    }
}
