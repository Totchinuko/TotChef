using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tot.Commands
{
    [Verb("search", HelpText = "Process a mod list to highlight common files")]
    internal class SearchCommand : ICommand
    {
        [Value(0, HelpText = "Path to the mod list", Required = true)]
        public string? path { get; set; }

        [Value(1, HelpText = "Search file name", Required = true)]
        public string? query { get; set; }

        public CommandCode Execute()
        {
            if (!KitchenClerk.CreateDevKitClerk(out KitchenClerk clerk))
                return clerk.LastError;

            if (string.IsNullOrEmpty(path))
                return CommandCode.MissingArg(nameof(path));

            if (string.IsNullOrEmpty(query))
                return CommandCode.MissingArg(nameof(query));

            FileInfo modlistFile = new FileInfo(path).GetProperCasedFileInfo();
            List<string> modlist = File.ReadAllLines(modlistFile.FullName).ToList();

            List<PakListing> listings = new List<PakListing>();
            foreach (string line in modlist)
            {
                string path = line;
                if (path.StartsWith("*"))
                    path = path.Substring(1);
                FileInfo mod = new FileInfo(path);
                if (mod.Exists)
                {
                    Tools.WriteColoredLine($"Parsing {mod.FullName}...", ConsoleColor.Cyan);
                    if (clerk.QueryPakFile(mod, out string output))
                        listings.Add(new PakListing(output, mod.Name));
                    else
                        return clerk.LastError;
                }
                else
                    Tools.WriteColoredLine($"Not Found: {mod.FullName}", ConsoleColor.DarkGray);
            }

            foreach (PakListing listing in listings)
            {
                foreach (PakedFile pakedFile in listing.pakedFiles)
                {
                    if (pakedFile.path.Contains(query))
                        Tools.WriteColoredLine(listing.pakName, ConsoleColor.DarkGray);
                }
            }

            return CommandCode.Success();
        }
    }
}