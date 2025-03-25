using System.CommandLine.IO;

namespace tot_lib;

public class DotnetConsoleStreamWriter(TextWriter writer) : IStandardStreamWriter
{
    public void Write(string? value)
    {
        writer.Write(value);
    }
}