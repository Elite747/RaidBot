using Discord;
using Discord.WebSocket;

namespace RaidBot.Discord.Tasks;

public abstract class DiscordTaskBase : IDiscordTask
{
    private TaskCompletionSource? _tcs;
    private Task? _executingTask;

    protected abstract Task ExecuteAsync(DiscordSocketClient discord, CancellationToken cancellationToken);

    public virtual IInteractionContext? InteractionContext => null;

    async Task IDiscordTask.ExecuteAsync(DiscordSocketClient discord, CancellationToken cancellationToken)
    {
        _executingTask = ExecuteAsync(discord, cancellationToken);
        await _executingTask;
        _tcs?.SetResult();
    }

    public async Task WaitForExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (_executingTask is not null)
        {
            await _executingTask;
        }
        else
        {
            _tcs ??= new();
            await _tcs.Task;
            _tcs = null;
        }
    }
}
