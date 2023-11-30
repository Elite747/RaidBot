using DSharpPlus;

namespace RaidBot2.Discord.Tasks;

public static class DiscordTaskQueueExtensions
{
    public static async Task ExecuteAsync(this IDiscordTaskQueue discordTaskQueue, Func<DiscordClient, CancellationToken, Task> task, CancellationToken cancellationToken = default)
    {
        await discordTaskQueue.ExecuteAsync(new DiscordTask(task), cancellationToken);
    }

    public static void Execute(this IDiscordTaskQueue discordTaskQueue, Func<DiscordClient, CancellationToken, Task> task)
    {
        discordTaskQueue.Execute(new DiscordTask(task));
    }
}
