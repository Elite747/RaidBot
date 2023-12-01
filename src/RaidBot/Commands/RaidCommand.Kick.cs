using Discord;
using Discord.Interactions;

namespace RaidBot;

public partial class RaidCommand
{
    [SlashCommand("kick", "Removes a member from a raid signup. This command can only be used by the creator of a raid.")]
    public async Task KickAsync([Summary("user", "The discord user to remove.")] IUser user)
    {
        await QueueContentTaskAsync(async (db, raid) =>
        {
            if (Context.User.Id == raid.OwnerId)
            {
                await db.Entry(raid).Collection(r => r.Members).LoadAsync();
                bool removed = false;
                foreach (var member in raid.Members.ToList())
                {
                    if (member.OwnerId == user.Id)
                    {
                        raid.Members.Remove(member);
                        db.RaidMembers.Remove(member);
                        removed = true;
                    }
                }
                if (removed)
                {
                    await SaveAsync(db, Context.Channel, raid);
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
        });
    }
}
