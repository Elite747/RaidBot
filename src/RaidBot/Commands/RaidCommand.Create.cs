using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using RaidBot.Data;

namespace RaidBot;

public partial class RaidCommand
{
    [SlashCommand("create", "Creates a new raid signup.")]
    public async Task CreateAsync(
        [Summary("expansion", "The expansion for the raid."), Autocomplete(typeof(ExpansionAutocompleteHandler))] long expansionId,
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

        await QueueTaskAsync(async db =>
        {
            var options = await db.GuildExpansionConfigurations.FirstOrDefaultAsync(x => x.ExpansionId == expansionId && x.GuildId == Context.Guild.Id);

            if (options is null)
            {
                await RespondSilentAsync("The guild has not been configured for that expansion yet.");
                return;
            }

            if (!((IGuildUser)Context.User).RoleIds.Contains(options.CreateRoleId))
            {
                await RespondSilentAsync("You do not have permission to use this command.");
                return;
            }

            var dateTimeOffset = TimeZoneHelpers.ConvertTimeToLocal(date.ToDateTime(time), options.Timezone, isUtc: false);

            if (dateTimeOffset < DateTimeOffset.UtcNow)
            {
                await RespondSilentAsync("The date and time is in the past!");
                return;
            }

            var channels = await Context.Guild.GetChannelsAsync();
            var eventChannels = new List<(Raid?, IGuildChannel)>();
            foreach (var otherChannel in channels.Where(c => c is INestedChannel nested && nested.CategoryId == options.CategoryId))
            {
                var raid = await db.Raids.FirstOrDefaultAsync(x => x.ChannelId == otherChannel.Id);

                if (raid is null || raid.Date >= DateTimeOffset.UtcNow.AddDays(-2))
                {
                    eventChannels.Add((raid, otherChannel));
                }
            }

            int index = (channels.FirstOrDefault(c => c.Id == options.CategoryId)?.Position ?? 0) + 1;
            int? targetIndex = null;
            foreach ((Raid? raid, IGuildChannel otherChannel) in eventChannels.OrderBy(t => t.Item1?.Date).ThenBy(t => t.Item2.Position))
            {
                if (raid is not null && raid.Date >= dateTimeOffset)
                {
                    targetIndex ??= index;
                    index++;
                }
                if (otherChannel.Position != index)
                {
                    int thisIndex = index;
                    await otherChannel.ModifyAsync(c => c.Position = thisIndex);
                }
                index++;
            }

            bool isToday = TimeZoneHelpers.ConvertTimeToLocal(DateTime.UtcNow, options.Timezone, isUtc: true).Date == dateTimeOffset.Date;

            var channel = await Context.Guild.CreateTextChannelAsync(
                $"{(isToday ? "⭐" : "")}{date:MMM-dd}-{name.Replace(' ', '-')}",
                c =>
                {
                    c.PermissionOverwrites = new List<Overwrite>
                    {
                        new(Context.Guild.EveryoneRole.Id, PermissionTarget.Role, hidden ? _everyoneHiddenPermissions : _everyonePermissions),
                        new(Context.Client.CurrentUser.Id, PermissionTarget.User, _ownerPermissions),
                        new(Context.User.Id, PermissionTarget.User, _ownerPermissions)
                    };
                    c.CategoryId = options.CategoryId;
                    c.Position = targetIndex ?? index;
                });

            await SaveAsync(db, channel, new Raid
            {
                ChannelId = channel.Id,
                Configuration = options,
                ConfigurationId = options.Id,
                Date = dateTimeOffset.UtcDateTime,
                Name = name,
                OwnerId = Context.User.Id,
                MessageId = 0
            });

            await channel.SendMessageAsync($"{Context.User.Mention}, please describe your rules here:", allowedMentions: new AllowedMentions { UserIds = [Context.User.Id] });

            await RespondSilentAsync($"Raid Created! <#{channel.Id}>");
        });
    }

    private bool TryParseNewraidParameters(ReadOnlySpan<char> parameters, out int expansionId, out bool hide)
    {
        var en = new DelimitEnumerator(parameters, [':']);
        if (en.MoveNext()
            && int.TryParse(en.Current, out expansionId)
            && en.MoveNext()
            && bool.TryParse(en.Current, out hide))
        {
            return true;
        }
        hide = false;
        expansionId = 0;
        return false;
    }

    [ComponentInteraction("newraid:*", ignoreGroupNames: true)]
    public async Task NewRaidClickedAsync(string customId)
    {
        if (!TryParseNewraidParameters(customId, out int expansionId, out bool hide))
        {
            await RespondSilentAsync("Invalid command.");
            return;
        }
        var parameters = customId.Split(':');
        await using var db = await _dbContext.CreateDbContextAsync();
        var options = await db.GuildExpansionConfigurations.AsNoTracking()
            .Where(x => x.GuildId == Context.Guild.Id && x.ExpansionId == expansionId)
            .FirstOrDefaultAsync();

        if (options is null)
        {
            await RespondSilentAsync("This guild has not been configured for that expansion.");
            return;
        }

        if (!((IGuildUser)Context.User).RoleIds.Contains(options.CreateRoleId))
        {
            await RespondSilentAsync("You do not have permission to use this command.");
            return;
        }

        await RespondWithModalAsync<NewRaidModal>($"newraid_{customId}");
    }

    [ModalInteraction("newraid_*", ignoreGroupNames: true)]
    public async Task NewRaidResponded(string customId, NewRaidModal modal)
    {
        if (!TryParseNewraidParameters(customId, out int expansionId, out bool hide))
        {
            await RespondSilentAsync("Invalid command.");
            return;
        }

        await CreateAsync(expansionId, modal.Name, modal.Date, modal.Time, hide);
    }

    [ModalInteraction("newraid_h", ignoreGroupNames: true)]
    public async Task NewRaidRespondedHidden(NewRaidModal modal)
    {
        await using var db = await _dbContext.CreateDbContextAsync();
        var options = await db.GuildExpansionConfigurations.AsNoTracking()
            .Where(x => x.GuildId == Context.Guild.Id && x.CategoryId == Context.Channel.Id)
            .FirstOrDefaultAsync();

        if (options is null)
        {
            await RespondSilentAsync("This command is only usable within a raid creation channel.");
            return;
        }

        await CreateAsync(options.ExpansionId, modal.Name, modal.Date, modal.Time, true);
    }
}
