using System.CommandLine.Invocation;

namespace tot_lib;

public interface IValueSymbol
{
    void SetValue(InvocationContext context);
}