using Controller.DTO.Organization;
using Controller.DTO.Role;

namespace Controller.DTO.UserOrganization
{
    public record class GetUserOrganizationResponse(
        int Id,
        string UserName,
        GetRoleResponse Role
    );

    public record GetUsersUserOrganizationResponse(
        int Id,
        GetRoleResponse Role,
        GetOrganizationResponse Organization
    );


    public static class GetUserOrganizationResponseExtensions
    {
        public static GetUserOrganizationResponse ToGetUserOrganizationResponse(this Infrastructure.Models.UserOrganization userOrganization)
        {
            ArgumentNullException.ThrowIfNull(userOrganization);
            ArgumentNullException.ThrowIfNull(userOrganization.User);
            ArgumentException.ThrowIfNullOrEmpty(userOrganization.User.UserName);
            ArgumentNullException.ThrowIfNull(userOrganization.Role);
            ArgumentException.ThrowIfNullOrEmpty(userOrganization.Role.Name);
            return new GetUserOrganizationResponse(
                Id: userOrganization.Id,
                UserName: userOrganization.User.UserName,
                Role: userOrganization.Role.ToGetRoleResponse()
            );
        }
        public static GetUsersUserOrganizationResponse ToGetUsersUserOrganizationResponse(this Infrastructure.Models.UserOrganization userOrganization)
        {
            ArgumentNullException.ThrowIfNull(userOrganization);
            ArgumentNullException.ThrowIfNull(userOrganization.Role);
            ArgumentException.ThrowIfNullOrEmpty(userOrganization.Role.Name);
            ArgumentNullException.ThrowIfNull(userOrganization.Organization);
            return new GetUsersUserOrganizationResponse(
                Id: userOrganization.Id,
                Role: userOrganization.Role.ToGetRoleResponse(),
                Organization:userOrganization.Organization.ToGetOrganizationResponse()
            );
        }
    }
}
