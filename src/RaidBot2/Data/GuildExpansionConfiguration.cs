namespace RaidBot2.Data;

public class GuildExpansionConfiguration : DbItem
{
    public ulong GuildId { get; set; }

    public int ExpansionId { get; set; }

    public Expansion Expansion { get; set; } = null!;

    /// <summary>
    /// The id of the category events are created under.
    /// </summary>
    public ulong CategoryId { get; set; }

    /// <summary>
    /// The id of the role that is required to create an event.
    /// </summary>
    public ulong CreateRoleId { get; set; }

    public required string Timezone { get; set; }
}
