using System.Text.Json;
using Infrastructure.Interface;
using Infrastructure.Interface.Service;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Infrastructure.Interceptors;

public sealed class AuditLogInterceptor : SaveChangesInterceptor
{
    private readonly IUserContextService _userContextService;
    public AuditLogInterceptor(IUserContextService userContextService)
    {
        _userContextService = userContextService;
    }

    private List<OrganizationAuditLog> organizationAuditLogs = new List<OrganizationAuditLog>();
    private List<SFTPConnectionAuditLog> sftpConnectionAuditLogs = new List<SFTPConnectionAuditLog>();
    private List<RoleAuditLog> roleAuditLogs = new List<RoleAuditLog>();
    private List<UserOrganizationAuditLog> userOrganizationAuditLogs = new List<UserOrganizationAuditLog>();

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        var context = eventData.Context;
        if (context is null)
        {
            return base.SavingChanges(eventData, result);
        }
        try
        {
            CreateAuditEntries(context.ChangeTracker);
            context.Set<OrganizationAuditLog>().AddRange(organizationAuditLogs);
            context.Set<SFTPConnectionAuditLog>().AddRange(sftpConnectionAuditLogs);
            context.Set<RoleAuditLog>().AddRange(roleAuditLogs);
            context.Set<UserOrganizationAuditLog>().AddRange(userOrganizationAuditLogs);
            organizationAuditLogs.Clear();
            sftpConnectionAuditLogs.Clear();
            roleAuditLogs.Clear();
            userOrganizationAuditLogs.Clear();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating audit entries: {ex.Message}");
        }

        return base.SavingChanges(eventData, result);
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;

        if (context is null)
        {
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        try
        {
            CreateAuditEntries(context.ChangeTracker);
            context.Set<OrganizationAuditLog>().AddRange(organizationAuditLogs);
            context.Set<SFTPConnectionAuditLog>().AddRange(sftpConnectionAuditLogs);
            context.Set<RoleAuditLog>().AddRange(roleAuditLogs);
            context.Set<UserOrganizationAuditLog>().AddRange(userOrganizationAuditLogs);
            organizationAuditLogs.Clear();
            sftpConnectionAuditLogs.Clear();
            roleAuditLogs.Clear();
            userOrganizationAuditLogs.Clear();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating audit entries: {ex.Message}");
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void CreateAuditEntries(ChangeTracker changeTracker)
    {
        string userid = _userContextService.GetCurrentUserId() ?? "Unknown";
        string userAgent = _userContextService.getUserAgent() ?? "Unknown";
        changeTracker.DetectChanges();

        foreach (var entry in changeTracker.Entries<IAuditAble>())
        {
            if (entry.Entity is OrganizationAuditLog)
            {
                continue;
            }

            if (entry.State is EntityState.Detached or EntityState.Unchanged)
            {
                Console.WriteLine($"Continuing, entry.State: {entry.State}");
                continue;
            }

            if (entry.Entity is Organization organization)
            {
                OrganizationAuditLog auditLog = new OrganizationAuditLog
                {
                    Id = Guid.NewGuid(),
                    ActorId = userid,
                    NewData = SerializeNewValues(entry),
                    OldData = SerializeOldValues(entry),
                    Organization = organization,
                    Source = userAgent,
                    Action = entry.State.ToString(),
                };
                organizationAuditLogs.Add(auditLog);
            }
            else if (entry.Entity is SFTPConnection sftpConnection)
            {
                SFTPConnectionAuditLog auditLog = new SFTPConnectionAuditLog
                {
                    Id = Guid.NewGuid(),
                    ActorId = userid,
                    NewData = SerializeNewValues(entry),
                    OldData = SerializeOldValues(entry),
                    SFTPConnnection = sftpConnection,
                    Source = userAgent,
                    Action = entry.State.ToString(),
                };
                sftpConnectionAuditLogs.Add(auditLog);
            }
            else if (entry.Entity is AspNetRole role)
            {
                RoleAuditLog auditLog = new RoleAuditLog
                {
                    Id = Guid.NewGuid(),
                    ActorId = userid,
                    NewData = SerializeNewValues(entry),
                    OldData = SerializeOldValues(entry),
                    Role = role,
                    Source = userAgent,
                    Action = entry.State.ToString(),
                };
                roleAuditLogs.Add(auditLog);
            }
            else if (entry.Entity is UserOrganization userOrgannization)
            {
                UserOrganizationAuditLog auditLog = new UserOrganizationAuditLog
                {
                    Id = Guid.NewGuid(),
                    ActorId = userid,
                    NewData = SerializeNewValues(entry),
                    OldData = SerializeOldValues(entry),
                    UserOrganization = userOrgannization,
                    Source = userAgent,
                    Action = entry.State.ToString(),
                };
                userOrganizationAuditLogs.Add(auditLog);
            }

        }
    }

    private string SerializeOldValues(EntityEntry<IAuditAble> entry)
    {
        var vals = entry.Properties
            .Where(p => p.IsModified && p.OriginalValue != p.CurrentValue)
            .ToDictionary(p => p.Metadata.Name, p => p.OriginalValue);
        return JsonSerializer.Serialize(vals);
    }
    private string SerializeNewValues(EntityEntry<IAuditAble> entry)
    {
        var vals = entry.Properties
            .Where(p => p.IsModified && p.OriginalValue != p.CurrentValue)
            .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);
        return JsonSerializer.Serialize(vals);
    }
}