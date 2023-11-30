namespace RaidBot2.Discord.Tasks;

public interface IDiscordTaskQueue
{
    Task ExecuteAsync(IDiscordTask task, CancellationToken cancellationToken = default);

    void Execute(IDiscordTask task);
}
