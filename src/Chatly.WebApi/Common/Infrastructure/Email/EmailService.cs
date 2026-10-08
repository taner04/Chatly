using Chatly.WebApi.Common.Composition.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Chatly.WebApi.Common.Infrastructure.Email;

[SingletonService(typeof(IEmailService))]
internal sealed partial class EmailService(
    ILogger<EmailService> logger,
    IOptions<EmailOption> emailOptions,
    EmailTemplateRenderer renderer) : IEmailService
{
    private const int ImplicitTlsPort = 465;

    private readonly EmailOption _emailOption = emailOptions.Value;

    public async Task SendEmailAsync(
        string recipient,
        EmailRequest email,
        CancellationToken cancellationToken)
    {
        var html = await renderer.RenderAsync(
            email.Template,
            email.Values,
            cancellationToken);

        var message = new MimeMessage
        {
            Subject = email.Subject,
            Body = new BodyBuilder
            {
                HtmlBody = html
            }.ToMessageBody()
        };

        message.From.Add(
            new MailboxAddress(
                _emailOption.SenderName,
                _emailOption.SenderEmail));

        message.To.Add(MailboxAddress.Parse(recipient));

        using var smtpClient = new SmtpClient();

        try
        {
            await smtpClient.ConnectAsync(
                _emailOption.Host,
                _emailOption.Port,
                GetSocketOptions(_emailOption),
                cancellationToken);

            if (!string.IsNullOrWhiteSpace(_emailOption.Username) &&
                !string.IsNullOrWhiteSpace(_emailOption.Password))
            {
                await smtpClient.AuthenticateAsync(
                    _emailOption.Username,
                    _emailOption.Password,
                    cancellationToken);
            }

            await smtpClient.SendAsync(
                message,
                cancellationToken);

            LogEmailSentToRecipient(
                logger,
                recipient,
                email.Subject);
        }
        catch (Exception exception)
        {
            LogErrorSendingEmail(
                logger,
                exception,
                recipient);

            throw;
        }
        finally
        {
            if (smtpClient.IsConnected)
            {
                await smtpClient.DisconnectAsync(
                    true,
                    CancellationToken.None);
            }
        }
    }

    internal static SecureSocketOptions GetSocketOptions(EmailOption option) =>
        option switch
        {
            { UseSsl: false } => SecureSocketOptions.None,
            { Port: ImplicitTlsPort } => SecureSocketOptions.SslOnConnect,
            _ => SecureSocketOptions.StartTls
        };

    [LoggerMessage(
        LogLevel.Information,
        "Email sent to {recipient} with subject {subject}")]
    private static partial void LogEmailSentToRecipient(
        ILogger<EmailService> logger,
        string recipient,
        string subject);

    [LoggerMessage(
        LogLevel.Error,
        "Failed to send email to {recipient}")]
    private static partial void LogErrorSendingEmail(
        ILogger<EmailService> logger,
        Exception exception,
        string recipient);
}