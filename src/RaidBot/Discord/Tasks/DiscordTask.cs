using Discord;
using Discord.WebSocket;

namespace RaidBot.Discord.Tasks;

public class DiscordTask(Func<DiscordSocketClient, CancellationToken, Task> taskFactory, IInteractionContext interactionContext) : DiscordTaskBase
{
    public override IInteractionContext InteractionContext { get; } = interactionContext;

    protected override Task ExecuteAsync(DiscordSocketClient discord, CancellationToken cancellationToken)
    {
        return taskFactory(discord, cancellationToken);
    }
}
