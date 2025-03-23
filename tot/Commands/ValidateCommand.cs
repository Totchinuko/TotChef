using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tot.Commands
{
    [Verb("validate", HelpText = "Validate git repositories for the cooking process")]
    internal class ValidateCommand : ModBasedCommand, ICommand
    {
        public CommandCode Execute()
        {
            if (!KitchenClerk.CreateClerk(ModName, out KitchenClerk clerk))
                return clerk.LastError;

            if (clerk.IsGitRepoDirty(clerk.ModsShared))
                return new CommandCode { code = CommandCode.RepositoryIsDirty, message = "ModsShared repo is dirty" };
            if (clerk.IsGitRepoDirty(clerk.ModFolder))
                return new CommandCode { code = CommandCode.RepositoryIsDirty, message = $"Mod {clerk.ModName} repo is dirty" };
            return CommandCode.Success();
        }
    }
}
