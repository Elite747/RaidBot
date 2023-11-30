using DSharpPlus;
using DSharpPlus.SlashCommands;

namespace RaidBot2.Discord.Tasks;

public class DiscordTask(Func<DiscordClient, CancellationToken, Task> taskFactory, InteractionContext interactionContext) : DiscordTaskBase
{
    public override InteractionContext InteractionContext { get; } = interactionContext;

    protected override Task ExecuteAsync(DiscordClient discord, CancellationToken cancellationToken)
    {
        return taskFactory(discord, cancellationToken);
    }
}
