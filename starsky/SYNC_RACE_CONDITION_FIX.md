# Sync Race Condition Fix - Summary

## Problem Description

During sync operations (manual sync or sync on startup), folders with content were sometimes being deleted from the database. This occurred due to a race condition in the parallel folder synchronization logic.

## Root Cause

The issue was in `SyncFolder.cs` in the `CheckIfFolderExistOnDisk()` method:

1. **Parallel Processing Race Condition**: Multiple threads were checking folders simultaneously using `MaxDegreesOfParallelism`
2. **Insufficient Validation**: The method only checked if a folder existed on disk before deletion, without verifying:
   - Whether child items were being added to the database by another thread
   - Whether subdirectories existed
3. **Timing Issue**: 
   - Thread A: Starts syncing parent folder `/photos`, finds it doesn't exist (or is being created)
   - Thread B: Creates folder `/photos` and adds child items to database
   - Thread A: Proceeds to delete all items Thread B just added, thinking the folder is empty

## Changes Made

### 1. Enhanced `CheckIfFolderExistOnDisk()` Method

**File**: `starsky.foundation.sync/SyncServices/SyncFolder.cs`

Added safety checks before folder deletion to prevent race conditions:

```csharp
// Double-check: verify folder doesn't exist
if ( !_subPathStorage.ExistFolder(item.FilePath!) )
{
    var queryFactory = new QueryFactory(_setupDatabaseTypes,
        _query, _memoryCache, _appSettings,
        _serviceScopeFactory, _logger);
    var query = queryFactory.Query();
    
    if ( query == null )
    {
        return null;
    }
    
    // Check if any subdirectories exist on disk
    var subDirectories = _subPathStorage.GetDirectoryRecursive(item.FilePath!);
    
    // Skip deletion only if subdirectories exist on disk
    // This indicates the folder structure is being actively written
    if ( subDirectories.Any() )
    {
        _logger.LogInformation(
            $"[SyncFolder] Skipping deletion of {item.FilePath} - subdirectories exist on disk");
        await query.DisposeAsync();
        return null;
    }
    
    // Folder doesn't exist and no subdirectories - safe to delete
    return await RemoveChildItems(query, item);
}
else
{
    // Folder exists now, don't remove
    return null;
}
```

**Key improvements**:
- Double-check folder existence before proceeding with deletion
- Check for subdirectories on disk (catches folders being actively written by parallel threads)
- Skip deletion if subdirectories exist, indicating active folder structure creation
- Log when skipping deletion for debugging
- Proceed with deletion if folder truly doesn't exist and no subdirectories are present
- Child items in database are deleted when folder doesn't exist (cleaning up stale DB entries)

### 2. Added Final Safety Check in `RemoveChildItems()` Method

Added a last-minute verification before actually removing items:

```csharp
internal async Task<FileIndexItem> RemoveChildItems(IQuery query, FileIndexItem item)
{
    // Final safety check before deletion - verify folder truly doesn't exist
    if ( _subPathStorage.ExistFolder(item.FilePath!) )
    {
        _logger.LogInformation($"[SyncFolder] Aborting RemoveChildItems - folder exists: {item.FilePath}");
        item.Status = FileIndexItem.ExifStatus.Ok;
        await query.DisposeAsync();
        return item;
    }

    // Child items within
    var removeItems = await _query.GetAllRecursiveAsync(item.FilePath!);
    
    _logger.LogInformation($"[SyncFolder] Removing {removeItems.Count} child items from {item.FilePath}");
    
    // ... rest of removal logic
}
```

**Key improvements**:
- Final check right before deletion (catches folders created since the earlier check)
- Logging for audit trail
- Abort deletion if folder now exists

## Benefits

1. **Prevents Data Loss**: Folders with content will no longer be incorrectly deleted
2. **Race Condition Mitigation**: Multiple defensive checks at different stages
3. **Better Observability**: Logging helps track when deletions are skipped and why
4. **No Breaking Changes**: The fix is additive - only adds safety checks without changing the API

## Testing Recommendations

1. **Manual Testing**:
   - Run sync on a large folder structure with `MaxDegreesOfParallelism` > 1
   - Monitor logs for "[SyncFolder] Skipping deletion" messages
   - Verify folders with content are not deleted

2. **Concurrent Testing**:
   - Start a manual sync while disk watcher is active
   - Create folders with content during sync operation
   - Verify no items are lost

3. **Startup Sync Testing**:
   - Enable sync on startup
   - Ensure folders with recently added content are preserved

## Related Configuration

The `MaxDegreesOfParallelism` setting in `AppSettings` controls how many folders are processed in parallel. Higher values increase performance but also increase the likelihood of race conditions (now mitigated by this fix).

## Monitoring

Watch for these log messages:
- `[SyncFolder] Skipping deletion of {path} - subdirectories exist on disk` - Normal, indicates the fix is working (folder structure is being actively created)
- `[SyncFolder] Aborting RemoveChildItems - folder exists: {path}` - Rare, indicates a folder was created between checks (second line of defense)
- `[SyncFolder] Removing {count} child items from {path}` - Normal deletion of folders that don't exist and their stale database entries

## Related Issues

This fix addresses issues mentioned in the changelog:
- Version 0.4.6: "Delete floating folders in database on scan synchronize"
- Version 0.4.5: "When remove a folder, the files within the folder are still in the database"
- Multiple fixes related to `UseDiskWatcher` race conditions

