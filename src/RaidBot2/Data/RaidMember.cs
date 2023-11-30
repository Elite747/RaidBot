using System.ComponentModel.DataAnnotations;

namespace RaidBot2.Data;

public class RaidMember : DbItem
{
    [Required, StringLength(32)]
    public required string Name { get; set; }

    public ulong OwnerId { get; set; }

    [Required, StringLength(32, MinimumLength = 2)]
    public required string OwnerName { get; set; }

    public int RaidId { get; set; }

    public Raid Raid { get; set; } = null!;

    public int ClassId { get; set; }

    public PlayerClass Class { get; set; } = null!;

    public int RoleId { get; set; }

    public PlayerRole Role { get; set; } = null!;
}
