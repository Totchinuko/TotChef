using System.CommandLine;

namespace tot_lib;

public static class ConsoleExtensions
{
    public static void Write(this IColoredConsole console, ConsoleColor foregroundColor, params string[] output)
    {
        console.ForegroundColor = foregroundColor;
        foreach (var o in output)
            console.Write(o);
        console.ResetColor();
    }
    
    public static void WriteLine(this IColoredConsole console, ConsoleColor foregroundColor, params string[] output)
    {
        console.ForegroundColor = foregroundColor;
        foreach (var o in output)
            console.WriteLine(o);
        console.ResetColor();
    }
    
    public static void Write(this IConsole console, string prefix, params string[] output)
    {
        foreach (var o in output)
            console.Write(o);
    }
    
    public static void WriteLine(this IConsole console, string prefix, params string[] output)
    {
        foreach (var o in output)
            console.WriteLine(o);
    }
    
    public static void Write(this IConsole console, params string[] output)
    {
        foreach (var o in output)
            console.Write(o);
    }
    
    public static void WriteLine(this IConsole console, params string[] output)
    {
        foreach (var o in output)
            console.WriteLine(o);
    }
}