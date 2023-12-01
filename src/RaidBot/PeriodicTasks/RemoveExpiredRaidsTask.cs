using Discord.Rest;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using RaidBot.Data;
using RaidBot.Discord.Tasks;

namespace RaidBot.PeriodicTasks;

public class RemoveExpiredRaidsTask(IDbContextFactory<ApplicationDbContext> dbFactory) : DiscordTaskBase
{
    protected override async Task ExecuteAsync(DiscordSocketClient discord, CancellationToken cancellationToken)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        await foreach (var raid in db.Raids.AsAsyncEnumerable().WithCancellation(cancellationToken))
        {
            if (await discord.Rest.GetChannelAsync(raid.ChannelId) is not RestGuildChannel channel)
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
