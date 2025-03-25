using System.CommandLine.IO;

namespace tot_lib;

public class ColorlessConsole : IColoredConsole
{
    public IStandardStreamWriter Out { get; } = new DotnetConsoleStreamWriter(Console.Out);
    public bool IsOutputRedirected => Console.IsOutputRedirected;
    public IStandardStreamWriter Error { get; } = new DotnetConsoleStreamWriter(Console.Error);
    public bool IsErrorRedirected => Console.IsErrorRedirected;
    public bool IsInputRedirected => Console.IsInputRedirected;
    public ConsoleColor BackgroundColor
    {
        get => ConsoleColor.Black;
        set => _ = value;
    }

    public ConsoleColor ForegroundColor
    {
        get => ConsoleColor.White;
        set => _ = value;
    }
    public void ResetColor()
    {
    }
}