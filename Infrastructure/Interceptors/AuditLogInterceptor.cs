using Infrastructure.Interface;
using Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Infrastructure.Services;
using System.Text.Json;

namespace Infrastructure.Interceptors;

public sealed class AuditLogInterceptor: SaveChangesInterceptor
{
    private readonly UserContextService _userContextService;
    public AuditLogInterceptor(UserContextService userContextService)
    {
        _userContextService = userContextService;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Console.WriteLine("Intercepter called saving changes");
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Console.WriteLine("Intercepter called saving changes async");
        var context = eventData.Context;
        if(context is null)
        {
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        CreateAuditEntries(context);
        
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private List<IAuditLog> CreateAuditEntries(DbContext context)
    {
        var entries = new List<IAuditLog>();
        string userid = _userContextService.GetCurrentUserId() ?? "Unknown";
        context.ChangeTracker.DetectChanges();

        foreach (var entry in context.ChangeTracker.Entries<IAuditAble>())
        {
            if (entry.State is EntityState.Detached or EntityState.Unchanged)
            {
                continue;
            }

            if (entry.Entity is Organization organization)
            {             
                var oldVals = entry.Properties.Where(p => p.IsModified && p.OriginalValue != p.CurrentValue).ToDictionary(p => p.Metadata.Name, p => p.OriginalValue);
                var newVals = entry.Properties.Where(p => p.IsModified && p.OriginalValue != p.CurrentValue).ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);
                Console.WriteLine($"Modified properties: {string.Join(", ", oldVals.Select(kv => $"{kv.Key}: {kv.Value}"))}");
                Console.WriteLine($"Modified properties: {string.Join(", ", newVals.Select(kv => $"{kv.Key}: {kv.Value}"))}");
                OrganizationAuditLog auditLog = new OrganizationAuditLog
                {
                    ActorId = userid,
                    NewData = JsonSerializer.Serialize(newVals),
                    OldData = JsonSerializer.Serialize(oldVals),
                    OrganizationId = organization.Id,   
                    Source = "API",
                };

                context.Add(auditLog);
            }

            // switch (entry.Entity)
            // {
            //     case entry.Entity is Organization:

            //         Organization organization = (Organization)entry.Entity;

                    
            //         break;
            //     default:
            //         Console.WriteLine($"Creating audit log for entity type: {entry.Entity.GetType().Name}");
            //         break;
            // }

        }

        return entries;
    }

} 