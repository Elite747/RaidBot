using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.EntityFrameworkCore;
using RaidBot2.Data;
using RaidBot2.Discord.Tasks;

namespace RaidBot2.Commands;

[SlashCommandGroup("raid", "Commands for manipulating raid signups.")]
public class RaidCommand(IDiscordTaskQueue taskQueue, ApplicationDbContext db) : ApplicationCommandModule
{
    [SlashCommandPermissions(Permissions.ManageGuild)]
    [SlashCommand("configure", "Sets up the raid bot for this server. This can only be used by the server's owner.")]
    public async Task ConfigureAsync(InteractionContext context,
        [Option("expansion", "The expansion to configure.")][ChoiceProvider(typeof(ExpansionChoiceProvider))] long expansionId,
        [Option("timezone", "The timezone of the Warcraft server.")][Autocomplete(typeof(TimezoneAutocompleteProvider))] string timezone,
        [Option("category", "The category to add events to. If no category with this name exists, it will be created.")] string categoryName,
        [Option("role", "The role that a user must have to add an event.")] DiscordRole role)
    {
        await context.DeferAsync(true);
        taskQueue.Execute(async (_, ct) =>
        {
            var expansion = (await db.Expansions.FindAsync([(int)expansionId], cancellationToken: ct))!;

            var allChannels = await context.Guild.GetChannelsAsync();

            var category = allChannels.FirstOrDefault(c => c.Type == ChannelType.Category && string.Equals(c.Name, categoryName, StringComparison.OrdinalIgnoreCase));

            category ??= await context.Guild.CreateChannelCategoryAsync(categoryName);

            var guild = await db.GuildExpansionConfigurations.FirstOrDefaultAsync(g => g.GuildId == context.Guild.Id && g.ExpansionId == expansion.Id, ct);

            if (guild is null)
            {
                guild = new()
                {
                    GuildId = context.Guild.Id,
                    ExpansionId = expansion.Id,
                    CategoryId = category.Id,
                    CreateRoleId = role.Id,
                    Timezone = timezone
                };
                db.GuildExpansionConfigurations.Add(guild);
            }
            else
            {
                guild.CategoryId = category.Id;
                guild.CreateRoleId = role.Id;
            }

            await db.SaveChangesAsync(ct);

            await category.AddOverwriteAsync(context.Guild.CurrentMember, allow: Permissions.All);

            await context.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().WithContent("Configuration saved!").AsEphemeral());

            await context.Channel.SendMessageAsync(builder => builder.WithContent($"Start a new {expansion.Name} raid:").AddComponents(
                new DiscordButtonComponent(ButtonStyle.Primary, "newraid:s", "Start New Raid"),
                new DiscordButtonComponent(ButtonStyle.Secondary, "newraid:h", "Start Hidden Raid")
                ));
        });
    }
}
