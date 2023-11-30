using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using RaidBot2.Discord.Tasks;

namespace RaidBot2.Commands;

[SlashCommandGroup("raid", "Commands for manipulating raid signups.")]
public class RaidCommand(IDiscordTaskQueue taskQueue, RaidCommandContext commands) : ApplicationCommandModule
{
    [SlashCommandPermissions(Permissions.ManageGuild)]
    [SlashCommand("configure", "Sets up the raid bot for this server. This can only be used by the server's owner.")]
    public Task ConfigureAsync(InteractionContext context,
        [Option("expansion", "The expansion to configure.")][ChoiceProvider(typeof(ExpansionChoiceProvider))] long expansionId,
        [Option("timezone", "The timezone of the Warcraft server.")][Autocomplete(typeof(TimezoneAutocompleteProvider))] string timezone,
        [Option("category", "The category to add events to. If no category with this name exists, it will be created.")] string categoryName,
        [Option("role", "The role that a user must have to add an event.")] DiscordRole role)
    {
        return ExecuteAsync(context, (_, ct) => commands.Configure.ExecuteAsync(context, (int)expansionId, timezone, categoryName, role, ct));
    }

    private async Task ExecuteAsync(InteractionContext context, Func<DiscordClient, CancellationToken, Task> task)
    {
        await context.DeferAsync(true);
        taskQueue.Execute(new DiscordTask(task, context));
    }
}
