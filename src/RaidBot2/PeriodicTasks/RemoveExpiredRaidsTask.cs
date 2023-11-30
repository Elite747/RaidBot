using DSharpPlus;
using RaidBot2.Data;
using RaidBot2.Discord.Tasks;

namespace RaidBot2.PeriodicTasks;

public class RemoveExpiredRaidsTask(ApplicationDbContext db) : DiscordTaskBase
{
    protected override async Task ExecuteAsync(DiscordClient discord, CancellationToken cancellationToken)
    {
        await foreach (var raid in db.Raids.AsAsyncEnumerable().WithCancellation(cancellationToken))
        {
            var channel = await discord.GetChannelAsync(raid.ChannelId);

            if (channel is null)
            {
                db.Raids.Remove(raid);
                continue;
            }

            var raidDate = new DateTime(raid.Date.Year, raid.Date.Month, raid.Date.Day, raid.Date.Hour, raid.Date.Minute, raid.Date.Second, DateTimeKind.Utc);

            if (raidDate < DateTimeOffset.UtcNow.AddDays(-2))
            {
                db.Raids.Remove(raid);
                await channel.DeleteAsync();
            }
        }
        await db.SaveChangesAsync(cancellationToken);
    }
}
