using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using RaidBot.Data;

namespace RaidBot;

public class ClassAutocompleteHandler : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter,
        IServiceProvider services)
    {
        string? value = autocompleteInteraction.Data.Current.Value?.ToString();
        await using var db = await services.GetRequiredService<IDbContextFactory<ApplicationDbContext>>().CreateDbContextAsync();

        var expansionId = await db.Raids.AsNoTracking()
            .Where(x => x.ChannelId == context.Channel.Id)
            .Where(x => string.IsNullOrWhiteSpace(value) || x.Name.Contains(value))
            .Select(x => x.Configuration.ExpansionId)
            .FirstOrDefaultAsync();

        if (expansionId is 0)
        {
            return AutocompletionResult.FromError(InteractionCommandError.Unsuccessful, "Interaction can only be used in a raid channel.");
        }

        return AutocompletionResult.FromSuccess(await db.ExpansionClasses.AsNoTracking()
            .Where(ec => ec.ExpansionId == expansionId)
            .OrderBy(ec => ec.Class.Name)
            .Select(ec => new AutocompleteResult(ec.Class.Name, ec.ClassId))
            .ToListAsync());
    }
}
