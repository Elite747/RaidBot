using System.ComponentModel.DataAnnotations;

namespace RaidBot.Data;

public class PlayerClass : DbItem
{
    /// <summary>
    /// The name of this class.
    /// </summary>
    [Required, StringLength(16)]
    public required string Name { get; set; }

    /// <summary>
    /// An icon that can be used to represent this class.
    /// </summary>
    [Required, StringLength(64)]
    public required string Icon { get; set; }

    /// <summary>
    /// Alternative search terms that can be used to identify this class.
    /// </summary>
    [Required, StringLength(128)]
    public required string SearchTerms { get; set; }

    public List<Expansion> Expansions { get; } = [];

    public List<ExpansionClass> ExpansionClasses { get; } = [];

    public List<RaidMember> Members { get; } = [];
}
