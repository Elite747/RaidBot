using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace RaidBot2.Commands;

public class TimezoneChoiceProvider : IChoiceProvider
{
    public static Dictionary<string, TimeZoneInfo> Timezones { get; } = CreateTimezones();

    public Task<IEnumerable<DiscordApplicationCommandOptionChoice>> Provider()
    {
        return Task.FromResult(Timezones.Select(tz => new DiscordApplicationCommandOptionChoice(tz.Value.DisplayName, tz.Key)));
    }

    private static Dictionary<string, TimeZoneInfo> CreateTimezones()
    {
        var results = new Dictionary<string, TimeZoneInfo>();
        foreach (var timezone in TimeZoneInfo.GetSystemTimeZones())
        {
            results[timezone.Id.Replace("/", "-")] = timezone;
        }
        return results;
    }
}

public class TimezoneAutocompleteProvider : IAutocompleteProvider
{
    public Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx)
    {
        return Task.FromResult(GetChoices(ctx.OptionValue?.ToString()));
    }

    private static IEnumerable<DiscordAutoCompleteChoice> GetChoices(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return [];
        }

        return TimeZoneInfo.GetSystemTimeZones()
            .Where(timezone => timezone.Id.Contains(value, StringComparison.OrdinalIgnoreCase) || timezone.DisplayName.Contains(value, StringComparison.OrdinalIgnoreCase))
            .OrderBy(timezone => timezone.DisplayName)
            .Take(10)
            .Select(timezone => new DiscordAutoCompleteChoice(timezone.DisplayName, timezone.Id))
            .ToList();
    }
}
