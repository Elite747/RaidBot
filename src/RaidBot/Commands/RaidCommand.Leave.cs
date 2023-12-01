using Discord.Interactions;

namespace RaidBot;

public partial class RaidCommand
{
    [SlashCommand("leave", "Leaves a raid signup.")]
    public Task SlashLeaveAsync()
    {
        return LeaveAsync();
    }

    [ComponentInteraction("raidleave", ignoreGroupNames: true)]
    public Task ClickLeaveAsync()
    {
        return LeaveAsync();
    }

    private async Task LeaveAsync()
    {
        await QueueContentTaskAsync(async (db, raid) =>
        {
            await db.Entry(raid).Collection(r => r.Members).LoadAsync();
            bool removed = false;
            foreach (var member in raid.Members.ToList())
            {
                if (member.OwnerId == Context.User.Id)
                {
                    raid.Members.Remove(member);
                    db.RaidMembers.Remove(member);
                    removed = true;
                }
            }
            if (removed)
            {
                await SaveAsync(db, Context.Channel, raid);
                await RespondSilentAsync("You've left the raid.");
            }
            else
            {
                await RespondSilentAsync("You aren't in this raid.");
            }
        });
    }
}
