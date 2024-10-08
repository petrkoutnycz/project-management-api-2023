using System.Net.Mail;
using MediatR;

namespace ProjectManager.Application.Contracts.Emails.Commands;

// https://en.wikipedia.org/wiki/Immutable_object
public record AddRegistrationConfirmationEmailCommand
    (MailAddress Receiver, string ConfirmationToken) : IRequest;
