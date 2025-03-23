using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tot.Commands
{
    [Verb("clean", HelpText = "Clean any missing file from the cookinfo.ini")]
    internal class CleanCommand : ModBasedCommand, ICommand
    {
        public CommandCode Execute()
        {
            if(!KitchenClerk.CreateClerk(ModName, out KitchenClerk clerk))
                return clerk.LastError;

            clerk.GetCookInfo(out List<string> included, out List<string> excluded);
            List<string> changes = clerk.RemoveMissingFiles(ref included, ref excluded);
            
            if (!clerk.SetCookInfo(included, excluded))
                return clerk.LastError;

            clerk.DumpChange(changes, ConsoleColor.Magenta);
            return CommandCode.Success($"{changes.Count} missing file(s) removed from {clerk.ModName} cookinfo.ini");
        }
    }
}
