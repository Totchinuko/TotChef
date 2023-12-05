using CommandLine;
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
        [Option('f', "feature")]
        public bool major { get; set; }

        [Value(0, HelpText = "Numeric modifier (+1/-1)", Required = true)]
        public int modifier { get; set; }

        [Option('b', "build")]
        public bool patch { get; set; }

        [Option('v', "version")]
        public bool version { get; set; }

        public CommandCode Execute()
        {
            if (!KitchenClerk.CreateClerk(ModName, out KitchenClerk clerk))
                return clerk.LastError;

            if (!clerk.GetModInfos(out ModinfoData modinfo))
                return clerk.LastError;

            if (!major && !version && !patch)
                return CommandCode.Error("Need at least one version flag");

            if (version)
                modinfo.VersionMajor += modifier;
            if (major)
                modinfo.VersionMinor += modifier;
            if (patch)
                modinfo.VersionBuild += modifier;

            Regex regex = new Regex(@"([0-9]+)\.([0-9]+)\.([0-9]+)");
            modinfo.Name = regex.Replace(modinfo.Name, $"{modinfo.VersionMajor}.{modinfo.VersionMinor}.{modinfo.VersionBuild}");

            Tools.WriteColoredLine(modinfo.Name, ConsoleColor.Cyan);

            if (!clerk.SetModInfos(modinfo))
                return clerk.LastError;

            return CommandCode.Success();
        }
    }
}