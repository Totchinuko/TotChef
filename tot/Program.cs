using CommandLine;
using LibGit2Sharp;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Tot
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();
            Type[] commands = types.Where(x => x.GetInterfaces().Contains(typeof(ICommand))).ToArray();
            Parser.Default.ParseArguments(args, commands)
                .WithParsed(Run)
                .WithNotParsed(HandleErrors);
        }

        private static void HandleErrors(IEnumerable<Error> enumerable)
        {
#if DEBUG
            foreach (var error in enumerable)
                Tools.WriteColoredLine(error.ToString() ?? "Unknown Error", ConsoleColor.Red);
#endif
            Environment.Exit(CommandCode.MissingArgument);
        }

        private static void Run(object obj)
        {
            CommandCode code;
            if (obj is ICommand)
                code = ((ICommand)obj).Execute();
            else
                code = CommandCode.Unknown();

            if(!string.IsNullOrEmpty(code.message))
                Tools.WriteColoredLine(code.message, code.code == 0 ? ConsoleColor.Cyan : ConsoleColor.Red);
            Environment.Exit(code.code);
        }
    }
}