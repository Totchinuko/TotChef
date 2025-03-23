using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Tot.Commands
{
    [Verb("bump", HelpText = "Bump a mod version")]
    internal class VersionBumpCommand : ModBasedCommand, ICommand
    {
        [Value(0, HelpText = "Part of the version to bump (major.minor.build)", Required = true)]
        public string Part { get; set; } = string.Empty;

        public CommandCode Execute()
        {
            if (!KitchenClerk.CreateClerk(ModName, out KitchenClerk clerk))
                return clerk.LastError;

            if (!clerk.GetModInfos(out ModinfoData modinfo))
                return clerk.LastError;

            switch (Part)
            {
                case "major":
                    modinfo.VersionMajor++;
                    modinfo.VersionMinor = 0;
                    modinfo.VersionBuild = 0;
                    break;
                case "minor":
                    modinfo.VersionMinor++;
                    modinfo.VersionBuild = 0;
                    break;
                case "build":
                    modinfo.VersionBuild++;
                    break;
                default:
                    return CommandCode.MissingArg("Part");
            }

            Regex regex = new Regex(@"([0-9]+)\.([0-9]+)\.([0-9]+)");
            modinfo.Name = regex.Replace(modinfo.Name, $"{modinfo.VersionMajor}.{modinfo.VersionMinor}.{modinfo.VersionBuild}");

            Tools.WriteColoredLine(modinfo.Name, ConsoleColor.Cyan);

            if (!clerk.SetModInfos(modinfo))
                return clerk.LastError;
            
            if(!clerk.UpdateModVersion())
                return clerk.LastError;

            return CommandCode.Success();
        }
    }
}