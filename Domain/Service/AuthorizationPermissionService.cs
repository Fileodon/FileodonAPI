using Domain.Interface;
using Infrastructure;
using Infrastructure.Models;
using Infrastructure.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Domain.Service;

public class AuthorizationPermissionService
    : IAuthorizationPermissionService
{
    private readonly ApplicationDbContext _context;

    public AuthorizationPermissionService(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> HasPermissionAsync(
     string userId,
     int organizationId,
     PermissionFlags permission)
    {
        if (string.IsNullOrWhiteSpace(userId) || organizationId <= 0)
            return false;

        // min. en permission skal være sat, og der må ikke være ukendte permission bits.
        if (permission == PermissionFlags.None ||
            (permission & ~PermissionFlags.All) != PermissionFlags.None)
        {
            return false;
        }

        var permissionMasks = await _context
            .Set<UserOrganization>()
            .AsNoTracking()
            .Where(membership =>
                membership.UserId == userId &&
                membership.OrganizationId == organizationId &&
                membership.Role.OrganizationId == organizationId)
            .Select(membership => membership.Role.PermissionMask)
            .Take(2)
            .ToListAsync();

        // én rolle per bruger/organisation.
        if (permissionMasks.Count != 1)
            return false;

        var permissionMask = permissionMasks[0];
        var knownMask = (long)PermissionFlags.All;
        var requiredMask = (long)permission;

        // Afvis ugyldige masker.
        if (permissionMask < 0 || (permissionMask & ~knownMask) != 0)
            return false;

        return (permissionMask & requiredMask) == requiredMask;
    }
}