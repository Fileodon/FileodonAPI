using Infrastructure.Interface;
using Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Infrastructure.Interceptors;

public sealed class AuditLogInterceptor: SaveChangesInterceptor
{
    private readonly UserManager<User> _userManager;

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

        var auditEntries = CreateAuditEntries(context);


        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private List<IAuditLog> CreateAuditEntries(DbContext context)
    {
        var entries = new List<IAuditLog>();

        context.ChangeTracker.DetectChanges();

        foreach (var entry in context.ChangeTracker.Entries<IAuditLog>())
        {
            if (entry.State is EntityState.Detached or EntityState.Unchanged)
            {
                continue;
            }

            switch (entry.Entity.GetType().Name)
            {
                case nameof(Organization):
                    Console.WriteLine("Creating audit log for Organization entity");
                    break;
                default:
                    Console.WriteLine($"Creating audit log for entity type: {entry.Entity.GetType().Name}");
                    break;
            }

        }

        return entries;
    }
} 