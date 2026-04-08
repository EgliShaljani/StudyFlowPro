using Microsoft.Extensions.Options;

namespace StudyFlowPro.Web.Services;

public sealed class DeadlineNotificationBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptionsMonitor<DeadlineNotificationSettings> _settings;
    private readonly ILogger<DeadlineNotificationBackgroundService> _logger;

    public DeadlineNotificationBackgroundService(
        IServiceProvider serviceProvider,
        IOptionsMonitor<DeadlineNotificationSettings> settings,
        ILogger<DeadlineNotificationBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _settings = settings;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = _serviceProvider.CreateAsyncScope();
                var notificationService = scope.ServiceProvider.GetRequiredService<IDeadlineNotificationService>();
                await notificationService.RefreshForAllAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Deadline notification refresh failed.");
            }

            var delay = TimeSpan.FromSeconds(Math.Max(15, _settings.CurrentValue.RefreshIntervalSeconds));
            await Task.Delay(delay, stoppingToken);
        }
    }
}
