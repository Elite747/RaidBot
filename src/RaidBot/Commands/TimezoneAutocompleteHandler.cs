using Discord;
using Discord.Interactions;

namespace RaidBot;

public class TimezoneAutocompleteHandler : AutocompleteHandler
{
    private static List<AutocompleteResult> GetChoices(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return [];
        }

        return TimeZoneInfo.GetSystemTimeZones()
            .Where(timezone => timezone.Id.Contains(value, StringComparison.OrdinalIgnoreCase) || timezone.DisplayName.Contains(value, StringComparison.OrdinalIgnoreCase))
            .OrderBy(timezone => timezone.DisplayName)
            .Take(10)
            .Select(timezone => new AutocompleteResult(timezone.DisplayName, timezone.Id))
            .ToList();
    }

    public override Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
    {
        return Task.FromResult(AutocompletionResult.FromSuccess(GetChoices(autocompleteInteraction.Data.Current.Value?.ToString())));
    }
}
