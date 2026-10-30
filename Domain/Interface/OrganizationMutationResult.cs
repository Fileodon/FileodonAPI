using Infrastructure.Models;

namespace Domain.Interface;

public enum OrganizationMutationStatus
{
    NotFound,
    Forbidden,
    Success
}

public sealed record OrganizationMutationResult(
    OrganizationMutationStatus Status,
    Organization? Organization);
