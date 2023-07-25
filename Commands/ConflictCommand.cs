using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tot.Commands
{
    [Verb("conflict", HelpText = "Process a mod list to highlight common files")]
    internal class ConflictCommand : ModBasedCommand, ICommand
    {
        [Value(0, HelpText = "Path to the mod list", Required = true)]
        public string? path { get; set; }

        public CommandCode Execute()
        {
            if (!KitchenClerk.CreateDevKitClerk(out KitchenClerk clerk))
                return clerk.LastError;

            if(string.IsNullOrEmpty(path))
                return CommandCode.MissingArg(nameof(path));

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

            Dictionary<string, List<PakedFile>> folded = new Dictionary<string, List<PakedFile>>();
            foreach (PakListing listing in listings)
            {
                foreach (PakedFile pakedFile in listing.pakedFiles)
                {
                    if (folded.ContainsKey(pakedFile.path))
                        folded[pakedFile.path].Add(pakedFile);
                    else folded.Add(pakedFile.path, new List<PakedFile> { pakedFile });
                }
            }

            foreach (KeyValuePair<string, List<PakedFile>> keyValuePair in folded)
            {
                if (!keyValuePair.Key.Contains("/"))
                    continue;

                if (keyValuePair.Value.Count > 1 && !keyValuePair.Value.AreShaIdentical())
                {
                    Console.WriteLine(keyValuePair.Value[0].path);
                    foreach (PakedFile pakedFile in keyValuePair.Value)
                        Tools.WriteColoredLine($"{pakedFile.sha} - {pakedFile.pakName} ({pakedFile.size} bytes)", ConsoleColor.DarkGray);
                }
            }

            return CommandCode.Success();
        }
    }
}
