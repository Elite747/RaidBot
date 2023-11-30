using DSharpPlus;

namespace RaidBot2.Discord.Tasks;

public interface IDiscordTask
{
    Task ExecuteAsync(DiscordClient discord, CancellationToken cancellationToken = default);

    Task WaitForExecuteAsync(CancellationToken cancellationToken = default);
}
