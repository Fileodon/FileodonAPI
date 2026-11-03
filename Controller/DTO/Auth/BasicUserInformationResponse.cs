using Infrastructure.Models;
using Microsoft.AspNetCore.Identity;

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
    public static async Task<BasicUserInformationResponse> ToBasicUserInformationResponse(this User user)
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
