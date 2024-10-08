using MediatR;
using Microsoft.Extensions.Options;
using ProjectManager.Application.Contracts.Emails.Commands;
using ProjectManager.Configurations;
using ProjectManager.Data;
using ProjectManager.Data.Entities;
using ProjectManager.Data.Interfaces;

namespace ProjectManager.Application.Emails.Commands;

public class AddRegistrationConfirmationEmailCommandHandler
    : IRequestHandler<AddRegistrationConfirmationEmailCommand>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly SmtpSettings _smtpSettings;

    public AddRegistrationConfirmationEmailCommandHandler(
        IOptions<SmtpSettings> smtpSettingsOptions, ApplicationDbContext dbContext)
    {
        _smtpSettings = smtpSettingsOptions.Value;
        _dbContext = dbContext;
    }

    public async Task Handle(AddRegistrationConfirmationEmailCommand request, CancellationToken cancellationToken)
    {
        // see https://learn.microsoft.com/en-us/dotnet/core/extensions/localization
        const string subject = "Potvrzení registrace";

        // TODO: replace hardcoded url with settings
        var body =
            $"<a href=\"https://www.projectmanagement.cz/?token={Uri.EscapeDataString(request.ConfirmationToken)}&email={request.Receiver.Address}\">{request.ConfirmationToken}</a>";

        var currentInstant = NodaTime.SystemClock.Instance.GetCurrentInstant();
        var newMail = new Email
        {
            Body = body,
            Subject = subject,
            Receiver = request.Receiver.Address,
            Sender = _smtpSettings.Sender,
            ScheduledAt = currentInstant,
        }.SetCreateBySystem(currentInstant);

        await _dbContext.Emails.AddAsync(newMail, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}