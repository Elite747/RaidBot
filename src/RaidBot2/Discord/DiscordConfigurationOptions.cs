namespace RaidBot2.Discord;

public class DiscordConfigurationOptions
{
    public required string BotToken { get; set; }

    public ulong? ServerId { get; set; }

    public TimeSpan PeriodicTaskInterval { get; set; }
}
