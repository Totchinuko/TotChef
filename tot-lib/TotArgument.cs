using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;

namespace tot_lib;

public class TotArgument<T> : Argument<T>, IValueSymbol
{
    private Action<T?>? _setter;

    public TotArgument()
    {
    }

    public TotArgument(string? name, string? description = null) : base(name, description)
    {
    }

    public TotArgument(string name, Func<T> getDefaultValue, string? description = null) : base(name, getDefaultValue, description)
    {
    }

    public TotArgument(Func<T> getDefaultValue) : base(getDefaultValue)
    {
    }

    public TotArgument(string? name, ParseArgument<T> parse, bool isDefault = false, string? description = null) : base(name, parse, isDefault, description)
    {
    }

    public TotArgument(ParseArgument<T> parse, bool isDefault = false) : base(parse, isDefault)
    {
    }

    public void AddSetter(Action<T?> action)
    {
        _setter = action;
    }

    void IValueSymbol.SetValue(InvocationContext context)
    {
        _setter?.Invoke(context.ParseResult.GetValueForArgument(this));
    }
}