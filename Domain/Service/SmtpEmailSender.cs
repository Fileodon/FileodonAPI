using System;
using System.Net;
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
        string html = BuildEmail(
            "Welcome to Fileodon",
            "Thanks for signing up. Confirm your email address to activate your account.",
            "Confirm email address",
            confirmationLink,
            "If you did not create a Fileodon account, you can safely ignore this email.");

        await SendAsync(email, subject, html);
    }

    public Task SendPasswordResetCodeAsync(User user, string email, string resetCode)
    {
        string subject = "Password Reset Code";
        string html = BuildEmail(
            "Reset your password",
            "Use the verification code below to reset your Fileodon password.",
            null,
            resetCode,
            "If you did not request a password reset, you can safely ignore this email.");

        return SendAsync(email, subject, html);
    }

    public Task SendPasswordResetLinkAsync(User user, string email, string resetLink)
    {
        string subject = "Reset your password";
        string html = BuildEmail(
            "Reset your password",
            "Click the button below to choose a new password for your Fileodon account.",
            "Reset password",
            resetLink,
            "If you did not request a password reset, you can safely ignore this email.");

        return SendAsync(email, subject, html);
    }

    private static string BuildEmail(
        string title,
        string message,
        string? actionLabel,
        string actionValue,
        string note)
    {
        string encodedTitle = WebUtility.HtmlEncode(title);
        string encodedMessage = WebUtility.HtmlEncode(message);
        string encodedValue = WebUtility.HtmlEncode(actionValue);
        string encodedNote = WebUtility.HtmlEncode(note);
        string action = actionLabel is null
            ? $"""
               <div style="margin:28px 0;padding:18px;text-align:center;background:#eef6ff;border:1px solid #cfe5ff;border-radius:8px;color:#0c2d55;font-size:28px;font-weight:700;letter-spacing:4px;">{encodedValue}</div>
               """
            : $"""
               <p style="margin:28px 0;text-align:center;">
                 <a href="{encodedValue}" style="display:inline-block;padding:14px 26px;background:#087cf5;border-radius:7px;color:#ffffff;font-size:16px;font-weight:700;text-decoration:none;">{WebUtility.HtmlEncode(actionLabel)}</a>
               </p>
               <p style="color:#667085;font-size:13px;line-height:1.6;word-break:break-all;">Or copy and paste this link into your browser:<br />{encodedValue}</p>
               """;

        return $"""
            <!doctype html>
            <html lang="en">
              <body style="margin:0;padding:0;background:#f4f7fb;font-family:Arial,Helvetica,sans-serif;color:#14213d;">
                <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="background:#f4f7fb;padding:32px 12px;">
                  <tr><td align="center">
                    <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="max-width:600px;background:#ffffff;border-radius:12px;overflow:hidden;">
                      <tr><td style="padding:24px 32px;background:#071a38;text-align:center;">
                        <img src="cid:fileodon-logo" alt="Fileodon" width="220" style="display:block;width:220px;max-width:100%;height:auto;margin:0 auto;" />
                      </td></tr>
                      <tr><td style="padding:36px 40px 32px;">
                        <h1 style="margin:0 0 16px;color:#071a38;font-size:26px;line-height:1.25;">{encodedTitle}</h1>
                        <p style="margin:0;color:#475467;font-size:16px;line-height:1.6;">{encodedMessage}</p>
                        {action}
                        <p style="margin:24px 0 0;color:#98a2b3;font-size:13px;line-height:1.5;">{encodedNote}</p>
                      </td></tr>
                      <tr><td style="padding:20px 40px;background:#f8fafc;color:#98a2b3;font-size:12px;text-align:center;">
                        Fileodon &middot; Secure and easy SFTP management
                      </td></tr>
                    </table>
                  </td></tr>
                </table>
              </body>
            </html>
            """;
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
        BodyBuilder body = new() { HtmlBody = html };
        using Stream? logoStream = typeof(SmtpEmailSender).Assembly
            .GetManifestResourceStream("Fileodon.EmailLogo.png");
        if (logoStream is null)
        {
            throw new InvalidOperationException("The embedded Fileodon email logo could not be found.");
        }

        MimeEntity logo = body.LinkedResources.Add("fileodon-logo", logoStream);
        logo.ContentId = "fileodon-logo";
        message.Body = body.ToMessageBody();

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
