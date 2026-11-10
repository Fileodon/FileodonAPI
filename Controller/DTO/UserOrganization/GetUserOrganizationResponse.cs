using Controller.DTO.Role;

namespace Controller.DTO.UserOrganization
{
    public record class GetUserOrganizationResponse(
        int id,
        string userName,
        GetRoleResponse role
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
              userOrganization.Id,
                userOrganization.User.UserName,
                new GetRoleResponse(
                    userOrganization.Role.Id,
                    userOrganization.Role.Name
                )
            );
        }
    }
}
