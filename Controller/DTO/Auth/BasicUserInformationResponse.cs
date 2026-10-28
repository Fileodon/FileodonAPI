using Infrastructure.Models;

namespace Controller.DTO.Auth;

public record class BasicUserInformationResponse(
    Guid Id,
    string Email,
    string Username,
    bool IsEmailConfirmed,
    DateTime CreatedAt
);

public static class BasicUserInformationResponseExtensions
{
    public static BasicUserInformationResponse ToBasicUserInformationResponse(this User user)
    {
        return new BasicUserInformationResponse(
            Guid.Parse(user.Id),
            user.Email ?? string.Empty,
            user.UserName ?? string.Empty,
            user.EmailConfirmed,
            user.CreatedAt
        );
    }
}
