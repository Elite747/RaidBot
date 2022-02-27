using Discord;
using Discord.Interactions;

namespace RaidBot;

public partial class RaidCommand
{
    [SlashCommand("kick", "Removes a member from a raid signup. This command can only be used by the creator of a raid.")]
    public async Task KickAsync([Summary("user", "The discord user to remove.")] IUser user)
    {
        await Context.Interaction.DeferAsync(true);
        _commandQueue.Queue(async () =>
        {
            if (await ReadContentAsync() is { } raidContent)
            {
                if (Context.User.Id == raidContent.OwnerId)
                {
                    if (raidContent.Members.RemoveAll(m => m.OwnerId == user.Id) > 0)
                    {
                        await SaveAsync(Context.Channel, raidContent);
                        await RespondSilentAsync($"<@!{user.Id}> has been removed from this raid.");
                    }
                    else
                    {
                        await RespondSilentAsync($"<@!{user.Id}> is not in this raid.");
                    }
                }
                else
                {
                    await RespondSilentAsync("You are not the owner of this raid channel.");
                }
            }
            else
            {
                await RespondSilentAsync("This channel is not a raid channel.");
            }
        });
    }
}
