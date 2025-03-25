using System.CommandLine.IO;

namespace tot_lib;

public class DotnetConsole : IColoredConsole
{
    public IStandardStreamWriter Out { get; } = new DotnetConsoleStreamWriter(Console.Out);
    public bool IsOutputRedirected => Console.IsOutputRedirected;
    public IStandardStreamWriter Error { get; } = new DotnetConsoleStreamWriter(Console.Error);
    public bool IsErrorRedirected => Console.IsErrorRedirected;
    public bool IsInputRedirected => Console.IsInputRedirected;


    public ConsoleColor BackgroundColor
    {
        get => Console.BackgroundColor;
        set => Console.BackgroundColor = value;
    }

    public ConsoleColor ForegroundColor
    {
        get => Console.ForegroundColor;
        set => Console.ForegroundColor = value;
    }
    public void ResetColor()
    {
        Console.ResetColor();
    }
}