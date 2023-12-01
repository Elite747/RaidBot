using System.ComponentModel.DataAnnotations;

namespace RaidBot.Data;

public class PlayerRole : DbItem
{
    /// <summary>
    /// The name of this role.
    /// </summary>
    [Required, StringLength(16)]
    public required string Name { get; set; }

    /// <summary>
    /// An icon that can be used to represent this role.
    /// </summary>
    [Required, StringLength(64)]
    public required string Icon { get; set; }

    /// <summary>
    /// Search terms that can be used to identify this role.
    /// </summary>
    [Required, StringLength(128)]
    public required string SearchTerms { get; set; }

    public List<RaidMember> Members { get; } = [];
}
