using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MimeKit;
using NodaTime;
using ProjectManager.Api.Settings;
using ProjectManager.Data;
using ProjectManager.Data.Entities;
using ProjectManager.Data.Interfaces;
using System.Net.Mail;

namespace ProjectManager.Api.Services;

public class EmailSenderService
{
    private readonly SmtpSettings _smtpSettings;
    private readonly ApplicationDbContext _dbContext;
    private readonly IClock _clock;

    public EmailSenderService(
        IClock clock,
        ApplicationDbContext dbContext,
        IOptionsSnapshot<SmtpSettings> smtpSettings
    )
    {
        _clock = clock;
        _dbContext = dbContext;
        _smtpSettings = smtpSettings.Value;
    }

    public async Task AddEmailToSendAsync(
        string receiver,
        string subject,
        string body,
        CancellationToken cancellationToken = default
        )
    {
        var now = _clock.GetCurrentInstant();

        var newMail = new Email
        {
            Body = body,
            Subject = subject,
            Receiver = receiver,
            Sender = _smtpSettings.Sender,
            ScheduledAt = now,
        }.SetCreateBySystem(now);

        await _dbContext.Emails.AddAsync(newMail, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SendEmailsAsync()
    {
        var mails = await _dbContext.Set<Email>().Where(x => x.SentAt == null).ToListAsync();

        foreach ( var mail in mails )
        {
            using var notif = new MailMessage
            {
                Subject = mail.Subject,
                Body = mail.Body,
                IsBodyHtml = false,
                From = new MailAddress(mail.Sender),
            };
            notif.To.Add(new MailAddress(mail.Receiver));

            try
            {
                using var smtp = new MailKit.Net.Smtp.SmtpClient();
                await smtp.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port);
                await smtp.AuthenticateAsync(_smtpSettings.UserName, _smtpSettings.Password);
                await smtp.SendAsync((MimeMessage)notif);

                mail.SentAt = _clock.GetCurrentInstant();

                // TODO: if it sent successfully and this fails, then it is sent twice. Right?
                await _dbContext.SaveChangesAsync();
            }
            // TODO: specific exceptions should be used instead
            catch (Exception ex)
            {
                // log error, notify someone
            }
            finally
            {
                // nothing to do right now
            }
        }
    }
}
