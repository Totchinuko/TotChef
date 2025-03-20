using System.Net;
using CommandLine;

namespace Tot.Commands
{
    [Verb("steamquery", HelpText = "Return the number of players of a server using steam protocol")]
    internal class SteamQueryCommand : ModBasedCommand, ICommand
    {
        [Value(0, HelpText = "Query IP", Required = true)]
        public string QueryIP { get; set; } = string.Empty;
        [Value(1, HelpText = "Query Port", Required = true)]
        public int QueryPort { get; set; } = 0;

        public CommandCode Execute()
        {
            var query = new SourceQueryReader(new IPEndPoint(IPAddress.Parse(QueryIP), QueryPort), 30 * 1000, 5 * 1000);
            query.Refresh();
            Console.WriteLine($"{query.Name} ({query.Players}/{query.MaxPlayers})");
            return CommandCode.Success();
        }
    }
}