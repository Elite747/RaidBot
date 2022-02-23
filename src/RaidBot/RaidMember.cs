namespace RaidBot;

public class RaidMember
{
    public RaidMember(PlayerClass playerClass, PlayerRole playerRole, ulong ownerId)
    {
        PlayerClass = playerClass;
        PlayerRole = playerRole;
        OwnerId = ownerId;
    }

    public PlayerClass PlayerClass { get; set; }

    public PlayerRole PlayerRole { get; set; }

    public ulong OwnerId { get; set; }
}
