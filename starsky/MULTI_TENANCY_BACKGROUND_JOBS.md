# Multi-Tenancy in Background Jobs - Architecture Analysis

## Overview
Starsky has a sophisticated multi-tenancy design for background jobs. There are two distinct patterns:
1. **Front-end triggered jobs** - Tenant-specific
2. **Global/watcher triggered jobs** - Expanded to all enabled tenants

---

## 1. Front-End Triggered Jobs (Tenant-Specific)

### Pattern
Jobs triggered by user actions through controllers are **explicitly tenant-specific**.

### Example: GeoController
```csharp
// starsky/Controllers/GeoController.cs (lines 90-102)
await queue.QueueJobAsync(new BackgroundTaskQueueJob
{
    MetaData = f,
    TraceParentId = Activity.Current?.Id,
    TenantId = tenantContext?.TenantId,              // ✅ EXPLICITLY SET
    TenantSlug = tenantContext?.TenantSlug,          // ✅ EXPLICITLY SET
    PriorityLane = ProcessTaskQueue.PriorityLaneUpdate,
    JobType = GeoSyncBackgroundJobHandler.GeoSync,
    PayloadJson = JsonSerializer.Serialize(...)
});
```

### Key Points
- Controllers receive `ITenantContext` via dependency injection
- The tenant context is populated from HTTP request context (JWT/cookie claims)
- Each job explicitly carries `TenantId` and `TenantSlug`
- Examples:
  - SmallThumbnailBackgroundJobService (lines 51-52)
  - ManualThumbnailGenerationService (lines 49-50)
  - Any POST/action endpoint

---

## 2. Global/Watcher Triggered Jobs (Multi-Tenant Expansion)

### Pattern
Jobs triggered by the **DiskWatcher (sync watcher)** are NOT initially tenant-specific. They get expanded to all enabled tenants.

### Example: DiskWatcher → QueueProcessor
```csharp
// starsky.foundation.sync/WatcherHelpers/QueueProcessor.cs (lines 52-62)
await _bgTaskQueue.QueueJobAsync(new BackgroundTaskQueueJob
{
    MetaData = $"from:{filepath}",
    TraceParentId = Activity.Current?.Id,
    TenantId = _tenantContext?.TenantId,             // ⚠️ Usually NULL (global context)
    TenantSlug = _tenantContext?.TenantSlug,         // ⚠️ Usually NULL
    PriorityLane = ProcessTaskQueue.PriorityLaneDiskWatcher,
    JobType = JobType,
    PayloadJson = JsonSerializer.Serialize(payload)
});
```

### Tenant Expansion Mechanism: QueueJobTenantEnforcer

Both `UpdateBackgroundTaskQueue` and `DiskWatcherBackgroundTaskQueue` use the same enforcement:

```csharp
// starsky.foundation.worker/Services/UpdateBackgroundTaskQueue.cs (lines 42-58)
public async ValueTask QueueJobAsync(BackgroundTaskQueueJob job)
{
    // ... validation ...
    
    using var scope = _scopeFactory.CreateScope();
    var logger = scope.ServiceProvider.GetService<IWebLogger>();
    
    // THIS IS THE KEY: Expands jobs to all tenants if needed
    var queuedJobs = await QueueJobTenantEnforcer.ExpandForTenantCoverageAsync(
        job, scope.ServiceProvider, logger, QueueName);
        
    foreach ( var queuedJob in queuedJobs )
    {
        await _backend.QueueJobAsync(queuedJob);
    }
}
```

### QueueJobTenantEnforcer Logic

```csharp
// starsky.foundation.worker/Helpers/QueueJobTenantEnforcer.cs (lines 15-72)
public static async Task<IReadOnlyList<BackgroundTaskQueueJob>> ExpandForTenantCoverageAsync(
    BackgroundTaskQueueJob job,
    IServiceProvider services,
    IWebLogger? logger,
    string queueName)
{
    // Step 1: If job already has tenant metadata, return as-is
    if ( job.TenantId.HasValue && !string.IsNullOrWhiteSpace(job.TenantSlug) )
    {
        return [ job ];  // ✅ Tenant-specific job
    }

    // Step 2: Try to get tenant from current context
    logger?.LogError($"[{queueName}] Background job producer omitted tenant metadata...");
    
    var tenantContext = services.GetService<ITenantContext>();
    var tenantId = tenantContext?.TenantId;
    var tenantSlug = tenantContext?.TenantSlug;

    if ( tenantId.HasValue && !string.IsNullOrWhiteSpace(tenantSlug) )
    {
        return [ Clone(job, tenantId.Value, tenantSlug) ];  // ✅ From context
    }

    // Step 3: If still no tenant, expand to ALL enabled tenants
    var db = services.GetService<ApplicationDbContext>();
    var enabledTenants = await db.Tenants.AsNoTracking()
        .Where(t => t.IsEnabled)
        .Select(t => new { t.Id, t.Slug })
        .ToListAsync();

    logger?.LogInformation(
        $"[{queueName}] Expanding job '{job.JobType}' to {enabledTenants.Count} tenants.");

    // 🔑 CREATES A SEPARATE JOB FOR EACH TENANT
    return enabledTenants
        .Select(t => Clone(job, t.Id, t.Slug))
        .ToList();
}
```

---

## 3. Execution: Context Propagation

When background tasks execute, the tenant context is properly set:

```csharp
// starsky.foundation.worker/Helpers/ProcessTaskQueue.cs (lines 167-194)
internal static async Task<bool> TryExecuteViaRegisteredHandlersAsync(
    IServiceScopeFactory? scopeFactory,
    BackgroundTaskQueueJob queueJob,
    CancellationToken cancellationToken)
{
    using var scope = scopeFactory.CreateScope();
    
    // 🔑 SET TENANT CONTEXT FROM JOB
    var tenantContext = scope.ServiceProvider.GetService<ITenantContext>();
    if ( tenantContext != null )
    {
        tenantContext.TenantId = queueJob.TenantId;      // ✅ Set from job
        tenantContext.TenantSlug = queueJob.TenantSlug;  // ✅ Set from job
    }

    // Execute handler with proper tenant context
    var handlers = scope.ServiceProvider.GetServices<IBackgroundJobHandler>();
    var handler = handlers.FirstOrDefault(h => h.JobType == queueJob.JobType);
    
    await handler.ExecuteAsync(queueJob.PayloadJson, cancellationToken);
}
```

---

## 4. Data Model: BackgroundTaskQueueJob

```csharp
// starsky.foundation.worker/Models/BackgroundTaskQueueJob.cs
public sealed class BackgroundTaskQueueJob
{
    public Guid JobId { get; init; } = Guid.NewGuid();
    public string? MetaData { get; init; }
    public string? TraceParentId { get; init; }
    
    // 🔑 TENANT METADATA
    public int? TenantId { get; init; }           // Can be NULL initially
    public string? TenantSlug { get; init; }      // Can be NULL initially
    
    public int PriorityLane { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public string? JobType { get; init; }
    public string? PayloadJson { get; init; }
}
```

---

## Summary Table

| Job Source | TenantId Set | TenantSlug Set | Behavior | Examples |
|-----------|:----------:|:----------:|----------|----------|
| **Front-End Controllers** | ✅ YES | ✅ YES | Single tenant (as created) | Geo sync, thumbnail generation, export |
| **DiskWatcher (Sync)** | ❌ NO | ❌ NO | **Auto-expanded to ALL enabled tenants** | Filesystem changes → Sync jobs |
| **Background Services** | ✅ YES | ✅ YES | Single tenant (if context set) | Manual operations |

---

## Design Pattern: "Global Job Awareness"

### The Problem (Before Enforcement)
- DiskWatcher runs as a global singleton
- It doesn't know about individual tenants
- If it created jobs without tenant info, they would process globally

### The Solution (QueueJobTenantEnforcer)
- **Intercepts** all job enqueue operations
- **Checks** if job has tenant metadata
- **Expands** tenant-agnostic jobs to all enabled tenants
- Each expanded job is **isolated** to its tenant

### Benefits
✅ DiskWatcher code remains simple (single-tenant logic)  
✅ Multi-tenant support is automatic via enforcer  
✅ Front-end jobs remain tenant-specific (preserves security)  
✅ Easy to audit (clear logging of expansion)

---

## Potential Issues to Monitor

### 1. ⚠️ Large Tenant Expansion
If DiskWatcher detects many file changes with 100+ tenants enabled:
- Each change creates 100+ background jobs
- Queue could grow very large
- Performance impact on database (expand query runs for each job)

### 2. ⚠️ Consistency Across Tenants
When DiskWatcher fires a job expanded to all tenants:
- All tenants process the same file change
- If file is actually in one tenant's storage, other tenants will see "file not found"
- Handlers should gracefully handle missing files

### 3. ⚠️ Context Availability During Expansion
If `ApplicationDbContext` is unavailable during expansion:
- Job is REJECTED (hard error)
- Prevents DiskWatcher from progressing
- Logged as critical error

---

## Recommended Improvements

### 1. Storage-Based Tenant Isolation
Instead of expanding to ALL enabled tenants, expand only to tenants whose storage path contains the changed file:
```csharp
var tenantsForPath = await GetTenantsOwningPath(changedFilePath);
// Only create jobs for relevant tenants
```

### 2. Conditional DiskWatcher Per Tenant
Consider running separate DiskWatcher instances per tenant storage folder:
```csharp
foreach (var tenant in enabledTenants)
{
    StartDiskWatcher(tenant.StoragePath, tenant.Id, tenant.Slug);
}
```

### 3. Metrics & Monitoring
Track expansion metrics:
```csharp
logger?.LogMetric("DiskWatcherJobExpansion", tenantCount);
logger?.LogMetric("BackgroundJobQueueSize", queue.Count());
```

### 4. Rate Limiting Per Tenant
Prevent one tenant's disk activity from overwhelming others:
```csharp
var queuedJobsForTenant = queue.Count(t => t.TenantId == tenantId);
if (queuedJobsForTenant > MAX_PER_TENANT) {
    logger?.LogWarning($"Queue full for tenant {tenantId}");
    return; // Skip this job
}
```

---

## Files Involved

### Core Multi-Tenancy Support
- `starsky.foundation.worker/Models/BackgroundTaskQueueJob.cs` - Contains TenantId/TenantSlug
- `starsky.foundation.worker/Helpers/QueueJobTenantEnforcer.cs` - **Main enforcement logic**
- `starsky.foundation.worker/Helpers/ProcessTaskQueue.cs` - Execution with context propagation
- `starsky.foundation.platform/Interfaces/ITenantContext.cs` - Tenant context interface

### Queue Services
- `starsky.foundation.worker/Services/UpdateBackgroundTaskQueue.cs` - Enforcer usage
- `starsky.foundation.sync/WatcherBackgroundService/DiskWatcherBackgroundTaskQueue.cs` - Enforcer usage

### Job Producers
- `starsky/Controllers/GeoController.cs` - Front-end job with tenant
- `starsky.feature.thumbnail/Services/SmallThumbnailBackgroundJobService.cs` - Frontend job with tenant
- `starsky.foundation.sync/WatcherHelpers/QueueProcessor.cs` - Watcher job (no tenant initially)

### Background Handlers
- `starsky.feature.geolookup/Services/GeoSyncBackgroundJobHandler.cs`
- `starsky.feature.thumbnail/Services/SmallThumbnailBackgroundJobHandler.cs`
- All handlers receive ITenantContext with tenant set during execution

