using Discord;
using Discord.Interactions;

namespace RaidBot;

public partial class RaidCommand
{
    [SlashCommand("create", "Creates a new raid signup.")]
    public async Task CreateAsync(
        [Summary("name", "The name of the raid. This value should only contain letters, numbers, and spaces.")] string name,
        [Summary("date", "The date of the raid. This value should use the server time.")] string dateString,
        [Summary("time", "The time of the raid. This value should use the server time.")] string timeString,
        [Summary("hidden", "If true, the signup will be hidden from everyone but you. Default is false.")] bool hidden = false)
    {
        if (name.Length > 93 || !name.All(c => c is (>= '0' and <= '9') or (>= 'a' and <= 'z') or (>= 'A' and <= 'Z') or ' '))
        {
            await RespondSilentAsync("The name parameter is not valid. Names can only contain letters, numbers, and spaces.");
            return;
        }

        if (!DateOnly.TryParse(dateString, out var date))
        {
            await RespondSilentAsync("The date parameter is not a valid date.");
            return;
        }

        if (!TimeOnly.TryParse(timeString, out var time))
        {
            await RespondSilentAsync("The time parameter is not a valid time of day.");
            return;
        }

        var dateTime = date.ToDateTime(time);
        var dateTimeOffset = new DateTimeOffset(dateTime, _timeZone.GetUtcOffset(dateTime));

        if (dateTimeOffset < DateTimeOffset.UtcNow)
        {
            await RespondSilentAsync("The date and time is in the past!");
            return;
        }

        await QueueTaskAsync(async () =>
        {
            var options = await _persistence.LoadAsync<GuildOptions>(Context.Guild.Id);

            if (options is null)
            {
                await RespondSilentAsync("The guild has not been configured yet.");
                return;
            }

            if (!((IGuildUser)Context.User).RoleIds.Contains(options.CreateRoleId))
            {
                await RespondSilentAsync("You do not have permission to use this command.");
                return;
            }

            var channels = await Context.Guild.GetChannelsAsync();
            var index = 0;
            foreach (var nextChannel in channels.OfType<ITextChannel>().Where(c => c.CategoryId == options.CategoryId).OrderBy(c => c.Position))
            {
                var raidContent = await _persistence.LoadAsync<RaidContent>(nextChannel.Id);

                if (raidContent is null || raidContent.Date < dateTimeOffset)
                {
                    index++;
                }
                else
                {
                    break;
                }
            }

            bool isToday = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, _timeZone).Date == dateTimeOffset.Date;

            var channel = await Context.Guild.CreateTextChannelAsync(
                $"{(isToday ? "⭐" : "")}{date:MMM-dd}-{name.Replace(' ', '-')}",
                c =>
                {
                    var overwrites = new List<Overwrite>();
                    c.PermissionOverwrites = overwrites;
                    c.CategoryId = options.CategoryId;
                    c.Position = index;

                    if (hidden)
                    {
                        overwrites.Add(new(Context.Guild.EveryoneRole.Id, PermissionTarget.Role, new(viewChannel: PermValue.Deny)));
                    }

                    overwrites.Add(new(Context.Client.CurrentUser.Id, PermissionTarget.User, new(useSlashCommands: PermValue.Allow, viewChannel: PermValue.Allow, sendMessages: PermValue.Allow, manageMessages: PermValue.Allow, useExternalEmojis: PermValue.Allow)));
                    overwrites.Add(new(Context.User.Id, PermissionTarget.User, new(useSlashCommands: PermValue.Allow, viewChannel: PermValue.Allow, sendMessages: PermValue.Allow, manageMessages: PermValue.Allow, useExternalEmojis: PermValue.Allow)));
                });

            // Set MessageId to 0 explicitly so that SaveAsync doesn't go searching for a declaration.
            await SaveAsync(channel, new RaidContent(name, dateTimeOffset, Context.User.Id) { MessageId = 0 });

            await channel.SendMessageAsync($"{Context.User.Mention}, please describe your rules here:", allowedMentions: new AllowedMentions { UserIds = new() { Context.User.Id } });

            await RespondSilentAsync($"Raid Created! <#{channel.Id}>");
        });
    }

    [ComponentInteraction("newraid:*", ignoreGroupNames: true)]
    public async Task NewRaidClickedAsync(string hideId)
    {
        var options = await _persistence.LoadAsync<GuildOptions>(Context.Guild.Id);

        if (options is null)
        {
            await RespondSilentAsync("The guild has not been configured yet.");
            return;
        }

        if (!((IGuildUser)Context.User).RoleIds.Contains(options.CreateRoleId))
        {
            await RespondSilentAsync("You do not have permission to use this command.");
            return;
        }

        await RespondWithModalAsync<NewRaidModal>("newraid_" + hideId);
    }

    [ModalInteraction("newraid_s", ignoreGroupNames: true)]
    public async Task NewRaidResponded(NewRaidModal modal)
    {
        await CreateAsync(modal.Name, modal.Date, modal.Time, false);
    }

    [ModalInteraction("newraid_h", ignoreGroupNames: true)]
    public async Task NewRaidRespondedHidden(NewRaidModal modal)
    {
        await CreateAsync(modal.Name, modal.Date, modal.Time, true);
    }
}
