namespace Controller.DTO.Role;

public record class GetRoleResponse
(
    string Id,
    string Name
);


public static class GetRoleResponseExtensions
{
    public static GetRoleResponse ToGetRoleResponse(this Infrastructure.Models.AspNetRole role) =>
        new(
            Id: role.Id,
            Name: role.Name
        );
}