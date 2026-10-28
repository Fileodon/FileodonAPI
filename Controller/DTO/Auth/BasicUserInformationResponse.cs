using Infrastructure.Models;
using Microsoft.AspNetCore.Identity;

namespace Controller.DTO.Auth;

public record class BasicUserInformationResponse(
    Guid Id,
    string Email,
    string Username,
    bool IsEmailConfirmed,
    IEnumerable<string> Roles,
    DateTime CreatedAt
);

public static class BasicUserInformationResponseExtensions
{
    public static async Task<BasicUserInformationResponse> ToBasicUserInformationResponse(this User user, UserManager<User> userManager)
    {
        return new BasicUserInformationResponse(
            Guid.Parse(user.Id),
            user.Email ?? string.Empty,
            user.UserName ?? string.Empty,
            user.EmailConfirmed,
            await userManager.GetRolesAsync(user),
            user.CreatedAt
        );
    }
}
