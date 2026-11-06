using Infrastructure.Interface;
using Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Infrastructure.Services;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Infrastructure.Interface.Service;

namespace Infrastructure.Interceptors;

public sealed class AuditLogInterceptor: SaveChangesInterceptor
{
    private readonly IUserContextService _userContextService;
    public AuditLogInterceptor(IUserContextService userContextService)
    {
        _userContextService = userContextService;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Console.WriteLine("Intercepter called saving changes");
        var context = eventData.Context;
        if(context is null)
        {
            return base.SavingChanges(eventData, result);
        }
        try
        {
            CreateAuditEntries(context);
        }catch (Exception ex)
        {
            Console.WriteLine($"Error creating audit entries: {ex.Message}");
        }
        
        return base.SavingChanges(eventData, result);
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Console.WriteLine("Intercepter called saving changes async");
        var context = eventData.Context;

        if(context is null)
        {
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }
        
        try
        {
            CreateAuditEntries(context);
        }catch (Exception ex)
        {
            Console.WriteLine($"Error creating audit entries: {ex.Message}");
        }
        
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void CreateAuditEntries(DbContext context)
    {
        string userid = _userContextService.GetCurrentUserId() ?? "Unknown";
        string userAgent = _userContextService.getUserAgent() ?? "Unknown";
        context.ChangeTracker.DetectChanges();

        foreach (var entry in context.ChangeTracker.Entries<IAuditAble>())
        {
            if(entry.Entity is OrganizationAuditLog)
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
                    ActorId = userid,
                    NewData = SerializeNewValues(entry),
                    OldData = SerializeOldValues(entry),
                    Organization = organization,
                    Source = userAgent,
                    Action = entry.State.ToString(),
                };
                context.Set<OrganizationAuditLog>().Add(auditLog);
            }
            else if(entry.Entity is SFTPConnection sftpConnection)
            {
                SFTPConnectionAuditLog auditLog = new SFTPConnectionAuditLog
                {
                    ActorId = userid,
                    NewData = SerializeNewValues(entry),
                    OldData = SerializeOldValues(entry),
                    SFTPConnnection = sftpConnection,
                    Source = userAgent,
                    Action = entry.State.ToString(),
                };
                context.Set<SFTPConnectionAuditLog>().Add(auditLog);
            }
            else if (entry.Entity is AspNetRole role)
            {
                RoleAuditLog auditLog = new RoleAuditLog
                {
                    ActorId = userid,
                    NewData = SerializeNewValues(entry),
                    OldData = SerializeOldValues(entry),
                    Role = role,
                    Source = userAgent,
                    Action = entry.State.ToString(),
                };
                context.Set<RoleAuditLog>().Add(auditLog);
            }
            else if(entry.Entity is UserOrganization userOrgannization)
            {
                UserOrganizationAuditLog auditLog = new UserOrganizationAuditLog
                {
                    ActorId = userid,
                    NewData = SerializeNewValues(entry),
                    OldData = SerializeOldValues(entry),
                    UserOrganization = userOrgannization,
                    Source = userAgent,
                    Action = entry.State.ToString(),
                };
                context.Set<UserOrganizationAuditLog>().Add(auditLog);
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