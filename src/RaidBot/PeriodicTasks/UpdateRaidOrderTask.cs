using Discord;
using Microsoft.EntityFrameworkCore;

namespace RaidBot.PeriodicTasks;

public class UpdateRaidOrderTask : IGuildExpansionTask
{
    public async Task ExecuteAsync(GuildExpansionContext context, CancellationToken cancellationToken = default)
    {
        int index = context.Category.Position;

        var raidChannelIds = await context.Database.Raids.Where(r => r.ConfigurationId == context.ConfigurationId)
            .OrderBy(r => r.Date)
            .ThenBy(r => r.Name)
            .Select(r => r.ChannelId)
            .ToListAsync(cancellationToken);

        var allChannels = await context.Guild.GetChannelsAsync();

        foreach (var untrackedChannel in allChannels.Where(c => c is INestedChannel nested && nested.CategoryId == context.Category.Id && !raidChannelIds.Contains(c.Id)).OrderBy(c => c.Position))
        {
            index++;
            await untrackedChannel.ModifyAsync(x => x.Position = index);
        }

        foreach (var channelId in raidChannelIds)
        {
            if (await context.Guild.GetChannelAsync(channelId) is { } channel)
            {
                index++;
                await channel.ModifyAsync(x => x.Position = index);
            }
        }
    }
}
