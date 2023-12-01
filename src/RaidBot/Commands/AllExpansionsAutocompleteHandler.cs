using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using RaidBot.Data;

namespace RaidBot;

public class AllExpansionsAutocompleteHandler : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter,
        IServiceProvider services)
    {
        string? value = autocompleteInteraction.Data.Current.Value?.ToString();
        await using var db = await services.GetRequiredService<IDbContextFactory<ApplicationDbContext>>().CreateDbContextAsync();
        return AutocompletionResult.FromSuccess(await db.Expansions.AsNoTracking()
            .Where(e => string.IsNullOrEmpty(value) || e.Name.Contains(value))
            .OrderBy(e => e.Id)
            .Select(e => new AutocompleteResult(e.Name, e.Id))
            .ToListAsync());
    }
}
