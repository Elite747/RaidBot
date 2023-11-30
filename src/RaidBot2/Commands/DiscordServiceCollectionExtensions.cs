using RaidBot2.Commands;

namespace Microsoft.Extensions.DependencyInjection;

public static class CommandsServiceCollectionExtensions
{
    public static IServiceCollection AddCommands(this IServiceCollection services)
    {
        services.AddTransient<RaidCommandContext>();
        services.AddTransient<ConfigureCommand>();
        return services;
    }
}
