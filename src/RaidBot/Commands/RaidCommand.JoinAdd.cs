using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using RaidBot.Data;

namespace RaidBot;

public partial class RaidCommand
{
    [SlashCommand("add", "Adds a member to a raid signup. This command can only be used by the creator of a raid.")]
    public async Task AddAsync(
        [Summary("user", "The discord user of this character.")] IUser user,
        [Summary("class", "The class of the character."), Autocomplete(typeof(ClassAutocompleteHandler))] int playerClassId,
        [Summary("role", "The role they will be performing."), Autocomplete(typeof(RoleAutocompleteHandler))] int playerRoleId,
        [Summary("name", "The optional character name.")] string? name = null)
    {
        await QueueContentTaskAsync((db, raid) => Context.User.Id == raid.OwnerId ?
            AddInternalAsync(db, raid, playerClassId, playerRoleId, user, name) :
            RespondSilentAsync("You are not the owner of this raid channel."));
    }

    [SlashCommand("join", "Joins a raid signup.")]
    public async Task JoinAsync(
        [Summary("class", "The class of your character."), Autocomplete(typeof(ClassAutocompleteHandler))] int playerClassId,
        [Summary("role", "The role you will be performing."), Autocomplete(typeof(RoleAutocompleteHandler))] int playerRoleId,
        [Summary("name", "The optional character name.")] string? name = null)
    {
        await QueueContentTaskAsync((db, raid) => AddInternalAsync(db, raid, playerClassId, playerRoleId, Context.User, name));
    }

    [ComponentInteraction("raidjoin", ignoreGroupNames: true)]
    public async Task JoinClick2Async()
    {
        await QueueTaskAsync(async db =>
        {
            if (await ReadContentAsync(db) is { } raidContent)
            {
                var currentInfo = await db.RaidMembers.AsNoTracking()
                    .Where(m => m.RaidId == raidContent.Id && m.OwnerId == Context.User.Id)
                    .Select(m => new { Role = m.Role.Name, Class = m.Class.Name, m.Name })
                    .FirstOrDefaultAsync();

                await RespondWithModalAsync(new ModalBuilder()
                    .WithTitle("Join Raid")
                    .WithCustomId("raidjoin_respond")
                    .AddTextInput("Role", "raidjoin_role", placeholder: "tank, healer, ranged, melee", maxLength: 10, required: true, value: currentInfo?.Role)
                    .AddTextInput("Class", "raidjoin_class", placeholder: "druid, hunter, mage, etc...", maxLength: 12, required: true, value: currentInfo?.Class)
                    .AddTextInput("Character Name (optional)", "raidjoin_name", maxLength: 12, required: false, value: currentInfo?.Name)
                    .Build());
            }
            else
            {
                await RespondSilentAsync("This channel is not a raid channel.");
            }
        }, defer: false);
    }

    [ModalInteraction("raidjoin_respond", ignoreGroupNames: true)]
    public async Task JoinRespondAsync(JoinModal modal)
    {
        await QueueContentTaskAsync(async (db, raid) =>
        {
            string? name = modal.Name;

            var expansionId = await db.Raids.AsNoTracking()
                .Where(raid => raid.ChannelId == Context.Channel.Id)
                .Select(raid => raid.Configuration.ExpansionId)
                .FirstOrDefaultAsync();

            if (expansionId is 0)
            {
                await RespondSilentAsync("Interaction can only be used in a raid channel.");
                return;
            }

            var playerClass = (await db.ExpansionClasses.AsNoTracking()
                .Where(ec => ec.ExpansionId == expansionId)
                .Select(ec => new { Id = ec.ClassId, SearchTerms = ec.Class.SearchTerms.Split(';', StringSplitOptions.RemoveEmptyEntries) })
                .ToListAsync())
                .Find(ec => ec.SearchTerms.Any(str => string.Equals(str, modal.Class, StringComparison.CurrentCultureIgnoreCase)));

            if (playerClass is null)
            {
                await RespondSilentAsync($"{Context.User.Mention} Class is not valid.");
                return;
            }

            var playerRole = (await db.Roles.AsNoTracking()
                .Select(role => new { role.Id, SearchTerms = role.SearchTerms.Split(';', StringSplitOptions.RemoveEmptyEntries) })
                .ToListAsync())
                .Find(ec => ec.SearchTerms.Any(str => string.Equals(str, modal.Role, StringComparison.CurrentCultureIgnoreCase)));

            if (playerRole is null)
            {
                await RespondSilentAsync($"{Context.User.Mention} Role is not valid.");
                return;
            }

            await AddInternalAsync(db, raid, playerClass.Id, playerRole.Id, Context.User, name);
        });
    }

    private async Task AddInternalAsync(
        ApplicationDbContext db,
        Raid raidContent,
        int playerClassId,
        int playerRoleId,
        IUser user,
        string? name)
    {
        var existing = raidContent.Members.Find(m => m.OwnerId == user.Id);
        var ownerName = (user as IGuildUser)?.DisplayName ?? user.Username;

        if (existing is not null)
        {
            existing.ClassId = playerClassId;
            existing.RoleId = playerRoleId;
            existing.Name = name ?? ownerName;
            existing.OwnerName = ownerName;
        }
        else
        {
            RaidMember member = new()
            {
                Name = name ?? ownerName,
                OwnerId = user.Id,
                OwnerName = ownerName,
                ClassId = playerClassId,
                RoleId = playerRoleId
            };
            db.RaidMembers.Add(member);
            raidContent.Members.Add(member);
        }

        await SaveAsync(db, Context.Channel, raidContent);

        string message = $"{user.Mention} {(existing is not null ? "updated." : "added.")}";

        if (Context.Interaction.Type != InteractionType.MessageComponent)
        {
            await RespondSilentAsync(message);
        }
    }
}
