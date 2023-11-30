using Microsoft.EntityFrameworkCore;
using RaidBot2.Data;

namespace RaidBot2.PeriodicTasks;

public class UpdateRaidOrderTask(ApplicationDbContext db) : IGuildExpansionTask
{
    public async Task ExecuteAsync(GuildExpansionContext context, CancellationToken cancellationToken = default)
    {
        int index = context.Category.Position;

        var raidChannelIds = await db.Raids.Where(r => r.ConfigurationId == context.ConfigurationId)
            .OrderBy(r => r.Date)
            .ThenBy(r => r.Name)
            .Select(r => r.ChannelId)
            .ToListAsync(cancellationToken);

        foreach (var untrackedChannel in context.Guild.Channels.Values.Where(c => c.ParentId == context.Category.Id && !raidChannelIds.Contains(c.Id)).OrderBy(c => c.Position))
        {
            await untrackedChannel.ModifyPositionAsync(++index);
        }

        foreach (var channelId in raidChannelIds)
        {
            if (context.Guild.Channels.TryGetValue(channelId, out var channel))
            {
                await channel.ModifyPositionAsync(++index);
            }
        }
    }
}
