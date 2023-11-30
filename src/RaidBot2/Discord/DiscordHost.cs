using System.Threading.Channels;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Options;
using RaidBot2.Discord.Tasks;

namespace RaidBot2.Discord;

public class DiscordHost : BackgroundService, IAsyncDisposable
{
    private readonly AsyncServiceScope _slashCommandsScope;
    private readonly ChannelReader<IDiscordTask> _taskReader;
    private readonly DiscordClient _discordClient;
    private readonly ILogger<DiscordHost> _logger;

    public DiscordHost(IServiceProvider serviceProvider, ILogger<DiscordHost> logger)
    {
        var options = serviceProvider.GetRequiredService<IOptions<DiscordConfigurationOptions>>();
        _slashCommandsScope = serviceProvider.CreateAsyncScope();
        _taskReader = serviceProvider.GetRequiredService<Channel<IDiscordTask>>();
        _discordClient = new(new DiscordConfiguration
        {
            TokenType = TokenType.Bot,
            Token = options.Value.BotToken,
            LoggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>()
        });
        var slash = _discordClient.UseSlashCommands(new SlashCommandsConfiguration
        {
            Services = _slashCommandsScope.ServiceProvider
        });
        RaidBot2.Commands.ExpansionChoiceProvider.ServiceProvider = _slashCommandsScope.ServiceProvider;
        slash.RegisterCommands<Commands.RaidCommand>(options.Value.ServerId);
        _logger = logger;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await _discordClient.InitializeAsync();
        await _discordClient.ConnectAsync(status: DSharpPlus.Entities.UserStatus.Online);
        await base.StartAsync(cancellationToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
        await _discordClient.DisconnectAsync();
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var task in _taskReader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await task.ExecuteAsync(_discordClient, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Event execution failed.");
            }
        }
    }

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        _discordClient.Dispose();
        return _slashCommandsScope.DisposeAsync();
    }
}
