using Chatly.WebApi.Common.Composition.Options;
using Chatly.WebApi.Common.Infrastructure.Email;
using MailKit.Security;

namespace Chatly.WebApi.UnitTests.Tests.Common.Infrastructure;

public sealed class EmailServiceTests
{
    [Theory]
    [InlineData(false, 2525, SecureSocketOptions.None)]
    [InlineData(false, 465, SecureSocketOptions.None)]
    [InlineData(true, 465, SecureSocketOptions.SslOnConnect)]
    [InlineData(true, 587, SecureSocketOptions.StartTls)]
    public void GetSocketOptions_Should_FollowUseSsl_When_ConnectingToSmtpServer(
        bool useSsl,
        int port,
        SecureSocketOptions expected)
    {
        var option = new EmailOption
        {
            Host = "smtp.example.com",
            Port = port,
            SenderName = "Chatly",
            SenderEmail = "noreply@example.com",
            UseSsl = useSsl
        };

        EmailService.GetSocketOptions(option).Should().Be(expected);
    }
}