using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using RaidBot.Data;

namespace RaidBot;

public class ExpansionAutocompleteHandler : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter,
        IServiceProvider services)
    {
        string? value = autocompleteInteraction.Data.Current.Value?.ToString();
        await using var db = await services.GetRequiredService<IDbContextFactory<ApplicationDbContext>>().CreateDbContextAsync();
        return AutocompletionResult.FromSuccess(await db.GuildExpansionConfigurations.AsNoTracking()
            .Where(config => config.GuildId == context.Guild.Id)
            .Where(config => string.IsNullOrEmpty(value) || config.Expansion.Name.Contains(value))
            .OrderBy(config => config.ExpansionId)
            .Select(config => new AutocompleteResult(config.Expansion.Name, config.ExpansionId))
            .ToListAsync());
    }
}
