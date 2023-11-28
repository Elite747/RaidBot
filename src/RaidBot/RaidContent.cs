namespace RaidBot;

public class RaidContent(string name, DateTimeOffset date, ulong ownerId)
{
    public string Name { get; set; } = name;

    public DateTimeOffset Date { get; set; } = date;

    public ulong OwnerId { get; set; } = ownerId;

    public List<RaidMember> Members { get; set; } = [];

    public ulong? MessageId { get; set; }
}
