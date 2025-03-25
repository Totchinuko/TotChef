using System.CommandLine;

namespace tot_lib;

public interface IColoredConsole : IConsole, ITotService
{
    ConsoleColor BackgroundColor { get; set; }
    ConsoleColor ForegroundColor { get; set; }
    void ResetColor();
}