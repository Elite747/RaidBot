using Discord;
using Discord.Rest;
using Discord.WebSocket;
using RaidBot.Data;

namespace RaidBot.PeriodicTasks;

public interface IGuildExpansionTask
{
    Task ExecuteAsync(GuildExpansionContext context, CancellationToken cancellationToken = default);
}

public readonly record struct GuildExpansionContext(ApplicationDbContext Database, int ConfigurationId, int ExpansionId, DiscordSocketClient Discord, IGuild Guild, RestGuildChannel Category);
