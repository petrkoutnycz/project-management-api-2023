
using ProjectManager.Api.Services;

namespace ProjectManager.Api.BackgroundServices;

public class EmailSenderBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public EmailSenderBackgroundService(
        IServiceProvider serviceProvider
        )
    {
        // TODO: usually services injected with IServiceProvider are suspicious
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await SendEmailsAsync(stoppingToken);
    }

    private async Task SendEmailsAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<EmailSenderService>();

            // TODO: why not propagating cancellation token? Intentional?
            await service!.SendEmailsAsync();

            // TODO: sometimes it makes sense to be explicit and say why 1 min
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
