using DSharpPlus;

namespace RaidBot2.Discord.Tasks;

public class DiscordTask(Func<DiscordClient, CancellationToken, Task> taskFactory) : DiscordTaskBase
{
    protected override Task ExecuteAsync(DiscordClient discord, CancellationToken cancellationToken)
    {
        return taskFactory(discord, cancellationToken);
    }
}
