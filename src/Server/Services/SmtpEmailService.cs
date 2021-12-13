using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using Vetrina.Server.Abstractions;
using Vetrina.Server.Options;
using Vetrina.Shared.Models;

namespace Vetrina.Server.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly SmtpOptions smtpOptions;
        private readonly ILogger<SmtpEmailService> logger;

        public SmtpEmailService(
            IOptions<SmtpOptions> smtpOptionsSnapshot,
            ILogger<SmtpEmailService> logger)
        {
            smtpOptions = smtpOptionsSnapshot.Value;
            this.logger = logger;
        }

        public async Task<SendEmailResponse> SendEmailAsync(
            SendEmailRequest sendEmailRequest,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var email = new MimeMessage
                {
                    Subject = sendEmailRequest.Subject,
                    To = { MailboxAddress.Parse(sendEmailRequest.To) },
                    From = { MailboxAddress.Parse(sendEmailRequest.From) },
                    Body = new TextPart(TextFormat.Html) { Text = sendEmailRequest.Content },
                };

                using var smtpClient = new SmtpClient();

                // Since connecting and authenticating are expensive operations,
                // re-using a connection can significantly improve the performance when sending
                // a large number of messages to the same SMTP server over a short period of time.
                await smtpClient.ConnectAsync(
                    host: smtpOptions.SmtpHost,
                    port: smtpOptions.SmtpPort,
                    SecureSocketOptions.StartTls,
                    cancellationToken);

                await smtpClient.AuthenticateAsync(
                    userName: smtpOptions.SmtpUser,
                    password: smtpOptions.SmtpPass,
                    cancellationToken);

                await smtpClient.SendAsync(
                    email,
                    cancellationToken);

                await smtpClient.DisconnectAsync(
                    quit: true,
                    cancellationToken);

                return new SendEmailResponse(SendEmailResponseType.Sent);
            }
            catch (Exception exception)
            {
                var errorMessage =
                    $"Encountered failure while attempting to send email using {nameof(SmtpEmailService)}. " +
                    $"Reason: {exception.Message}." +
                    $"Request: {JsonSerializer.Serialize(sendEmailRequest)}";

                logger.LogError(
                    exception,
                    errorMessage);

                return new SendEmailResponse(SendEmailResponseType.Failed, errorMessage);
            }
        }
    }
}
