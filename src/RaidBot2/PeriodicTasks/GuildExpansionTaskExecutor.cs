using DSharpPlus;
using Microsoft.EntityFrameworkCore;
using RaidBot2.Data;
using RaidBot2.Discord.Tasks;

namespace RaidBot2.PeriodicTasks;

public class GuildExpansionTaskExecutor(
    ApplicationDbContext db,
    IEnumerable<IGuildExpansionTask> guildExpansionTasks,
    ILogger<GuildExpansionTaskExecutor> logger) : DiscordTaskBase
{
    protected override async Task ExecuteAsync(DiscordClient discord, CancellationToken cancellationToken)
    {
        await foreach (var config in db.GuildExpansionConfigurations
            .Select(x => new { x.Id, x.ExpansionId, x.CategoryId, x.GuildId })
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken))
        {
            var guild = await discord.GetGuildAsync(config.GuildId);
            if (guild is null)
            {
                logger.LogWarning("Configuration found for guild with id '{guildId}', but it either doesn't exist or we don't have permission to access it anymore.", config.GuildId);
                continue;
            }
            var allChannels = await guild.GetChannelsAsync();
            if (allChannels?.FirstOrDefault(c => c.Id == config.CategoryId && c.IsCategory) is not { } category)
            {
                logger.LogWarning("Configuration with guild id '{guildId}' has a channel configured with id '{channelId}' that no longer exists.", config.GuildId, config.CategoryId);
                continue;
            }
            GuildExpansionContext context = new(config.Id, config.ExpansionId, discord, guild, category);
            foreach (var task in guildExpansionTasks)
            {
                await task.ExecuteAsync(context, cancellationToken);
            }
        }
    }
}
