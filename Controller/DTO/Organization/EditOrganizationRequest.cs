namespace Controller.DTO.Organization;


public record EditOrganizationRequest (
    int Id,
    string OrgName,
    string WorkspaceURL,
    string ContactEmail
);
