using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tot
{
    internal abstract class ModBasedCommand
    {
        [Option('m', "mod", HelpText = "Specify the mod name you want to perform the action on")]
        public string? ModName { get; set; }
    }
}
