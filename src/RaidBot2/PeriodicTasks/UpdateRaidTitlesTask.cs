using Microsoft.EntityFrameworkCore;
using RaidBot2.Data;

namespace RaidBot2.PeriodicTasks;

public class UpdateRaidTitlesTask(ApplicationDbContext db) : IGuildExpansionTask
{
    public async Task ExecuteAsync(GuildExpansionContext context, CancellationToken cancellationToken = default)
    {
        await foreach (var raid in db.Raids.Select(r => new { r.Name, r.Date, r.ChannelId, r.Configuration.Timezone }).AsAsyncEnumerable().WithCancellation(cancellationToken))
        {
            var channel = await context.Discord.GetChannelAsync(raid.ChannelId);
            if (channel is null)
            {
                // TODO log
                continue;
            }

            var tz = TimeZoneInfo.FindSystemTimeZoneById(raid.Timezone) ?? TimeZoneInfo.Utc;
            var today = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, tz).Date;
            var raidDate = TimeZoneInfo.ConvertTime(new DateTime(raid.Date.Year, raid.Date.Month, raid.Date.Day, raid.Date.Hour, raid.Date.Minute, raid.Date.Second, DateTimeKind.Utc), tz).Date;

            string prefix;
            if (raidDate == today)
            {
                prefix = "⭐";
            }
            else if (raidDate < today)
            {
                prefix = "❌";
            }
            else
            {
                prefix = "";
            }

            string targetName = $"{prefix}{raidDate:MMM-dd}-{raid.Name.Replace(' ', '-')}";

            if (channel.Name != targetName)
            {
                await channel.ModifyAsync(c => c.Name = targetName);
            }
        }
    }
}
