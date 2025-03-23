using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tot.Commands
{
    [Verb("checkout", HelpText = "Checkout the dedicated mod branch (sharing the same name) in the ModsShared folder")]
    internal class CheckoutCommand : ModBasedCommand, ICommand
    {
        public CommandCode Execute()
        {
            if (!KitchenClerk.CreateClerk(ModName, out KitchenClerk clerk))
                return clerk.LastError;

            if (!Repository.IsValid(clerk.ModsShared.FullName))
                return CommandCode.Success();

            if(!clerk.CheckoutModsSharedBranch(out string branch))
                return clerk.LastError;
            return CommandCode.Success($"{branch} branch is now active on Shared repository");
        }
    }
}
