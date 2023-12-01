using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using RaidBot.Data;
using RaidBot.Discord.Tasks;

namespace RaidBot.PeriodicTasks;

public class GuildExpansionTaskExecutor(
    IDbContextFactory<ApplicationDbContext> dbFactory,
    IEnumerable<IGuildExpansionTask> guildExpansionTasks,
    ILogger<GuildExpansionTaskExecutor> logger) : DiscordTaskBase
{
    protected override async Task ExecuteAsync(DiscordSocketClient discord, CancellationToken cancellationToken)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        await foreach (var config in db.GuildExpansionConfigurations
            .Select(x => new { x.Id, x.ExpansionId, x.CategoryId, x.GuildId })
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken))
        {
            var guild = await discord.Rest.GetGuildAsync(config.GuildId);
            if (guild is null)
            {
                logger.LogWarning("Configuration found for guild with id '{guildId}', but it either doesn't exist or we don't have permission to access it anymore.", config.GuildId);
                continue;
            }
            var allChannels = await guild.GetChannelsAsync();
            if (allChannels?.FirstOrDefault(c => c is ICategoryChannel && c.Id == config.CategoryId) is not { } category)
            {
                logger.LogWarning("Configuration with guild id '{guildId}' has a channel configured with id '{channelId}' that no longer exists.", config.GuildId, config.CategoryId);
                continue;
            }
            GuildExpansionContext context = new(db, config.Id, config.ExpansionId, discord, guild, category);
            foreach (var task in guildExpansionTasks)
            {
                await task.ExecuteAsync(context, cancellationToken);
            }
        }
    }
}
