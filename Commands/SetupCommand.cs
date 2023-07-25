using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tot;

namespace Tot.Commands
{
    [Verb("setup", HelpText = "Setup the Dev Kit path")]
    internal class SetupCommand : ICommand
    {
        [Value(0, HelpText = "Path to the Dev Kit", Required = true)]
        public string? path { get; set; }

        public CommandCode Execute()
        {
            if (path == null)
                return CommandCode.MissingArg(nameof(path));

            DirectoryInfo devkit = new DirectoryInfo(path);
            if (!devkit.Exists)
                return CommandCode.NotFound(devkit);
            
            FileInfo cmd = new FileInfo(Path.Join(devkit.FullName, KitchenClerk.CMDBinary));
            if (!cmd.Exists)
                return CommandCode.NotFound(cmd);

            Config config = new Config() { DevKitPath = devkit.FullName };

            try
            {
                config.SaveConfig();
            }
            catch (Exception)
            {
                return CommandCode.Forbidden(new FileInfo(Config.GetConfigPath() ?? ""));
            }

            return CommandCode.Success("Setup Successful");
        }
    }
}
