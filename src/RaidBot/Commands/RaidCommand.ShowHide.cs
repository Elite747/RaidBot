using Discord;
using Discord.Interactions;
using RaidBot.Data;

namespace RaidBot;

public partial class RaidCommand
{
    [SlashCommand("hide", "Hides a raid signup channel. This command can only be used by the creator of a raid.")]
    public async Task HideAsync()
    {
        await QueueContentTaskAsync((_, raid) => SetHiddenAsync(raid, true));
    }

    [SlashCommand("show", "Shows a hidden raid signup channel. This command can only be used by the creator of a raid.")]
    public async Task ShowAsync()
    {
        await QueueContentTaskAsync((_, raid) => SetHiddenAsync(raid, false));
    }

    private async Task SetHiddenAsync(Raid raid, bool hidden)
    {
        if (Context.User.Id == raid.OwnerId)
        {
            var textChannel = (ITextChannel)Context.Channel;

            if (hidden)
            {
                await textChannel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, _everyoneHiddenPermissions);
                await RespondSilentAsync("Channel is now hidden!");
            }
            else
            {
                await textChannel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, _everyonePermissions);
                await RespondSilentAsync("Channel is now visible!");
            }
        }
        else
        {
            await RespondSilentAsync("You are not the owner of this channel.");
        }
    }
}
