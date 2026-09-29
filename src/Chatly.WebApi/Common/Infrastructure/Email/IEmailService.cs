namespace Chatly.WebApi.Common.Infrastructure.Email;

internal interface IEmailService
{
    Task SendEmailAsync(string recipient, EmailRequest email, CancellationToken cancellationToken);
}