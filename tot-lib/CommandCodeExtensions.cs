using System.CommandLine;
using System.CommandLine.IO;

namespace tot_lib;

public static class CommandCodeExtensions
{
    public static Task<int> OutputCommandError(this IConsole console, Exception? ex)
    {
        if (ex is CommandException cEx) return OutputCommandError(console, cEx);
        if (ex is null) return Task.FromResult(0);
        console.Error.WriteLine("Error:" + ex.Message);
        return Task.FromResult(CommandCode.UnknownError);
    }

    public static Task<int> OutputCommandError(this IConsole console, CommandException? code)
    {
        if (code is null || code.Code == 0) return Task.FromResult(0);
        console.Error.WriteLine("Error:" + code.Message);
        return Task.FromResult(code.Code);
    }

    public static Task<int> OutputCommandError(this IConsole console, int code, string message)
    {
        if (code == 0) return Task.FromResult(0);
        console.Error.WriteLine("Error:" + message);
        return Task.FromResult(code);
    }
}