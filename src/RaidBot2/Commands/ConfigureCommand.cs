using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.EntityFrameworkCore;
using RaidBot2.Data;

namespace RaidBot2.Commands;

public class ConfigureCommand(ApplicationDbContext db)
{
    public async Task ExecuteAsync(InteractionContext context, int expansionId, string timezone, string categoryName, DiscordRole role, CancellationToken cancellationToken = default)
    {
        var expansion = (await db.Expansions.FindAsync([expansionId], cancellationToken: cancellationToken))!;

        var allChannels = await context.Guild.GetChannelsAsync();

        var category = allChannels.FirstOrDefault(c => c.Type == ChannelType.Category && string.Equals(c.Name, categoryName, StringComparison.OrdinalIgnoreCase));

        category ??= await context.Guild.CreateChannelCategoryAsync(categoryName);

        var guild = await db.GuildExpansionConfigurations.FirstOrDefaultAsync(g => g.GuildId == context.Guild.Id && g.ExpansionId == expansion.Id, cancellationToken);

        if (guild is null)
        {
            guild = new()
            {
                GuildId = context.Guild.Id,
                ExpansionId = expansion.Id,
                CategoryId = category.Id,
                CreateRoleId = role.Id,
                Timezone = timezone
            };
            db.GuildExpansionConfigurations.Add(guild);
        }
        else
        {
            guild.CategoryId = category.Id;
            guild.CreateRoleId = role.Id;
        }

        await db.SaveChangesAsync(cancellationToken);

        await category.AddOverwriteAsync(context.Guild.CurrentMember, allow: Permissions.All);

        await context.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().WithContent("Configuration saved!").AsEphemeral());

        await context.Channel.SendMessageAsync(builder => builder.WithContent($"Start a new {expansion.Name} raid:").AddComponents(
            new DiscordButtonComponent(ButtonStyle.Primary, "newraid:s", "Start New Raid"),
            new DiscordButtonComponent(ButtonStyle.Secondary, "newraid:h", "Start Hidden Raid")
            ));
    }
}
