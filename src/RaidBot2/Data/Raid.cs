using System.ComponentModel.DataAnnotations;

namespace RaidBot2.Data;

public class Raid : DbItem
{
    [Required, StringLength(32)]
    public required string Name { get; set; }

    public DateTime Date { get; set; }

    public ulong OwnerId { get; set; }

    public ulong ChannelId { get; set; }

    public ulong MessageId { get; set; }

    public List<RaidMember> Members { get; } = [];

    public int ConfigurationId { get; set; }

    public GuildExpansionConfiguration Configuration { get; set; } = null!;
}
