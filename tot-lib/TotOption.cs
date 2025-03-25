using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;

namespace tot_lib;

public class TotOption<T> : Option<T>, IValueSymbol
{
    private Action<T?>? _setter;
    
    public TotOption(string name, string? description = null) : base(name, description)
    {
    }

    public TotOption(string[] aliases, string? description = null) : base(aliases, description)
    {
    }

    public TotOption(string name, ParseArgument<T> parseArgument, bool isDefault = false, string? description = null) : base(name, parseArgument, isDefault, description)
    {
    }

    public TotOption(string[] aliases, ParseArgument<T> parseArgument, bool isDefault = false, string? description = null) : base(aliases, parseArgument, isDefault, description)
    {
    }

    public TotOption(string name, Func<T> getDefaultValue, string? description = null) : base(name, getDefaultValue, description)
    {
    }

    public TotOption(string[] aliases, Func<T> getDefaultValue, string? description = null) : base(aliases, getDefaultValue, description)
    {
    }

    public void AddSetter(Action<T?> action)
    {
        _setter = action;
    }

    void IValueSymbol.SetValue(InvocationContext context)
    {
        _setter?.Invoke(context.ParseResult.GetValueForOption(this));
    }
}