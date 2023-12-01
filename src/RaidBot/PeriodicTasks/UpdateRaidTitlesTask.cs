using Discord.Rest;
using Microsoft.EntityFrameworkCore;

namespace RaidBot.PeriodicTasks;

public class UpdateRaidTitlesTask : IGuildExpansionTask
{
    public async Task ExecuteAsync(GuildExpansionContext context, CancellationToken cancellationToken = default)
    {
        await foreach (var raid in context.Database.Raids
            .Select(r => new { r.Name, r.Date, r.ChannelId, r.Configuration.Timezone, Expansion = r.Configuration.Expansion.ShortName })
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken))
        {
            if (await context.Discord.Rest.GetChannelAsync(raid.ChannelId) is not RestGuildChannel channel)
            {
                // TODO log
                continue;
            }

            var today = TimeZoneHelpers.ConvertTimeToLocal(DateTime.UtcNow, raid.Timezone, isUtc: true).Date;
            var raidDate = TimeZoneHelpers.ConvertTimeToLocal(raid.Date, raid.Timezone, isUtc: true).Date;

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
