namespace Controller.DTO.Organization;

public record GetOrganizationResponse(
    int Id,
    string OrgName,
    string WorkspaceURL,
    string ContactEmail
);

public static class GetOrganizationRequestExtensions
{
    public static GetOrganizationResponse ToGetOrganizationResponse(this Infrastructure.Models.Organization organization) =>
        new(
            organization.Id,
            organization.OrgName,
            organization.WorkspaceURL,
            organization.ContactEmail
        );
}