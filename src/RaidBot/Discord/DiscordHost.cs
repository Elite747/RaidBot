using System.Reflection;
using System.Threading.Channels;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using RaidBot.Discord.Tasks;

namespace RaidBot.Discord;

internal class DiscordHost : IHostedService, IAsyncDisposable
{
    private readonly AsyncServiceScope _scope;
    private readonly DiscordSocketClient _discord;
    private readonly InteractionService _interactions;
    private readonly ILogger _logger;
    private readonly DiscordConfigurationOptions _options;
    private readonly ChannelReader<IDiscordTask> _taskReader;
    private CancellationTokenSource? _maintenanceCts;

    public DiscordHost(
        DiscordSocketClient discord,
        IServiceProvider serviceProvider,
        ChannelReader<IDiscordTask> taskReader,
        ILogger<DiscordHost> logger,
        IOptions<DiscordConfigurationOptions> options)
    {
        _discord = discord;
        _interactions = new InteractionService(discord, new()
        {
            AutoServiceScopes = true,
            DefaultRunMode = RunMode.Async,
            ExitOnMissingModalField = true,
            UseCompiledLambda = true
        });
        _taskReader = taskReader;
        _logger = logger;
        _options = options.Value;
        _scope = serviceProvider.CreateAsyncScope();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _discord.InteractionCreated += HandleInteractionAsync;
        _discord.AutocompleteExecuted += HandleAutocompleteAsync;
        _discord.Ready += OnReady;
        _discord.Connected += OnConnected;
        _discord.Log += Log;
        _interactions.Log += Log;
        await _interactions.AddModulesAsync(Assembly.GetEntryAssembly(), _scope.ServiceProvider);
        await _discord.LoginAsync(TokenType.Bot, _options.BotToken);
        await _discord.StartAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _discord.InteractionCreated -= HandleInteractionAsync;
        _discord.Ready -= OnReady;
        _discord.Connected -= OnConnected;
        _discord.Log -= Log;
        _interactions.Log -= Log;
        await _discord.LogoutAsync();
        EndMaintenance();
    }

    private async Task MaintainAsync(CancellationToken cancellationToken)
    {
        await foreach (var task in _taskReader.ReadAllAsync(cancellationToken))
        {
            try
            {
                await task.ExecuteAsync(_discord, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Event execution failed.");
            }
        }
    }

    private async Task HandleInteractionAsync(SocketInteraction arg)
    {
        try
        {
            // Create an execution context that matches the generic type parameter of your InteractionModuleBase<T> modules
            var ctx = new SocketInteractionContext(_discord, arg);
            await _interactions.ExecuteCommandAsync(ctx, _scope.ServiceProvider);
            var t = ctx.Interaction.GetType();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute interaction");

            // If a Slash Command execution fails it is most likely that the original interaction acknowledgement will persist. It is a good idea to delete the original
            // response, or at least let the user know that something went wrong during the command execution.
            if (arg.Type == InteractionType.ApplicationCommand)
            {
                var msg = await arg.GetOriginalResponseAsync();
                await msg.DeleteAsync();
            }
        }
    }

    private async Task HandleAutocompleteAsync(SocketAutocompleteInteraction arg)
    {
        try
        {
            var context = new InteractionContext(_discord, arg, arg.Channel);
            await _interactions.ExecuteCommandAsync(context, services: _scope.ServiceProvider);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute autocomplete");

            // If a Slash Command execution fails it is most likely that the original interaction acknowledgement will persist. It is a good idea to delete the original
            // response, or at least let the user know that something went wrong during the command execution.
            if (arg.Type == InteractionType.ApplicationCommand)
            {
                var msg = await arg.GetOriginalResponseAsync();
                await msg.DeleteAsync();
            }
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2254:Template should be a static expression", Justification = "<Pending>")]
    private Task Log(LogMessage arg)
    {
        _logger.Log(
            arg.Severity switch
            {
                LogSeverity.Critical => LogLevel.Critical,
                LogSeverity.Error => LogLevel.Error,
                LogSeverity.Warning => LogLevel.Warning,
                LogSeverity.Info => LogLevel.Information,
                LogSeverity.Verbose => LogLevel.Trace,
                LogSeverity.Debug => LogLevel.Debug,
                _ => (LogLevel)arg.Severity
            },
            exception: arg.Exception,
            message: arg.Message);
        return Task.CompletedTask;
    }

    private async Task OnReady()
    {
        if (_options.ServerId > 0)
        {
            await _interactions.RegisterCommandsToGuildAsync(_options.ServerId.Value);
        }
        else
        {
            await _interactions.RegisterCommandsGloballyAsync();
        }
    }

    private Task OnConnected()
    {
        EndMaintenance();
        _maintenanceCts = new();
        _ = MaintainAsync(_maintenanceCts.Token);
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        EndMaintenance();
        return _scope.DisposeAsync();
    }

    private void EndMaintenance()
    {
        _maintenanceCts?.Cancel();
        _maintenanceCts?.Dispose();
        _maintenanceCts = null;
    }
}
