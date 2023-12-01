using System.Text;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RaidBot.Data;
using RaidBot.Discord;
using RaidBot.Discord.Tasks;

namespace RaidBot;

[Group("raid", "Commands for manipulating raid signups.")]
public partial class RaidCommand(
    IDbContextFactory<ApplicationDbContext> dbContext,
    IOptions<DiscordConfigurationOptions> options,
    IDiscordTaskQueue taskQueue) : InteractionModuleBase
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContext = dbContext;
    private readonly IDiscordTaskQueue _taskQueue = taskQueue;
    private readonly DiscordConfigurationOptions _options = options.Value;

    private static Emote? GetEmote(string key)
    {
        if (Emote.TryParse(key, out var emote))
        {
            return emote;
        }
        return null;
    }

    private async Task QueueTaskAsync(Func<ApplicationDbContext, Task> task, bool defer = true)
    {
        if (defer)
        {
            await Context.Interaction.DeferAsync(true);
        }
        _taskQueue.Execute(new DiscordTask(async (_, ct) =>
        {
            try
            {
                await using var db = await _dbContext.CreateDbContextAsync(ct);
                await task(db);
            }
            catch
            {
                await RespondSilentAsync(":x: Something went wrong...");
                throw;
            }
        }, Context));
    }

    private Task QueueContentTaskAsync(Func<ApplicationDbContext, Raid, Task> task)
    {
        return QueueTaskAsync(async db =>
        {
            if (await ReadContentAsync(db) is { } raidContent)
            {
                await task(db, raidContent);
            }
            else
            {
                await RespondSilentAsync("This channel is not a raid channel.");
            }
        });
    }

    private async Task<Raid?> ReadContentAsync(ApplicationDbContext db)
    {
        return await db.Raids.Include(raid => raid.Configuration).FirstOrDefaultAsync(raid => raid.ChannelId == Context.Channel.Id);
    }

    private async Task<IUserMessage?> GetDeclarationAsync()
    {
        if (Context.Interaction is SocketMessageComponent msg && msg.Message.Author.Id == Context.Client.CurrentUser.Id && msg.Message.IsPinned)
        {
            return msg.Message;
        }
        else
        {
            var pinned = await Context.Channel.GetPinnedMessagesAsync();
            if (pinned.FirstOrDefault(msg => msg.Author.Id == Context.Client.CurrentUser.Id) is IUserMessage declarationMessage)
            {
                return declarationMessage;
            }
        }

        return null;
    }

    private async Task RespondSilentAsync(string message, MessageComponent? components = null)
    {
        if (Context.Interaction.HasResponded)
        {
            await Context.Interaction.FollowupAsync(message, ephemeral: true, components: components);
        }
        else
        {
            await Context.Interaction.RespondAsync(message, ephemeral: true, components: components);
        }
    }

    private async Task SaveAsync(ApplicationDbContext db, IMessageChannel channel, Raid raid)
    {
        if (raid.Id is not 0)
        {
            await db.Entry(raid).Reference(r => r.Configuration).LoadAsync();
            await db.Entry(raid).Collection(r => r.Members).LoadAsync();
        }
        else
        {
            db.Raids.Add(raid);
        }

        foreach (var member in raid.Members)
        {
            if (string.IsNullOrEmpty(member.OwnerName))
            {
                var user = await Context.Guild.GetUserAsync(member.OwnerId);

                if (user is not null)
                {
                    member.OwnerName = user.DisplayName;
                }
            }
        }

        var classes = await db.ExpansionClasses
            .Where(ec => ec.ExpansionId == raid.Configuration.ExpansionId)
            .Select(ec => ec.Class)
            .ToDictionaryAsync(c => c.Id);
        var roles = await db.Roles.OrderBy(x => x.Id).ToListAsync();

        var raidDate = TimeZoneHelpers.ConvertTimeToLocal(raid.Date, raid.Configuration.Timezone, isUtc: true);

        if (raid.MessageId > 0)
        {
            await channel.ModifyMessageAsync(raid.MessageId, message =>
            {
                message.Content = MakeMessageContent(raid.OwnerId);
                message.Embed = MakeMessageEmbed(raid.Name, raidDate, raid.Members, classes, roles);
                message.Components = MakeMessageComponents();
                message.AllowedMentions = new AllowedMentions { UserIds = [Context.User.Id] };
            });
        }
        else
        {
            var message = await channel.SendMessageAsync(
                MakeMessageContent(raid.OwnerId),
                embed: MakeMessageEmbed(raid.Name, raidDate, raid.Members, classes, roles),
                allowedMentions: new AllowedMentions { UserIds = [Context.User.Id] },
                components: MakeMessageComponents());
            await message.PinAsync();
            raid.MessageId = message.Id;
        }

        await db.SaveChangesAsync();
    }

    private static Embed MakeMessageEmbed(string name, DateTimeOffset date, List<RaidMember> members, Dictionary<int, PlayerClass> classes, List<PlayerRole> roles)
    {
        var builder = new EmbedBuilder()
            .WithTitle(name)
            .AddField("Date (Server Time)", date.ToString("dddd, MMMM dd, yyyy hh:mm tt"), inline: true)
            .AddField("Date (Local Time)", $"<t:{date.ToUnixTimeSeconds()}:F>", inline: true)
            .AddField("Total Signups", members.Count.ToString("N0"), inline: false);

        var sb = new StringBuilder();

        foreach (var role in roles)
        {
            AddField(builder, sb, members, classes, role);
        }

        return builder.Build();
    }

    private static string MakeMessageContent(ulong ownerId)
    {
        return $"""
            <@!{ownerId}>, your raid will automatically delete 48 hours after the start time.
            You can manually add `/raid add @user` or remove `/raid kick @user` users.
            You can change this event with `/raid update`.
            """;
    }

    private static MessageComponent MakeMessageComponents()
    {
        return new ComponentBuilder()
            .WithButton("Join or Update", "raidjoin", ButtonStyle.Primary)
            .WithButton("Leave", "raidleave", ButtonStyle.Danger)
            .Build();
    }

    private static void AddField(EmbedBuilder builder, StringBuilder sb, List<RaidMember> members, Dictionary<int, PlayerClass> classes, PlayerRole role)
    {
        var fieldName = $"{role.Icon} {role.Name} ({members.Count(m => m.RoleId == role.Id)})";
        var roleMembers = members.Where(m => m.RoleId == role.Id);

        if (!roleMembers.Any())
        {
            builder.AddField(fieldName, "none", inline: true);
            return;
        }

        bool first = true;
        sb.Length = 0;

        foreach (var member in roleMembers)
        {
            if (!first)
            {
                sb.Append('\n');
            }

            sb.Append('`')
                .Append(members.IndexOf(member) + 1)
                .Append("` ")
                .Append(classes[member.ClassId].Icon)
                .Append(' ');

            if (member.Name?.Length > 0)
            {
                sb.Append("**").Append(char.ToUpper(member.Name[0]));

                for (int i = 1; i < member.Name.Length; i++)
                {
                    sb.Append(char.ToLower(member.Name[i]));
                }

                sb.Append("**");

                if (member.OwnerName?.Length > 0)
                {
                    if (!member.OwnerName.Contains(member.Name, StringComparison.CurrentCultureIgnoreCase))
                    {
                        sb.Append(" (").AppendTruncated(member.OwnerName, 15).Append(')');
                    }
                }
                else
                {
                    sb.Append(" (<@!").Append(member.OwnerId).Append(">)");
                }
            }
            else if (member.OwnerName?.Length > 0)
            {
                sb.Append("**").AppendTruncated(member.OwnerName, 15).Append("**");
            }
            else
            {
                sb.Append("<@!").Append(member.OwnerId).Append('>');
            }

            first = false;
        }

        builder.AddField(fieldName, sb.ToString(), inline: true);
    }

    private readonly OverwritePermissions _everyonePermissions = new(
        addReactions: PermValue.Allow,
        viewChannel: PermValue.Allow,
        sendMessages: PermValue.Allow,
        embedLinks: PermValue.Allow,
        attachFiles: PermValue.Allow,
        readMessageHistory: PermValue.Allow,
        useExternalEmojis: PermValue.Allow,
        useSlashCommands: PermValue.Allow,
        useApplicationCommands: PermValue.Allow,
        useExternalStickers: PermValue.Allow);

    private readonly OverwritePermissions _everyoneHiddenPermissions = new(viewChannel: PermValue.Deny);

    private readonly OverwritePermissions _ownerPermissions = new(
        addReactions: PermValue.Allow,
        viewChannel: PermValue.Allow,
        sendMessages: PermValue.Allow,
        manageMessages: PermValue.Allow,
        attachFiles: PermValue.Allow,
        readMessageHistory: PermValue.Allow,
        useExternalEmojis: PermValue.Allow,
        useSlashCommands: PermValue.Allow,
        useApplicationCommands: PermValue.Allow,
        useExternalStickers: PermValue.Allow);
}
