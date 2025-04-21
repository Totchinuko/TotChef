using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Help;
using System.CommandLine.Parsing;
using System.Drawing;
using Microsoft.Extensions.DependencyInjection;
using Pastel;
using Serilog;
using Serilog.Core;
using Serilog.Templates;
using Serilog.Templates.Themes;
using Tot.Commands;
using tot.Services;

namespace Tot;

internal static class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("Conan Exile Modding Cli");
        rootCommand.AddCommand(CheckoutCommand.Command);
        rootCommand.AddCommand(CleanCommand.Command);
        rootCommand.AddCommand(ConfigCommand.Command);
        rootCommand.AddCommand(ConflictCommand.Command);
        rootCommand.AddCommand(CookCommand.Command);
        rootCommand.AddCommand(DescriptionCommand.Command);
        rootCommand.AddCommand(DevKitCommand.Command);
        rootCommand.AddCommand(ListCommand.Command);
        rootCommand.AddCommand(OpenCommand.Command);
        rootCommand.AddCommand(PakCommand.Command);
        rootCommand.AddCommand(GhostCommand.Command);
        rootCommand.AddCommand(PathCommand.Command);
        rootCommand.AddCommand(SearchCommand.Command);
        rootCommand.AddCommand(StatusCommand.Command);
        rootCommand.AddCommand(SwapCommand.Command);
        rootCommand.AddCommand(SwitchCommand.Command);
        rootCommand.AddCommand(ValidateCommand.Command);
        rootCommand.AddCommand(VersionCommand.Command);
        rootCommand.AddCommand(NoteCommand.Command);

        var parser = new CommandLineBuilder(rootCommand)
            .UseDefaults()
            .UseHelp(ctx =>
            {
                ctx.HelpBuilder.CustomizeLayout(
                    _ => HelpBuilder.Default
                        .GetLayout()
                        .Skip(1)
                        .Prepend(hc => hc.Output.WriteLine("tot.exe is a CLI that provide helpers for modding Conan Exile, using .net, and is an Open Source project covered by the GNU General Public License version 2."))
                        .Prepend(hc => hc.Output.WriteLine("Tot!Chet".Pastel(Constants.ColorOrange)))
                    );
            }).Build();
        
        return await parser.InvokeAsync(args);
    }

    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging(builder => builder.AddSerilog(GetLogger(), true));
        services.AddSingleton(Config.LoadConfig());
        services.AddSingleton<KitchenFiles>();
        services.AddSingleton<KitchenClerk>();
        services.AddSingleton<GitHandler>();
        services.AddSingleton<Stove>();
        services.AddSingleton<PatchHandler>();
    }

    public static Logger GetLogger()
    {
        return new LoggerConfiguration()
#if !DEBUG
            .MinimumLevel.Information()
#endif
            .WriteTo.Console(new ExpressionTemplate(
                "[{@t:HH:mm:ss} {@l:u3}" +
                "{#if DevKitSource is not null} " +
                    "{DevKitSource,-25}" +
                "{#end}" +
                "] {@m}\n{@x}",
                theme: TemplateTheme.Code   
                ))
            .CreateLogger();
    }
}