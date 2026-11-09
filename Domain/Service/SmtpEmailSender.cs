using System;
using Infrastructure.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace Domain.Service;

public class SmtpEmailSender : IEmailSender<User>
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IConfiguration configuration, ILogger<SmtpEmailSender> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendConfirmationLinkAsync(User user, string email, string confirmationLink)
    {
        string subject = "Confirm your email";
        string html = $"Please confirm your account by clicking this link: <a href=\"{confirmationLink}\">Confirm Email</a><br/>" +
                      $"Or copy and paste the following link into your browser: {confirmationLink}";

        await SendAsync(email, subject, html);
    }

    public Task SendPasswordResetCodeAsync(User user, string email, string resetCode)
    {
        string subject = "Password Reset Code";
        string html = $"Your password reset code is: <strong>{resetCode}</strong><br/>" +
                      $"If you did not request a password reset, please ignore this email.";

        return SendAsync(email, subject, html);
    }

    public Task SendPasswordResetLinkAsync(User user, string email, string resetLink)
    {
        string subject = "Reset your password";
        string html = $"Please reset your password by clicking this link: <a href=\"{resetLink}\">Reset Password</a><br/>" +
                      $"Or copy and paste the following link into your browser: {resetLink}";

        return SendAsync(email, subject, html);
    }

    private async Task SendAsync(string to, string subject, string html)
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
            MailboxAddress toAddress = MailboxAddress.Parse(to);
            message.To.Add(toAddress);
        }
        catch (ParseException)
        {
            _logger.LogError("Invalid email format for recipient. {ToEmail}", to);
            throw;
        }

        message.Subject = subject;
        message.Body = new BodyBuilder { HtmlBody = html }.ToMessageBody();

        using var client = new SmtpClient();
        try
        {
            var socketOptions = useStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.SslOnConnect;

            await client.ConnectAsync(host, port, socketOptions);
            await client.AuthenticateAsync(username, password);

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send {Subject} email to {ToEmail}", subject, to);
            throw;
        }
    }
}
