namespace Controller.DTO.Organization;

public record CreateOrganizationRequest(
    string OrgName,
    string WorkspaceURL,
    string ContactEmail
);
