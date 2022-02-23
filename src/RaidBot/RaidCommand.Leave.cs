using Discord.Interactions;

namespace RaidBot;

public partial class RaidCommand
{
    [SlashCommand("leave", "Leaves a raid signup.")]
    public Task SlashLeaveAsync() => LeaveAsync();

    [ComponentInteraction("raidleave", ignoreGroupNames: true)]
    public Task ClickLeaveAsync() => LeaveAsync();

    private async Task LeaveAsync()
    {
        await Context.Interaction.DeferAsync(true);
        _commandQueue.Queue(async () =>
        {
            if (await GetDeclarationAsync() is { } declarationMessage &&
                await ReadContentAsync() is { } raidContent)
            {
                if (raidContent.Members.RemoveAll(m => m.OwnerId == Context.User.Id) > 0)
                {
                    await SaveAsync(Context.Channel, raidContent, declarationMessage.Id);
                    await RespondSilentAsync("You've left the raid.");
                }
                else
                {
                    await RespondSilentAsync("You aren't in this raid.");
                }
            }
            else
            {
                await RespondSilentAsync("This channel is not a raid channel.");
            }
        });
    }
}
