using DSharpPlus;
using DSharpPlus.Entities;

namespace RaidBot2.PeriodicTasks;

public interface IGuildExpansionTask
{
    Task ExecuteAsync(GuildExpansionContext context, CancellationToken cancellationToken = default);
}

public readonly record struct GuildExpansionContext(int ConfigurationId, int ExpansionId, DiscordClient Discord, DiscordGuild Guild, DiscordChannel Category);
