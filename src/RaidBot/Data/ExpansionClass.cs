namespace RaidBot.Data;

public class ExpansionClass
{
    public int ExpansionId { get; set; }
    public int ClassId { get; set; }
    public Expansion Expansion { get; set; } = null!;
    public PlayerClass Class { get; set; } = null!;
}
