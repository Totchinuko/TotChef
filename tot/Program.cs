using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using Microsoft.Extensions.DependencyInjection;
using tot_lib;
using Tot.Commands;
using tot.Services;

namespace Tot;

internal class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("Conan Exile Modding Cli");
        rootCommand.CreateTot<CheckoutCommand>(ConfigureServices);
        rootCommand.CreateTot<CleanCommand>(ConfigureServices);
        rootCommand.CreateTot<ConfigCommand>(ConfigureServices);
        rootCommand.CreateTot<ConflictCommand>(ConfigureServices);
        rootCommand.CreateTot<CookCommand>(ConfigureServices);
        rootCommand.CreateTot<DescriptionCommand>(ConfigureServices);
        rootCommand.CreateTot<DevKitCommand>(ConfigureServices);
        rootCommand.CreateTot<GhostCommand>(ConfigureServices);
        rootCommand.CreateTot<ListCommand>(ConfigureServices);
        rootCommand.CreateTot<OpenCommand>(ConfigureServices);
        rootCommand.CreateTot<PakCommand>(ConfigureServices);
        rootCommand.CreateTot<PathCommand>(ConfigureServices);
        rootCommand.CreateTot<SearchCommand>(ConfigureServices);
        rootCommand.CreateTot<StatusCommand>(ConfigureServices);
        rootCommand.CreateTot<SwapCommand>(ConfigureServices);
        rootCommand.CreateTot<SwitchCommand>(ConfigureServices);
        rootCommand.CreateTot<ValidateCommand>(ConfigureServices);
        rootCommand.CreateTot<VersionCommand>(ConfigureServices);
        
        return await rootCommand.InvokeAsync(args);
    }

    static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IColoredConsole>(Console.IsOutputRedirected ? new ColorlessConsole() : new DotnetConsole());
        services.AddSingleton(Config.LoadConfig());
        services.AddSingleton<KitchenFiles>();
        services.AddSingleton<KitchenClerk>();
        services.AddSingleton<GitHandler>();
        services.AddSingleton<Stove>();
    }
}