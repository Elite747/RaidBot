using Discord;
using Discord.Interactions;

namespace RaidBot;

public partial class RaidCommand
{
    [SlashCommand("add", "Adds a member to a raid signup. This command can only be used by the creator of a raid.")]
    public async Task AddAsync(
        [Summary("user", "The discord user of this character.")] IUser user,
        [Summary("class", "The class of the character.")] PlayerClass playerClass,
        [Summary("role", "The role they will be performing.")] PlayerRole playerRole)
    {
        await Context.Interaction.DeferAsync(true);
        _commandQueue.Queue(async () =>
        {
            if (await GetDeclarationAsync() is { } declarationMessage &&
            await ReadContentAsync() is { } raidContent)
            {
                if (Context.User.Id == raidContent.OwnerId)
                {
                    await AddInternalAsync(declarationMessage, raidContent, playerClass, playerRole, user.Id);
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

    [SlashCommand("join", "Joins a raid signup.")]
    public async Task JoinAsync(
        [Summary("class", "The class of your character.")] PlayerClass playerClass,
        [Summary("role", "The role you will be performing.")] PlayerRole playerRole)
    {
        await Context.Interaction.DeferAsync(true);
        _commandQueue.Queue(async () =>
        {
            if (await GetDeclarationAsync() is { } declarationMessage &&
                await ReadContentAsync() is { } raidContent)
            {
                await AddInternalAsync(declarationMessage, raidContent, playerClass, playerRole, Context.User.Id);
            }
            else
            {
                await RespondSilentAsync("This channel is not a raid channel.");
            }
        });
    }

    [ComponentInteraction("raidjoin:*", ignoreGroupNames: true)]
    public async Task JoinClickAsync(string roleString)
    {
        var playerRole = Enum.Parse<PlayerRole>(roleString);
        await Context.Interaction.DeferAsync(true);

        _commandQueue.Queue(async () =>
        {
            if (await GetDeclarationAsync() is { } declarationMessage &&
                await ReadContentAsync() is { } raidContent)
            {
                await RespondSilentAsync("Select Your Class", new ComponentBuilder()
                    .WithSelectMenu(
                        $"raidjoin_class:{playerRole}",
                        Enum.GetValues<PlayerClass>().Select(c => new SelectMenuOptionBuilder().WithLabel(c.ToString()).WithValue(c.ToString()).WithEmote(GetEmote(c))).ToList(),
                        "Select your class")
                    .Build());
            }
            else
            {
                await RespondSilentAsync("This channel is not a raid channel.");
            }
        });
    }

    [ComponentInteraction("raidjoin_class:*", ignoreGroupNames: true)]
    public async Task JoinRespondAsync(string roleString, string[] selection)
    {
        var playerRole = Enum.Parse<PlayerRole>(roleString);
        var playerClass = Enum.Parse<PlayerClass>(selection[0]);
        await Context.Interaction.DeferAsync(true);

        _commandQueue.Queue(async () =>
        {
            if (await GetDeclarationAsync() is { } declarationMessage &&
                await ReadContentAsync() is { } raidContent)
            {
                await AddInternalAsync(declarationMessage, raidContent, playerClass, playerRole, Context.User.Id);
            }
            else
            {
                await RespondSilentAsync("This channel is not a raid channel.");
            }
        });
    }

    private async Task AddInternalAsync(
        IUserMessage declarationMessage,
        RaidContent raidContent,
        PlayerClass playerClass,
        PlayerRole playerRole,
        ulong userId)
    {
        var existing = raidContent.Members.Find(m => m.OwnerId == userId);

        if (existing is not null)
        {
            existing.PlayerClass = playerClass;
            existing.PlayerRole = playerRole;
            raidContent.Members.RemoveAll(m => m != existing && m.OwnerId == userId);
        }
        else
        {
            raidContent.Members.Add(new(playerClass, playerRole, userId));
        }

        await SaveAsync(Context.Channel, raidContent, declarationMessage.Id);

        string message = $"{FindRawEmote(playerClass)} <@!{userId}> {(existing is not null ? "updated." : "added.")}";

        if (Context.Interaction.Type != InteractionType.MessageComponent)
        {
            await RespondSilentAsync(message);
        }
    }
}
