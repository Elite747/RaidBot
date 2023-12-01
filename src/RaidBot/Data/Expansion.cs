using System.ComponentModel.DataAnnotations;

namespace RaidBot.Data;

public class Expansion : DbItem
{
    /// <summary>
    /// The name of this expansion.
    /// </summary>
    [Required, StringLength(32)]
    public required string Name { get; set; }

    /// <summary>
    /// A shortened acronym that may be used to identify this expansion.
    /// </summary>
    [Required, StringLength(8)]
    public required string ShortName { get; set; }

    public List<PlayerClass> Classes { get; } = [];

    public List<ExpansionClass> ExpansionClasses { get; } = [];

    public List<GuildExpansionConfiguration> GuildExpansionConfigurations { get; } = [];
}
