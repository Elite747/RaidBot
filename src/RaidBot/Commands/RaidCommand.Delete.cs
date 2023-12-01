using Discord;
using Discord.Interactions;

namespace RaidBot;

public partial class RaidCommand
{
    [SlashCommand("delete", "Deletes a raid signup. This command can only be used by the raid or server owner.")]
    public async Task DeleteAsync()
    {
        await QueueContentTaskAsync(async (_, raid) =>
        {
            if (Context.User.Id == raid.OwnerId || Context.User.Id == Context.Guild.OwnerId)
            {
                await RespondSilentAsync("Deleting raid...");
                await ((ITextChannel)Context.Channel).DeleteAsync();
            }
            else
            {
                await RespondSilentAsync("You are not the owner of this raid channel.");
            }
        });
    }
}
