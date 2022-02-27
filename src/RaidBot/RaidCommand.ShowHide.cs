using Discord;
using Discord.Interactions;

namespace RaidBot;

public partial class RaidCommand
{
    [SlashCommand("hide", "Hides a raid signup channel. This command can only be used by the creator of a raid.")]
    public async Task HideAsync()
    {
        await Context.Interaction.DeferAsync(true);
        _commandQueue.Queue(() => SetHiddenAsync(true));
    }

    [SlashCommand("show", "Shows a hidden raid signup channel. This command can only be used by the creator of a raid.")]
    public async Task ShowAsync()
    {
        await Context.Interaction.DeferAsync(true);
        _commandQueue.Queue(() => SetHiddenAsync(false));
    }

    private async Task SetHiddenAsync(bool hidden)
    {
        if (await ReadContentAsync() is { } raidContent)
        {
            if (Context.User.Id == raidContent.OwnerId)
            {
                var options = await _persistence.LoadAsync<GuildOptions>(Context.Guild.Id);

                if (options is null)
                {
                    await RespondSilentAsync("This guild has not been configured yet.");
                    return;
                }

                var textChannel = (ITextChannel)Context.Channel;
                if (!hidden)
                {
                    await textChannel.RemovePermissionOverwriteAsync(Context.Guild.EveryoneRole);
                    await RespondSilentAsync("Channel is now visible!");
                }
                else
                {
                    await textChannel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, new(viewChannel: PermValue.Deny));
                    await RespondSilentAsync("Channel is now hidden!");
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
    }
}
