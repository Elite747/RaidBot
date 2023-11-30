using Discord;
using Discord.Interactions;

namespace RaidBot;

public partial class RaidCommand
{
    [SlashCommand("configure", "Sets up the raid bot for this server. This can only be used by the server's owner.")]
    public async Task ConfigureAsync(
        [Summary("category", "The category to add events to. If no category with this name exists, it will be created.")] string categoryName,
        [Summary("role", "The role that a user must have to add an event.")] IRole role)
    {
        if (Context.User.Id != Context.Guild.OwnerId)
        {
            await RespondSilentAsync("Only the server owner can execute this command!");
        }
        else
        {
            await QueueTaskAsync(async () =>
            {
                var category = (await Context.Guild.GetCategoriesAsync()).FirstOrDefault(c => string.Equals(c.Name, categoryName, StringComparison.OrdinalIgnoreCase))
                    ?? await Context.Guild.CreateCategoryAsync(categoryName);

                await _persistence.SaveAsync(Context.Guild.Id, new GuildOptions
                {
                    GuildId = Context.Guild.Id,
                    CategoryId = category.Id,
                    CreateRoleId = role.Id
                });

                await category.AddPermissionOverwriteAsync(Context.Client.CurrentUser, OverwritePermissions.AllowAll(category));

                await RespondSilentAsync("Configuration saved!");

                await Context.Channel.SendMessageAsync("Raids:", components: new ComponentBuilder()
                    .WithButton("Start A New Raid", "newraid:s", row: 0)
                    .WithButton("Start A Hidden Raid", "newraid:h", ButtonStyle.Secondary, row: 1)
                    .Build());
            });
        }
    }
}
