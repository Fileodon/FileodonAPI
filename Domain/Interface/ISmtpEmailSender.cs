using Infrastructure.Models;

namespace Domain.Interface;

public interface ISmtpEmailSender
{
    Task<bool> SendConfirmationLinkAsync(User user);
    Task<bool> SendPasswordResetLinkAsync(User user);
}
