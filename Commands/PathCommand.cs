using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tot.Commands
{
    [Verb("path", HelpText = "Return a path to be used with cd")]
    internal class PathCommand : ModBasedCommand, ICommand
    {
        [Option('s', "shared", HelpText = "Return the path to the shared folder")]
        public bool shared { get; set; }

        [Option('p', "pak", HelpText = "Return the path to the cooked pak file")]
        public bool pak { get; set; }

        public CommandCode Execute()
        {
            string path = "";
            if(!string.IsNullOrEmpty(ModName))
            {
                if (!KitchenClerk.CreateClerk(ModName, out KitchenClerk clerk))
                    return clerk.LastError;
                path = clerk.ModFolder.PosixFullName();
            }
            else
            {
                if (!KitchenClerk.CreateDevKitClerk(out KitchenClerk clerk))
                    return clerk.LastError;
                if(shared)
                    path = clerk.ModsShared.PosixFullName();
                else if(pak)
                    path = clerk.ModPakFile.PosixFullName();
            }

            Console.Write(path);
            return CommandCode.Success();
        }
    }
}
