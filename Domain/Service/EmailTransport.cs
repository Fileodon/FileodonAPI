using Domain.Worker;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace Domain.Service;

public class EmailTransport : IEmailTransport
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailTransport> _logger;

    public EmailTransport(IConfiguration configuration, ILogger<EmailTransport> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendAsync(EmailWorkItem workItem, CancellationToken cancellationToken)
    {
        var message = new MimeMessage();
        string from = _configuration["SMTP:FROM_ADDRESS"] ?? throw new InvalidOperationException("SMTP:FROM_ADDRESS is not configured.");
        string host = _configuration["SMTP:HOST"] ?? throw new InvalidOperationException("SMTP:HOST is not configured.");
        int port = Convert.ToInt32(_configuration["SMTP:PORT"] ?? throw new InvalidOperationException("SMTP:PORT is not configured."));
        string username = _configuration["SMTP:USER"] ?? throw new InvalidOperationException("SMTP:USER is not configured.");
        string password = _configuration["SMTP:PASSWORD"] ?? throw new InvalidOperationException("SMTP:PASSWORD is not configured.");
        bool useStartTls = Convert.ToBoolean(_configuration["SMTP:USE_STARTTLS"] ?? "false");

        try
        {
            MailboxAddress fromAddress = MailboxAddress.Parse(from);
            message.From.Add(new MailboxAddress("Fileodon", fromAddress.ToString()));
        }
        catch (ParseException)
        {
            _logger.LogError("Invalid email format for FromEmail in configuration. {FromEmail}", from);
            throw;
        }

        try
        {
            MailboxAddress toAddress = MailboxAddress.Parse(workItem.Recipient);
            message.To.Add(toAddress);
        }
        catch (ParseException)
        {
            _logger.LogError("Invalid email format for recipient. {ToEmail}", workItem.Recipient);
            throw;
        }

        message.Subject = workItem.Subject;
        BodyBuilder body = new() { HtmlBody = workItem.HtmlBody };
        using Stream? logoStream = typeof(SmtpEmailSender).Assembly
            .GetManifestResourceStream("Fileodon.EmailLogo.png") ?? throw new InvalidOperationException("The embedded Fileodon email logo could not be found.");
        MimeEntity logo = body.LinkedResources.Add("fileodon-logo", logoStream, cancellationToken);
        logo.ContentId = "fileodon-logo";
        message.Body = body.ToMessageBody();

        using var client = new SmtpClient();
        try
        {
            var socketOptions = useStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.None;

            await client.ConnectAsync(host, port, socketOptions, cancellationToken);
            await client.AuthenticateAsync(username, password, cancellationToken);

            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send {Subject} email to {ToEmail}", workItem.Subject, workItem.Recipient);
            throw;
        }
    }
}
