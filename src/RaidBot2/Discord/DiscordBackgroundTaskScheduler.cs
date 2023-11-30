using Microsoft.Extensions.Options;
using RaidBot2.Discord.Tasks;

namespace RaidBot2.Discord;

public class DiscordBackgroundTaskScheduler(
    IServiceProvider serviceProvider,
    IOptions<DiscordConfigurationOptions> options,
    ILogger<DiscordBackgroundTaskScheduler> logger) : BackgroundService
{
    private readonly TimeSpan _interval = options.Value.PeriodicTaskInterval > TimeSpan.Zero ? options.Value.PeriodicTaskInterval : TimeSpan.FromHours(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await using (var scope = serviceProvider.CreateAsyncScope())
            {
                foreach (var task in scope.ServiceProvider.GetServices<IDiscordTask>())
                {
                    try
                    {
                        await scope.ServiceProvider.GetRequiredService<IDiscordTaskQueue>().ExecuteAsync(task, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failure while running background task '{task}'.", task.GetType().Name);
                    }
                }
            }
            await Task.Delay(_interval, stoppingToken);
        }
    }
}
