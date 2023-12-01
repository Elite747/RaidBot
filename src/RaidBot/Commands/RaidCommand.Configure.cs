using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;

namespace RaidBot;

public partial class RaidCommand
{
    [SlashCommand("configure", "Sets up the raid bot for this server. This can only be used by the server's owner.")]
    public async Task ConfigureAsync(
        [Summary("expansion", "The expansion to configure.")][Autocomplete(typeof(AllExpansionsAutocompleteHandler))] long expansionId,
        [Summary("timezone", "The timezone of the Warcraft server.")][Autocomplete(typeof(TimezoneAutocompleteHandler))] string timezone,
        [Summary("category", "The category to add events to. If no category with this name exists, it will be created.")] string categoryName,
        [Summary("role", "The role that a user must have to add an event.")] IRole role)
    {
        if (Context.User.Id != Context.Guild.OwnerId)
        {
            await RespondSilentAsync("Only the server owner can execute this command!");
        }
        else
        {
            await QueueTaskAsync(async db =>
            {
                var expansion = (await db.Expansions.FindAsync([(int)expansionId]))!;

                var allChannels = await Context.Guild.GetChannelsAsync();

                var category = (await Context.Guild.GetCategoriesAsync()).FirstOrDefault(c => string.Equals(c.Name, categoryName, StringComparison.OrdinalIgnoreCase))
                    ?? await Context.Guild.CreateCategoryAsync(categoryName);

                var guild = await db.GuildExpansionConfigurations.FirstOrDefaultAsync(g => g.GuildId == Context.Guild.Id && g.ExpansionId == expansion.Id);

                if (guild is null)
                {
                    guild = new()
                    {
                        GuildId = Context.Guild.Id,
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

                await db.SaveChangesAsync();

                await category.AddPermissionOverwriteAsync(Context.Client.CurrentUser, OverwritePermissions.AllowAll(category));

                await RespondSilentAsync("Configuration saved!");

                await Context.Channel.SendMessageAsync("Raids:", components: new ComponentBuilder()
                    .WithButton("Start A New Raid", $"newraid:{expansionId}:{false}", row: 0)
                    .WithButton("Start A Hidden Raid", $"newraid:{expansionId}:{true}", ButtonStyle.Secondary, row: 1)
                    .Build());
            });
        }
    }
}
