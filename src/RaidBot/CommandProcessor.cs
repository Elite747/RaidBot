namespace RaidBot;

internal class CommandProcessor(CommandQueue commandQueue, ILogger<CommandProcessor> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var command = await commandQueue.DequeueAsync(stoppingToken);

            if (command is not null)
            {
                try
                {
                    await command();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Executing a command failed.");
                }
            }
        }
    }
}
