# Tenant Slug Filtering Fix

## Problem
When accessing `/starsky/main/api/index?f=/`, the API returned a folder structure showing `/main` as a child directory of the root folder, rather than displaying the true contents of the main tenant's root directory.

## Root Cause
After the storage migration from a flat structure (`StorageFolder/`) to a per-tenant structure (`StorageFolder/main/`), directory entries were created in the database for tenant slug directories (e.g., a FileIndexItem with FilePath="/main", ParentDirectory="/"). 

When querying folders at the root level, these tenant slug directory entries were being included in the listing because the database query had no filter to exclude them.

## Solution
Modified three database query methods to exclude directory entries matching the current tenant's slug when querying the root directory (`/`):

### 1. QueryFolder.cs - `QueryDisplayFileFolders`
**File**: `starsky.foundation.database/Query/QueryFolder.cs`

When listing files and folders in the root directory of a tenant, exclude any FileIndexItem where FileName matches the tenant slug (case-insensitive).

**Example**: For the "main" tenant, if there's a FileIndexItem with FileName="main" at ParentDirectory="/", it will be filtered out.

### 2. QueryGetNextPrevInFolder.cs - `QueryGetNextPrevInFolder`
**File**: `starsky.foundation.database/Query/QueryGetNextPrevInFolder.cs`

Applied the same filtering logic to prevent navigating to tenant slug directories when using next/previous navigation at the root level.

### 3. QueryGetFoldersAsync.cs - `GetAllFoldersQuery`
**File**: `starsky.foundation.database/Query/QueryGetFoldersAsync.cs`

Applied the same filtering logic to the folder query builder when getting folders at the root level.

## Implementation Details

The filtering logic checks:
```csharp
if (subPath == "/" && context.TenantContext?.TenantSlug != null)
{
    var tenantSlug = context.TenantContext.TenantSlug;
    items = items.Where(p => !string.Equals(p.FileName, tenantSlug, StringComparison.OrdinalIgnoreCase)).ToList();
}
```

This ensures:
- Only applies when querying **root** directory (`/`)
- Only applies when a **tenant context exists** (multi-tenant scenario)
- Uses **case-insensitive comparison** for tenant slug matching
- Filters out entries where the **FileName matches the tenant slug**

##  Test Coverage

Added new test class: `QueryDisplayFileFoldersTenantSlugFilterTest`

**File**: `starskytest/starsky.foundation.database/QueryTest/QueryFolderTest.cs`

**Test**: `QueryDisplayFileFolders_ExcludesTenantSlugFromRootListing`

Verifies that:
1. A user-created folder (e.g., "photos") appears in the root listing
2. A tenant slug directory (e.g., "main") is excluded from the root listing
3. Only the user folder is returned when querying the root

## Expected Behavior After Fix

For the main tenant, when accessing `/starsky/main/api/index?f=/`:
- **Before**: JSON response showed `/main` as a child directory
- **After**: JSON response shows actual user folders, excluding the "/main" tenant directory

## Files Modified

1. `starsky.foundation.database/Query/QueryFolder.cs` - Line 150-175
2. `starsky.foundation.database/Query/QueryGetNextPrevInFolder.cs` - Line 13-42
3. `starsky.foundation.database/Query/QueryGetFoldersAsync.cs` - Line 49-69
4. `starskytest/starsky.foundation.database/QueryTest/QueryFolderTest.cs` - Added new test class

## Compilation Status

✅ All changes compile successfully without errors
✅ Exception: Non-blocking warnings about code style (e.g., unused using directives)
✅ Database library builds successfully

