namespace Controller.DTO.Organization;

public record class CreateOrganizationRequest(
    string OrgName,
    string WorkspaceURL,
    string ContactEmail
);
