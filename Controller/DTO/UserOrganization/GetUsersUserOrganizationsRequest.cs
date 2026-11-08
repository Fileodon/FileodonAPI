using Controller.DTO.Organization;
using Infrastructure.Models;

namespace Controller.DTO.UserOrganization;

public record class UserOrganizationRequest
(
    int Id,
    string roleName,
    GetOrganizationRequest organization
);

public class GetUsersUserOrganizationRequest
{
    public List<UserOrganizationRequest> userOrganizationRequests {get;}

    public GetUsersUserOrganizationRequest(List<global::Infrastructure.Models.UserOrganization> userOrganizations)
    {
        userOrganizationRequests = userOrganizations.Select(uo => new UserOrganizationRequest(
            Id: uo.Id,
            roleName: uo.Role.Name,
            organization: uo.Organization?.ToGetOrganizationRequest()
        )).ToList();
    }
}