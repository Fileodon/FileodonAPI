namespace Controller.DTO.Organization;

public record class GetOrganizationRequest(
    string OrgName,
    string WorkspaceURL,
    string ContactEmail
);

public static class GetOrganizationRequestExtensions
{
    public static GetOrganizationRequest ToGetOrganizationRequest(this Infrastructure.Models.Organization organization) =>
        new(
            organization.OrgName,
            organization.WorkspaceURL,
            organization.ContactEmail
        );
}