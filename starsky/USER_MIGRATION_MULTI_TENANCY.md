# User Migration for Multi-Tenancy

## Overview
When migrating from single-tenant to multi-tenant architecture, existing users (especially global admins) need to be automatically added to the default "main" tenant so they maintain access to existing data.

## Changes Made

### 1. Database Migration Update
**File:** `starsky.foundation.database/Migrations/20260430121000_TenantIsolationPhase1Backfill.cs`

**Added:** Automatic migration of existing global admins to the main tenant

```sql
-- Migrate existing global admins to the 'main' tenant as admins
INSERT INTO TenantUsers (TenantId, UserId, Role)
SELECT 
    (SELECT Id FROM Tenants WHERE Slug = 'main' LIMIT 1),
    u.Id,
    1  -- TenantRole.Admin = 1
FROM Users u
WHERE u.IsGlobalAdmin = 1 
  AND NOT EXISTS (
    SELECT 1 FROM TenantUsers tu 
    WHERE tu.TenantId = (SELECT Id FROM Tenants WHERE Slug = 'main' LIMIT 1) 
      AND tu.UserId = u.Id
  );
```

**What it does:**
- ✅ Creates default "main" tenant if it doesn't exist
- ✅ Assigns all existing files to "main" tenant
- ✅ Assigns all existing queue items to "main" tenant
- ✅ **NEW:** Adds all existing global admins as Admin members of "main" tenant
- ✅ Prevents duplicate entries with EXISTS check

### 2. Create Tenant Endpoint Update
**File:** `starsky/Controllers/TenantsController.cs`

**Enhanced Create Method:**

When creating a new tenant, if it's the **first tenant being created**, all existing global admins are automatically added as Admin members:

```csharp
// Check if this is the first tenant being created
var tenantCount = await dbContext.Tenants.CountAsync();
var isFirstTenant = tenantCount == 1;

// Add creator as admin of the new tenant
var tenantUser = new TenantUser
{
    TenantId = newTenant.Id,
    UserId = user.Id,
    Role = TenantRole.Admin
};

dbContext.TenantUsers.Add(tenantUser);

// If this is the first tenant, add all existing global admins as admins of this tenant
if (isFirstTenant)
{
    var globalAdmins = await dbContext.Users
        .Where(u => u.IsGlobalAdmin && u.Id != user.Id)
        .ToListAsync();

    foreach (var globalAdmin in globalAdmins)
    {
        dbContext.TenantUsers.Add(new TenantUser
        {
            TenantId = newTenant.Id,
            UserId = globalAdmin.Id,
            Role = TenantRole.Admin
        });
    }
}
```

**Benefits:**
- ✅ First tenant automatically populated with all global admins
- ✅ Ensures existing admins maintain access
- ✅ Only applies to first tenant (doesn't affect subsequent tenants)

## Migration Scenario Example

### Before Migration (Single-Tenant)
```
Users Table:
- ID: 1   Name: Alice   IsGlobalAdmin: true
- ID: 2   Name: Bob     IsGlobalAdmin: true
- ID: 3   Name: Charlie IsGlobalAdmin: false

Files (all in shared storage):
- /photos/2023/vacation.jpg
- /photos/2024/birthday.jpg
```

### After Migration (Multi-Tenant)
**Instant (via migration):**
```
Tenants Table:
- ID: 1   Slug: main   IsEnabled: true

FileIndex Table:
- /photos/2023/vacation.jpg   TenantId: 1
- /photos/2024/birthday.jpg   TenantId: 1

TenantUsers Table:
- TenantId: 1  UserId: 1 (Alice)    Role: Admin
- TenantId: 1  UserId: 2 (Bob)      Role: Admin
```

### After User Creates Second Tenant
```
Tenants Table:
- ID: 1   Slug: main         IsEnabled: true
- ID: 2   Slug: family       IsEnabled: true

TenantUsers Table (updated):
- TenantId: 1  UserId: 1 (Alice)    Role: Admin
- TenantId: 1  UserId: 2 (Bob)      Role: Admin
- TenantId: 2  UserId: 1 (Alice)    Role: Admin  [creator auto-added]
- TenantId: 2  UserId: 2 (Bob)      Role: Admin  [first tenant migration auto-added]
```

## User Experience

### For Existing Global Admins
1. After migration, they can:
   - Access all their existing files via the "main" tenant
   - Create new additional tenants
   - Manage multiple tenants

### For Non-Admin Users
- They remain as regular Users (not admins)
- They need to be manually added to tenants by admins
- They access the "main" tenant if admin adds them

## Permission Levels

| User Type | Main Tenant | New Tenant 1 | New Tenant 2 |
|-----------|------------|-------------|-------------|
| Global Admin | ✅ Admin (auto) | ✅ Admin (if creator) | ✅ Admin (if creator) |
| Regular User | ❌ None (manual add) | ❌ None (manual add) | ❌ None (manual add) |

## Backward Compatibility

✅ **Fully backward compatible:**
- Existing data preserved
- Existing file references intact
- All users maintain their previous permissions
- Optional: can create additional tenants later

## Error Handling

The migration includes safeguards:
- ✅ Only inserts "main" tenant if it doesn't exist
- ✅ Only assigns users if they're not already assigned
- ✅ Handles non-existent Users/Tenants gracefully
- ✅ Transaction support (all-or-nothing)

## Testing the Migration

1. **Before running migration:**
   - Backup database
   - Note user IDs and global admin flags
   - Record file ownership

2. **After running migration:**
   - Check Tenants table has "main" entry
   - Verify all files have TenantId assigned
   - Check TenantUsers has all global admins as admins
   - Login as global admin and verify file access

3. **Test tenant creation:**
   - Login as global admin
   - Create new tenant
   - Verify all other global admins can access it

## SQL Verification Queries

```sql
-- Check migration status
SELECT * FROM Tenants;              -- Should have "main" tenant
SELECT COUNT(*) FROM FileIndex WHERE TenantId IS NULL;  -- Should be 0
SELECT COUNT(*) FROM QueueItems WHERE TenantId IS NULL; -- Should be 0

-- Check user assignments
SELECT u.Id, u.Name, u.IsGlobalAdmin, t.Slug, tu.Role
FROM TenantUsers tu
JOIN Users u ON tu.UserId = u.Id
JOIN Tenants t ON tu.TenantId = t.Id
ORDER BY u.Id, t.Slug;

-- Check global admin migrations specifically
SELECT u.Id, u.Name, COUNT(tu.Id) as TenantCount
FROM Users u
LEFT JOIN TenantUsers tu ON u.Id = tu.UserId
WHERE u.IsGlobalAdmin = 1
GROUP BY u.Id, u.Name;
```

## Related Documentation
- [Create Tenant Feature](./CREATE_TENANT_FEATURE.md)
- [Multi-Tenancy Architecture](./MULTI_TENANCY_BACKGROUND_JOBS.md)


