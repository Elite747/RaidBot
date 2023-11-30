using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.EntityFrameworkCore;
using RaidBot2.Data;

namespace RaidBot2.Commands;

public class ExpansionChoiceProvider : IChoiceProvider
{
    /// <summary>
    /// IChoiceProvider doesn't support DI. :/
    /// </summary>
    public static IServiceProvider ServiceProvider { get; set; } = null!;

    public async Task<IEnumerable<DiscordApplicationCommandOptionChoice>> Provider()
    {
        return await ServiceProvider.GetRequiredService<ApplicationDbContext>().Expansions.AsNoTracking()
            .OrderBy(e => e.Name)
            .Select(e => new DiscordApplicationCommandOptionChoice(e.Name, e.Id))
            .ToListAsync();
    }
}
