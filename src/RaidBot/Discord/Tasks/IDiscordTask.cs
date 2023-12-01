using Discord;
using Discord.WebSocket;

namespace RaidBot.Discord.Tasks;

public interface IDiscordTask
{
    Task ExecuteAsync(DiscordSocketClient discord, CancellationToken cancellationToken = default);

    Task WaitForExecuteAsync(CancellationToken cancellationToken = default);

    IInteractionContext? InteractionContext { get; }
}
