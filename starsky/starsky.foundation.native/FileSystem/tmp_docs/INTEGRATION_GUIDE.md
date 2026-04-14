# Integration Guide: Using macOS Security-Scoped Bookmarks in Starsky

## When to Use

Use `MacOsSecurityScopedBookmark` when you need to:

1. **Allow child processes to access files** that the parent process has permission to access
2. **Maintain security sandbox compliance** on macOS
3. **Pass file access permissions** via command-line arguments or configuration

## Quick Start

### Step 1: Create Bookmark in Parent/UI Context

```csharp
using starsky.foundation.native.FileSystem;

// When user selects a folder (e.g., via file dialog)
var selectedPath = "/Users/myuser/Pictures/MyPhotos";

if (MacOsSecurityScopedBookmark.TryCreateBookmark(selectedPath, out var bookmarkBase64))
{
    // Store or pass bookmarkBase64 to child process
    _logger.LogInformation($"Bookmark created successfully");
}
else
{
    _logger.LogError("Failed to create bookmark");
}
```

### Step 2: Pass to Child Process

```csharp
var processInfo = new ProcessStartInfo
{
    FileName = "starsky-backend",
    Arguments = $"--data-folder-bookmark {bookmarkBase64}"
};

Process.Start(processInfo);
```

### Step 3: Child Process Resolves & Activates Access

```csharp
// In child process startup
if (MacOsSecurityScopedBookmark.TryResolveAndStartAccess(bookmarkFromArgs, out var resolvedPath))
{
    _logger.LogInformation($"Storage folder resolved to: {resolvedPath}");
    
    // Now you can access files in resolvedPath
    var files = Directory.GetFiles(resolvedPath);
    
    // When shutting down, stop access
    MacOsSecurityScopedBookmark.StopAccess(resolvedPath);
}
else
{
    _logger.LogError("Failed to resolve bookmark - no file access");
    Environment.Exit(1);
}
```

## Common Patterns

### Pattern 1: Desktop App → Backend Service

**Parent (Desktop UI):**

```csharp
// User selects folder via file dialog
MacOsSecurityScopedBookmark.TryCreateBookmark(userSelectedPath, out var bookmark);

// Start backend with bookmark
var args = new[] { "--storage-bookmark", bookmark };
StartBackendService(args);
```

**Child (Backend Service):**

```csharp
var bookmarkArg = args.FirstOrDefault(a => a.StartsWith("--storage-bookmark"));
if (bookmarkArg != null)
{
    var bookmark = bookmarkArg.Substring("--storage-bookmark".Length).Trim();
    MacOsSecurityScopedBookmark.TryResolveAndStartAccess(bookmark, out var path);
    _storagePath = path;
}
```

### Pattern 2: Configuration File Storage

**Parent:**

```csharp
// Store bookmark in AppSettings
var config = new AppSettings();
config.StorageBookmark = bookmarkBase64;
await SaveConfigAsync(config);
```

**Child:**

```csharp
// Load bookmark from config
var config = await LoadConfigAsync();
if (config.StorageBookmark != null)
{
    MacOsSecurityScopedBookmark.TryResolveAndStartAccess(
        config.StorageBookmark, 
        out var storagePath);
}
```

## Error Handling

All methods return `false` on failure and log internally. Always check return values:

```csharp
// Good
if (!MacOsSecurityScopedBookmark.TryCreateBookmark(path, out var bookmark))
{
    _logger.LogError("Failed to create bookmark");
    return;
}

// Bad - don't do this
var bookmark = out_variable; // Will be null, no error info
MacOsSecurityScopedBookmark.TryCreateBookmark(path, out bookmark);
```

## Important Notes

### ⚠️ Platform-Specific

This class is **macOS-only**. On other platforms:

- Call will throw `EntryPointNotFoundException`
- Guard with platform checks:

```csharp
if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
{
    MacOsSecurityScopedBookmark.TryResolveAndStartAccess(bookmark, out var path);
}
```

### ⚠️ Process Boundaries

Security-scoped access is **process-bound**:

```csharp
// ✅ This works - same process image
MacOsSecurityScopedBookmark.TryResolveAndStartAccess(bookmark, out var path);
// Later...
Process.Start("exec", $"/path/to/other-binary --arg");  // Inherits access

// ❌ This does NOT work - new process
MacOsSecurityScopedBookmark.TryResolveAndStartAccess(bookmark, out var path);
var child = Process.Start("separate-process");  // Does NOT inherit access
```

### ⚠️ Cleanup is Important

Always call `StopAccess()` when you're done:

```csharp
MacOsSecurityScopedBookmark.TryResolveAndStartAccess(bookmark, out var path);
try
{
    // Use path
    DoWork(path);
}
finally
{
    // Always clean up - balances startAccessingSecurityScopedResource()
    MacOsSecurityScopedBookmark.StopAccess(path);
}
```

## Testing

See `MacOsSecurityScopedBookmarkTests.cs` for 14 comprehensive test cases including:

- Valid paths
- Error cases (non-existent paths, invalid base64, corrupted data)
- Round-trip create→resolve cycles
- Best-effort cleanup scenarios
- Platform-specific guards

Run tests with:

```bash
dotnet test --filter "MacOsSecurityScopedBookmark"
```

## Troubleshooting

### "EntryPointNotFoundException"

- Running on non-macOS platform - guard with platform checks

### "Bookmark resolution fails silently"

- Check that bookmark hasn't expired (bookmarks can become invalid)
- Verify file/folder still exists
- Check file permissions haven't changed

### "Access still fails after resolution"

- Ensure you're using the `resolvedPath` output, not the original path
- Verify `StopAccess()` isn't being called too early

## Related Documentation

- [macOS BookmarkData - Apple Docs](https://developer.apple.com/documentation/foundation/nsurl/1417051-bookmarkdata)
- [Security-Scoped Resources - Apple Docs](https://developer.apple.com/documentation/foundation/nsurl/1408010-startaccessingsecurityscopedreso)

