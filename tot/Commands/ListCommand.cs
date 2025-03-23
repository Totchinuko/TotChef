using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tot.Commands
{
    [Verb("list", HelpText = "List the mods available in the DevKit")]
    internal class ListCommand : ICommand
    {
        public CommandCode Execute()
        {
            if(!KitchenClerk.CreateDevKitClerk(out KitchenClerk clerk))
                return clerk.LastError;

            Tools.WriteColoredLine("Mod list:", ConsoleColor.Cyan);
            foreach (DirectoryInfo directory in clerk.ModsFolder.GetDirectories())
            {
                Console.WriteLine(directory.Name);
            }
            return CommandCode.Success();
        }
    }
}
