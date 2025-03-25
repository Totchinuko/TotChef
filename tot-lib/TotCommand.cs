using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace tot_lib;

public sealed class TotCommand : Command
{
    private IServiceProvider _provider;
    private ITotCommand _command;
    
    private TotCommand(ITotCommand command, Action<IServiceCollection> configureServices) : base(command.Command, command.Description)
    {
        _command = command;
        var services = new ServiceCollection();
        services.TryAddSingleton<IColoredConsole>(new DotnetConsole());
        configureServices(services);
        _provider = services.BuildServiceProvider();
        
        if(_command is ITotCommandOptions options)
            foreach (var option in options.GetOptions())
                AddOption(option);
        if(_command is ITotCommandArguments arguments)
            foreach (var argument in arguments.GetArguments())
                AddArgument(argument);
        if(_command is ITotCommandSubCommands subCommands)
            foreach (var c in subCommands.GetSubCommands())
                 AddCommand(new TotCommand(c, configureServices));
        
        if(_command is ITotCommandInvoked invoked)
            this.SetHandler(BaseHandlerAsync);
    }

    private Task<int> BaseHandlerAsync(InvocationContext context)
    {
        foreach(var option in Options)
            if(option is IValueSymbol valueSymbol)
                valueSymbol.SetValue(context);
        foreach(var arg in Arguments)
            if(arg is IValueSymbol valueSymbol)
                valueSymbol.SetValue(context);
        
        return ((ITotCommandInvoked)_command).InvokeAsync(_provider, context.GetCancellationToken());
    }

    public static TotCommand Create<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]T>
        (Action<IServiceCollection> configureService) where T : class, ITotCommand
    {
        return new TotCommand(Activator.CreateInstance<T>(), configureService);
    }
}