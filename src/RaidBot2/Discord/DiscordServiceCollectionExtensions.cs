using System.Threading.Channels;
using RaidBot2.Discord;
using RaidBot2.Discord.Tasks;
using RaidBot2.PeriodicTasks;

namespace Microsoft.Extensions.DependencyInjection;

public static class DiscordServiceCollectionExtensions
{
    public static IServiceCollection AddDiscord(this IServiceCollection services, IConfiguration configuration, string sectionName = "Discord")
    {
        services.Configure<DiscordConfigurationOptions>(configuration.GetSection(sectionName));
        services.AddTransient<IDiscordTaskQueue, DiscordTaskQueue>();
        services.AddSingleton<Channel<IDiscordTask>>(_ => Channel.CreateUnbounded<IDiscordTask>(new() { SingleReader = true }));
        services.AddSingleton(provider => provider.GetRequiredService<Channel<IDiscordTask>>().Reader);
        services.AddTransient(provider => provider.GetRequiredService<Channel<IDiscordTask>>().Writer);
        services.AddHostedService<DiscordHost>();
        services.AddHostedService<DiscordBackgroundTaskScheduler>();
        return services;
    }

    public static IServiceCollection AddPeriodicTask<T>(this IServiceCollection services) where T : class, IDiscordTask
    {
        return services.AddTransient<IDiscordTask, T>();
    }

    public static IServiceCollection AddPeriodicGuildExpansionTask<T>(this IServiceCollection services) where T : class, IGuildExpansionTask
    {
        return services.AddTransient<IGuildExpansionTask, T>();
    }
}
