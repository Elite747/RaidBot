using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using System.Text;

namespace RaidBot;

[Group("raid", "Commands for manipulating raid signups.")]
public partial class RaidCommand : InteractionModuleBase
{
    private readonly IEventPersistence _persistence;
    private readonly TimeZoneInfo _timeZone;
    private readonly CommandQueue _commandQueue;
    private readonly DiscordConfigurationOptions _options;

    public RaidCommand(IEventPersistence persistence, IOptions<DiscordConfigurationOptions> options, TimeZoneInfo timeZone, CommandQueue commandQueue)
    {
        _persistence = persistence;
        _options = options.Value;
        _timeZone = timeZone;
        _commandQueue = commandQueue;
    }

    private Emote? GetEmote(Enum en) => GetEmote(en.ToString());

    private Emote? GetEmote(string key)
    {
        if (Emote.TryParse(FindRawEmote(key), out var emote))
        {
            return emote;
        }
        return null;
    }

    private string? FindRawEmote(Enum key) => FindRawEmote(key.ToString());

    private string? FindRawEmote(string key)
    {
        if (_options.Emoji.TryGetValue(key, out var rawValue))
        {
            return rawValue;
        }
        return null;
    }

    private async Task<RaidContent?> ReadContentAsync()
    {
        return await _persistence.LoadAsync<RaidContent>(Context.Channel.Id);
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

    private async Task SaveAsync(IMessageChannel channel, RaidContent raidContent, ulong messageId)
    {
        foreach (var member in raidContent.Members)
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

        await _persistence.SaveAsync(channel.Id, raidContent);

        if (messageId > 0)
        {
            await channel.ModifyMessageAsync(messageId, message =>
            {
                message.Content = MakeMessageContent(raidContent);
                message.Embed = MakeMessageEmbed(raidContent);
                message.Components = MakeMessageComponents();
            });
        }
        else
        {
            var message = await channel.SendMessageAsync(
                MakeMessageContent(raidContent),
                embed: MakeMessageEmbed(raidContent),
                components: MakeMessageComponents());
            await message.PinAsync();
        }
    }

    private Embed MakeMessageEmbed(RaidContent raidContent)
    {
        return new EmbedBuilder()
            .WithTitle(raidContent.Name)
            .AddField("Date (Server Time)", raidContent.Date.ToString("dddd, MMMM dd, yyyy hh:mm tt"), inline: true)
            .AddField("Date (Local Time)", $"<t:{raidContent.Date.ToUnixTimeSeconds()}:F>", inline: true)
            .AddField("Total Signups", raidContent.Members.Count.ToString("N0"))
            .AddField($"{FindRawEmote(PlayerRole.Tank)} Tanks ({raidContent.Members.Count(m => m.PlayerRole == PlayerRole.Tank)})", BuildField(raidContent, PlayerRole.Tank), inline: true)
            .AddField($"{FindRawEmote(PlayerRole.Healer)} Healers ({raidContent.Members.Count(m => m.PlayerRole == PlayerRole.Healer)})", BuildField(raidContent, PlayerRole.Healer), inline: true)
            .AddField($"{FindRawEmote(PlayerRole.Melee)} Melee DPS ({raidContent.Members.Count(m => m.PlayerRole == PlayerRole.Melee)})", BuildField(raidContent, PlayerRole.Melee), inline: true)
            .AddField($"{FindRawEmote(PlayerRole.Ranged)} Ranged DPS ({raidContent.Members.Count(m => m.PlayerRole == PlayerRole.Ranged)})", BuildField(raidContent, PlayerRole.Ranged), inline: true)
            .Build();
    }

    private static string MakeMessageContent(RaidContent raidContent)
    {
        return $"**{raidContent.Name}**\n*created by* <@!{raidContent.OwnerId}>";
    }

    private static MessageComponent MakeMessageComponents()
    {
        return new ComponentBuilder()
            .WithButton("Join or Update", "raidjoin", ButtonStyle.Primary)
            .WithButton("Leave", "raidleave", ButtonStyle.Danger)
            .Build();
    }

    private string BuildField(RaidContent raidContent, PlayerRole role)
    {
        var roleMembers = raidContent.Members.Where(member => member.PlayerRole == role);
        if (!roleMembers.Any())
        {
            return "none";
        }

        var sb = new StringBuilder();
        bool first = true;

        foreach (var member in roleMembers)
        {
            if (!first)
            {
                sb.Append('\n');
            }

            sb.Append('`')
                .Append(raidContent.Members.IndexOf(member) + 1)
                .Append("` ")
                .Append(FindRawEmote(member.PlayerClass))
                .Append(' ');

            bool hasName = member.Name?.Length > 0;

            if (hasName)
            {
                sb.Append(member.Name).Append(" (");
            }

            if (member.OwnerName?.Length > 0)
            {
                if (member.OwnerName.Length < 15)
                {
                    sb.Append(member.OwnerName);
                }
                else
                {
                    sb.Append(member.OwnerName.AsSpan()[..12].TrimEnd()).Append("...");
                }
            }
            else
            {
                sb.Append("<@!").Append(member.OwnerId).Append('>');
            }

            if (hasName)
            {
                sb.Append(')');
            }

            first = false;
        }

        return sb.ToString();
    }
}
