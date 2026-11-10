using Controller.DTO.Organization;
using Controller.DTO.Role;

namespace Controller.DTO.UserOrganization
{
    public record class GetUserOrganizationResponse(
        int id,
        string userName,
        GetRoleResponse role
    );

    public record GetUsersUserOrganizationResponse(
        int id,
        GetRoleResponse role,
        GetOrganizationResponse organization
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
                id: userOrganization.Id,
                userName: userOrganization.User.UserName,
                role: userOrganization.Role.ToGetRoleResponse()
            );
        }
        public static GetUsersUserOrganizationResponse ToGetUsersUserOrganizationResponse(this Infrastructure.Models.UserOrganization userOrganization)
        {
            ArgumentNullException.ThrowIfNull(userOrganization);
            ArgumentNullException.ThrowIfNull(userOrganization.Role);
            ArgumentException.ThrowIfNullOrEmpty(userOrganization.Role.Name);
            ArgumentNullException.ThrowIfNull(userOrganization.Organization);
            return new GetUsersUserOrganizationResponse(
                id: userOrganization.Id,
                role: userOrganization.Role.ToGetRoleResponse(),
                organization:userOrganization.Organization.ToGetOrganizationResponse()
            );
        }
    }
}
