# NoAccountMiddleware Multi-Tenant Fix

## Problem
The `NoAccountMiddleware` (used for localhost development) did not work with the new multi-tenant architecture. It was creating a localhost user but not associating them with any tenant, causing authentication failures in multi-tenant mode.

## Solution
Updated the middleware to:
1. Create the default "main" tenant if it doesn't exist
2. Automatically add the localhost user as an Admin member of the main tenant
3. Ensure tenant associations are in place before login

## Changes Made

### File: `starsky.foundation.accountmanagement/Middleware/NoAccountLocalhostMiddleware.cs`

#### 1. Added Imports
```csharp
using Microsoft.EntityFrameworkCore;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models.Account;
```

#### 2. Updated Invoke Method
Added retrieval of `ApplicationDbContext` from DI:
```csharp
var dbContext = (ApplicationDbContext) context.RequestServices
    .GetRequiredService(typeof(ApplicationDbContext));
```

Pass dbContext to `CreateOrUpdateNewUsers`:
```csharp
var user = await CreateOrUpdateNewUsers(userManager, dbContext);
```

#### 3. Updated CreateOrUpdateNewUsers Method Signature
Changed from:
```csharp
internal static async Task<User?> CreateOrUpdateNewUsers(IUserManager userManager)
```

To:
```csharp
internal static async Task<User?> CreateOrUpdateNewUsers(IUserManager userManager, ApplicationDbContext dbContext)
```

#### 4. Added Tenant Setup Call
After user creation, ensure tenant association:
```csharp
// Handle multi-tenancy: ensure user is in main tenant
if (user != null)
{
    await EnsureUserInMainTenant(user, dbContext);
}
```

#### 5. New Helper Method: EnsureUserInMainTenant
```csharp
private static async Task EnsureUserInMainTenant(User user, ApplicationDbContext dbContext)
{
    // Ensure main tenant exists
    var mainTenant = await dbContext.Tenants.FirstOrDefaultAsync(t => t.Slug == "main");
    if (mainTenant == null)
    {
        mainTenant = new Tenant
        {
            Slug = "main",
            Name = "main",
            IsEnabled = true
        };
        dbContext.Tenants.Add(mainTenant);
        await dbContext.SaveChangesAsync();
    }

    // Check if user is already a member of main tenant
    var existingMembership = await dbContext.TenantUsers
        .FirstOrDefaultAsync(tu => tu.TenantId == mainTenant.Id && tu.UserId == user.Id);

    if (existingMembership == null)
    {
        // Add user to main tenant as admin for localhost development
        var tenantUser = new TenantUser
        {
            TenantId = mainTenant.Id,
            UserId = user.Id,
            Role = TenantRole.Admin
        };
        dbContext.TenantUsers.Add(tenantUser);
        await dbContext.SaveChangesAsync();
    }
}
```

## Behavior

### On First Localhost Access
1. ✅ Creates default "main" tenant (if needed)
2. ✅ Creates `mail@localhost` user (if needed)
3. ✅ Adds user as Admin member of "main" tenant (if not already)
4. ✅ Auto-logs in the user

### Result
- Localhost user can access all features
- User is properly associated with "main" tenant
- Multi-tenant filtering works correctly
- Files are properly scoped to the tenant

## Development Usage

### Before (Single-Tenant)
- Auto-login on localhost: ✅
- User authenticated: ✅
- Access to all files: ✅

### After (Multi-Tenant)
- Auto-login on localhost: ✅
- User authenticated: ✅
- User in "main" tenant: ✅
- Access to tenant-scoped files: ✅

## Security Note
This middleware is only active when:
- `NoAccountLocalhost` setting is enabled (default for local development)
- **OR** `DemoUnsafeDeleteStorageFolder` is true
- **AND** request is from localhost

It does NOT apply to:
- Production deployments (unless explicitly enabled)
- Remote/network requests
- API calls or realtime connections
- Logout requests

## Testing

To verify the fix works:

```csharp
// Test 1: Localhost access without authentication
// Expected: Auto-logged in as mail@localhost

// Test 2: Check user permissions
// Expected: User has Admin role in main tenant

// Test 3: Access file via API
// Expected: File accessible (filtered to main tenant)

// SQL verification:
SELECT * FROM Tenants WHERE Slug = 'main';
SELECT * FROM Users WHERE Credentials.Identifier = 'mail@localhost';
SELECT * FROM TenantUsers 
WHERE UserId = (SELECT Id FROM Users WHERE ...)
  AND TenantId = (SELECT Id FROM Tenants WHERE Slug = 'main');
```

## Impact
- ✅ Localhost development fully restored
- ✅ Multi-tenant environment compatible
- ✅ No breaking changes
- ✅ Backward compatible
- ✅ Automatic tenant setup

## Related Files
- `TenantsController.cs` - Tenant creation API
- `TenantSessionAuthenticationMiddleware.cs` - Tenant session handling
- `USER_MIGRATION_MULTI_TENANCY.md` - User migration documentation


