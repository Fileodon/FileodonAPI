namespace Controller.DTO.Organization;

public record class GetOrganizationRequest(
    int Id,
    string OrgName,
    string WorkspaceURL,
    string ContactEmail
);

public static class GetOrganizationRequestExtensions
{
    public static GetOrganizationRequest ToGetOrganizationRequest(this Infrastructure.Models.Organization organization) =>
        new(
            organization.Id,
            organization.OrgName,
            organization.WorkspaceURL,
            organization.ContactEmail
        );
}