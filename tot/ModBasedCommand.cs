using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using tot_lib;
using tot.Services;

namespace Tot;

public class ModBasedCommand<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
    TOptions,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
    TOptionsHandler> :
    Command<TOptions, TOptionsHandler>
    where TOptions : ModBasedCommandOptions, new()
    where TOptionsHandler : ModBasedCommandHandler<TOptions>
{
    public ModBasedCommand(string verb, string description) : base(verb, description)
    {
        var option = new Option<string>("--conan-mod",
            "Specify the mod name you want to perform the action on");
        option.AddAlias("-m");
        AddOption(option);
    }
}

public class ModBasedCommandOptions : ICommandOptions
{
    public string ConanMod { get; set; } = string.Empty;
}

public class ModBasedCommandHandler<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
    TOptions>(KitchenFiles kitchenFiles) : ICommandOptionsHandler<TOptions>
    where TOptions : ModBasedCommandOptions, new()
{
    public virtual Task<int> HandleAsync(TOptions options, CancellationToken cancellationToken)
    {
        kitchenFiles.SetModName(options.ConanMod);
        if (!kitchenFiles.IsDevkitPathValid())
            throw CommandCode.NotFound(kitchenFiles.DevKit);
        if (!kitchenFiles.IsModPathValid())
            throw CommandCode.NotFound(kitchenFiles.ModFolder);

        return Task.FromResult(0);
    }
}