using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace Tot.Commands
{
    [Verb("cook", HelpText = "Start a cook process for the mod")]
    internal class CookCommand : ModBasedCommand, ICommand
    {
        [Option('f', "force", HelpText = "Force the cook process even if the repo is dirty")]
        public bool force { get; set; }

        [Option('v', "verbose", HelpText = "Display the Dev Kit cook output")]
        public bool Verbose { get; set; }

        public CommandCode Execute()
        {
            if (!KitchenClerk.CreateClerk(ModName, out KitchenClerk clerk))
                return clerk.LastError;

            if (clerk.IsGitRepoDirty(clerk.ModsShared) && !force)
                return new CommandCode { code = CommandCode.RepositoryIsDirty, message = "ModsShared repo is dirty" };
            if (clerk.IsGitRepoDirty(clerk.ModFolder) && !force)
                return new CommandCode { code = CommandCode.RepositoryIsDirty, message = $"Mod {clerk.ModName} repo is dirty" };
            if (!clerk.IsModsSharedBranchValid())
                return new CommandCode { code = CommandCode.RepositoryWrongBranch, message = "Dedicated ModsShared branch is not checked out" };

            if (!clerk.SwitchActive())
                return clerk.LastError;
            Tools.WriteColoredLine($"{clerk.ModName} is now active", ConsoleColor.Cyan);

            clerk.GetCookInfo(out List<string> included, out List<string> excluded);
            List<string>? change = clerk.UpdateIncludedCookInfo(clerk.ModLocalFolder, ref included, excluded);
            if (change == null)
                return clerk.LastError;

            if (!clerk.SetCookInfo(included, excluded))
                return clerk.LastError;

            if (change.Count > 0)
            {
                clerk.DumpChange(change, ConsoleColor.Yellow);
                Tools.WriteColoredLine($"Added {change.Count} missing local mod files to cooking", ConsoleColor.Cyan);
            }

            Tools.WriteColoredLine($"Cleaning cook folders from previous operations...", ConsoleColor.Cyan);
            clerk.CleanCookingFolder();
            Tools.WriteColoredLine($"Cooking {clerk.ModName}...", ConsoleColor.Cyan);
            Stove stove = new Stove(clerk, Verbose);
            stove.StartCooking();

            if (!stove.wasSuccess)
                return new CommandCode { code = CommandCode.CookingFailure, message = $"Cooking failed. {stove.errors} Error(s)" };

            clerk.CleanCookedFolder();

            if (!clerk.CopyAndFilter(Verbose))
                return clerk.LastError;

            return CommandCode.Success($"{clerk.ModName} cooked successfuly.. !");
        }
    }
}