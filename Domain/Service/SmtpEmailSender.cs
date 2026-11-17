using System.Net;
using Domain.Interface;
using Domain.Worker;
using Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Domain.Service;

public class SmtpEmailSender : ISmtpEmailSender
{
  private readonly IConfiguration _configuration;
  private readonly IEmailQueue _emailQueue;
  private readonly UserManager<User> _userManager;

  public SmtpEmailSender(
      IConfiguration configuration,
      IEmailQueue emailQueue,
      UserManager<User> userManager)
  {
    _configuration = configuration;
    _emailQueue = emailQueue;
    _userManager = userManager;
  }

  public async Task<bool> SendConfirmationLinkAsync(User user)
  {
    if (string.IsNullOrEmpty(user.Email))
    {
      throw new InvalidOperationException("User email is not set.");
    }

    string host = _configuration["API:HOST"] ?? throw new InvalidOperationException("API:HOST is not set in configuration.");

    string token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
    string encodedUserId = Uri.EscapeDataString(user.Id.ToString());
    string encodedToken = Uri.EscapeDataString(token);
    string confirmationLink = $"{host}/confirm-email?userId={encodedUserId}&token={encodedToken}";

    string subject = "Confirm your email";
    string html = BuildEmail(
        "Welcome to Fileodon",
        "Thanks for signing up. Confirm your email address to activate your account.",
        "Confirm email address",
        confirmationLink,
        "If you did not create a Fileodon account, you can safely ignore this email.");

    EmailWorkItem workItem = new(user.Email, subject, html);

    return _emailQueue.Enqueue(workItem);
  }

  public async Task<bool> SendPasswordResetLinkAsync(User user)
  {
    if (string.IsNullOrEmpty(user.Email))
    {
      throw new InvalidOperationException("User email is not set.");
    }

    string host = _configuration["API:HOST"] ?? throw new InvalidOperationException("API:HOST is not set in configuration.");

    string token = await _userManager.GeneratePasswordResetTokenAsync(user);
    string encodedUserId = Uri.EscapeDataString(user.Id.ToString());
    string encodedToken = Uri.EscapeDataString(token);
    string resetLink = $"{host}/reset-password?userId={encodedUserId}&token={encodedToken}";

    string subject = "Reset your password";
    string html = BuildEmail(
        "Reset your password",
        "Click the button below to choose a new password for your Fileodon account.",
        "Reset password",
        resetLink,
        "If you did not request a password reset, you can safely ignore this email.");

    EmailWorkItem workItem = new(user.Email, subject, html);

    return _emailQueue.Enqueue(workItem);
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
               <p style="color:#667085;font-size:13px;line-height:1.6;word-break:break-all;">Or copy and paste this link into your browser:<br /> </p>
               <a href="{encodedValue}" style="color:#667085;display:inline-block;max-width:100%;overflow-wrap:anywhere;word-break:break-all;">{encodedValue}</a>
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
}
