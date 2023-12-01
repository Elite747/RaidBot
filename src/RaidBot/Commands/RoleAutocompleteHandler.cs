using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using RaidBot.Data;

namespace RaidBot;

public class RoleAutocompleteHandler : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter,
        IServiceProvider services)
    {
        string? value = autocompleteInteraction.Data.Current.Value?.ToString();
        await using var db = await services.GetRequiredService<IDbContextFactory<ApplicationDbContext>>().CreateDbContextAsync();

        return AutocompletionResult.FromSuccess(await db.Roles.AsNoTracking()
            .OrderBy(role => role.Id)
            .Where(role => string.IsNullOrEmpty(value) || role.Name.Contains(value))
            .Select(role => new AutocompleteResult(role.Name, role.Id))
            .ToListAsync());
    }
}
