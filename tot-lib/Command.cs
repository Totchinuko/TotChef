using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace tot_lib;

public abstract class Command<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]TOptions, 
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]TOptionsHandler> : Command
    where TOptions : class, ICommandOptions
    where TOptionsHandler : class, ICommandOptionsHandler<TOptions>
{
    protected Command(string name, string? description = null) : base(name, description)
    {
        Handler = CommandHandler.Create<TOptions, IServiceProvider, CancellationToken>(HandleOptionsAsync);
    }

    private static async Task<int> HandleOptionsAsync(TOptions options, IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        // True dependency injection happening here
        var handler = ActivatorUtilities.CreateInstance<TOptionsHandler>(serviceProvider);
        return await handler.HandleAsync(options, cancellationToken);
    }
}