
using System.Threading.Channels;

namespace RaidBot.Discord.Tasks;

public class DiscordTaskQueue(ChannelWriter<IDiscordTask> commandWriter) : IDiscordTaskQueue
{
    public void Execute(IDiscordTask task)
    {
        commandWriter.TryWrite(task);
    }

    public async Task ExecuteAsync(IDiscordTask task, CancellationToken cancellationToken = default)
    {
        await commandWriter.WriteAsync(task, cancellationToken);
        await task.WaitForExecuteAsync(cancellationToken);
    }
}
