using System.CommandLine;

namespace tot_lib;

public static class ConsoleExtensions
{
    public static void Write(this IColoredConsole console, ConsoleColor foregroundColor, string output)
    {
        console.ForegroundColor = foregroundColor;
        console.Write(output);
        console.ResetColor();
    }
    
    public static void WriteLines(this IColoredConsole console, ConsoleColor foregroundColor, params string[] output)
    {
        console.ForegroundColor = foregroundColor;
        foreach (var o in output)
            console.WriteLine(o);
        console.ResetColor();
    }
    
    public static void WriteLines(this IConsole console, params string[] output)
    {
        foreach (var o in output)
            console.WriteLine(o);
    }
}