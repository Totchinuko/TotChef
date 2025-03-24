using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using Microsoft.Extensions.DependencyInjection;
using tot_lib;
using Tot.Commands;
using tot.Services;

namespace Tot;

internal class Program
{
    private static int Main(string[] args)
    {
        var builder = new CommandLineBuilder().UseDefaults().UseDependencyInjection(services =>
        {
            services.AddSingleton(Config.LoadConfig());
            services.AddSingleton<GitHandler>();
            services.AddSingleton<KitchenFiles>();
            services.AddSingleton<KitchenClerk>();

            services.AddTransient<CheckoutCommand>();
            services.AddTransient<CleanCommand>();
            services.AddTransient<ConfigCommand>();
        });

        return builder.Build().Invoke(args);
    }
}