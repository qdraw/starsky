# macOS Security-Scoped Bookmark Implementation

## Overview

A new macOS-specific feature has been added to handle security-scoped bookmark resolution and access
in the Starsky application. This feature is located in `starsky.foundation.native` and provides
P/Invoke wrappers for macOS Foundation framework APIs.

## File Structure

### Main Implementation

- **`starsky.foundation.native/FileSystem/MacOsSecurityScopedBookmark.cs`** - Core P/Invoke bindings
  and API

### Tests

- **`starskytest/starsky.foundation.native/FileSystem/MacOsSecurityScopedBookmarkTests.cs`** -
  Comprehensive unit tests

## Key Semantics

**CRITICAL:** macOS security-scoped access is **process-bound**, not transferable to child
processes:

- ✅ **STAYS ACCESSIBLE**: Parent process starts access → exec() into child binary → child inherits
  access (same process image)
- ❌ **LOST**: Parent process starts access → spawn new child process → child CANNOT access resource

This is why the raw bookmark base64 must be passed to the child, and the child must call the macOS
APIs to start access itself.

## Public API

### `TryResolveAndStartAccess(string bookmarkData, out string? resolvedPath): bool`

Resolves a security-scoped bookmark and starts file access. The resource remains accessible to the
current process and any child processes spawned via `exec()` (same process image).

**Parameters:**

- `bookmarkData` - Base64-encoded bookmark data
- `resolvedPath` - Output: resolved file path, or null on failure

**Returns:** `true` if successful; `false` otherwise

**Example:**

```csharp
if (MacOsSecurityScopedBookmark.TryResolveAndStartAccess(base64Bookmark, out var path))
{
    Console.WriteLine($"Access granted to: {path}");
    // Resource is now accessible - can spawn child processes
    // Remember to call StopAccess when done
    MacOsSecurityScopedBookmark.StopAccess(path);
}
```

### `StopAccess(string path): void`

Stops accessing a security-scoped resource. This balances `startAccessingSecurityScopedResource()`
and should be called when the resource is no longer needed.

**Note:** This method is best-effort and does not throw exceptions.

**Parameters:**

- `path` - The resolved path from `TryResolveAndStartAccess()`

### `TryCreateBookmark(string filePath, out string? bookmarkBase64): bool`

Creates a security-scoped bookmark from a file path. Returns base64-encoded bookmark data suitable
for later resolution.

**Parameters:**

- `filePath` - Absolute path to file or directory
- `bookmarkBase64` - Output: base64-encoded bookmark data, or null on failure

**Returns:** `true` if successful; `false` otherwise

**Example:**

```csharp
if (MacOsSecurityScopedBookmark.TryCreateBookmark("/path/to/file", out var bookmark))
{
    // bookmark is base64-encoded and can be stored/transmitted
    Console.WriteLine($"Created bookmark: {bookmark}");
}
```

## Implementation Details

### P/Invoke Layer

The class uses P/Invoke to call Foundation framework APIs:

- **`NSURL(fileURLWithPath:)`** - Create file URLs
- **`NSURL(bookmarkDataWithOptions:...)`** - Create security-scoped bookmarks
- **`NSURL(URLByResolvingBookmarkData:...)`** - Resolve bookmarks back to URLs
- **`NSURL(startAccessingSecurityScopedResource)`** - Start file access
- **`NSURL(stopAccessingSecurityScopedResource)`** - Stop file access
- **`NSData` APIs** - Convert bookmark data to/from byte arrays
- **CFString APIs** - String marshalling with UTF-16 and UTF-8 encoding

### Memory Management

- **Retain cycles:** `startAccessingSecurityScopedResource()` establishes an intentional retain
  cycle on the NSURL. Do NOT release the NSURL after calling it.
- **Cleanup:** The retain cycle is broken when `stopAccessingSecurityScopedResource()` is called.
  Always pair start/stop calls.
- **Error handling:** All public methods return `bool` or are best-effort (`StopAccess`). Exceptions
  are caught internally.

## Usage Pattern

```csharp
// Step 1: Create bookmark in parent/UI context
if (!MacOsSecurityScopedBookmark.TryCreateBookmark(userSelectedPath, out var bookmarkBase64))
{
    // Handle error
}

// Step 2: Pass bookmarkBase64 to child process via command-line args or file
var childProcess = Process.Start(new ProcessStartInfo 
{ 
    FileName = "my-backend",
    Arguments = $"--bookmark {bookmarkBase64}"
});

// Step 3: Child process resolves and accesses the resource
if (MacOsSecurityScopedBookmark.TryResolveAndStartAccess(passedBookmarkBase64, out var path))
{
    // Now the child has access to the file/directory
    File.WriteAllText(path + "/somefile.txt", "data");
    
    // Clean up when done
    MacOsSecurityScopedBookmark.StopAccess(path);
}
```

## Test Coverage

The implementation includes 14 comprehensive unit tests:

### Creation Tests

- `TryCreateBookmark_ValidPath_ReturnsValidBase64` - Valid file path
- `TryCreateBookmark_NonExistentPath_ReturnsFalse` - Error case
- `TryCreateBookmark_EmptyPath_ReturnsFalse` - Error case

### Resolution Tests

- `TryResolveAndStartAccess_ValidBookmark_ResolvesPath` - Round-trip test
- `TryResolveAndStartAccess_InvalidBase64_ReturnsFalse` - Error case
- `TryResolveAndStartAccess_EmptyBookmarkData_ReturnsFalse` - Error case
- `TryResolveAndStartAccess_CorruptedBookmarkData_ReturnsFalse` - Error case

### Cleanup Tests

- `StopAccess_ValidPath_CompletesWithoutException` - Normal cleanup
- `StopAccess_InvalidPath_CompletesWithoutException` - Best-effort cleanup
- `StopAccess_EmptyPath_CompletesWithoutException` - Best-effort cleanup

### Integration Tests

- `RoundTrip_CreateAndResolveBookmark_SuccessfullyResolvesPath` - Create → Resolve cycle

### Platform Tests

- `TryCreateBookmark_OnNonMacOS_Throws` - macOS-only guard
- `TryResolveAndStartAccess_OnNonMacOS_Throws` - macOS-only guard

All tests are decorated with `[OSCondition]` attributes to run only on appropriate platforms.

## Error Handling

All methods use **best-effort error handling**:

- Public methods catch all exceptions and return `false` or `null`
- `StopAccess()` is completely best-effort and never throws
- Invalid base64, corrupted data, and missing files are handled gracefully

## Platform Support

- ✅ **macOS** (All versions with Foundation framework)
- ❌ **Windows** - P/Invoke calls will fail with `EntryPointNotFoundException`
- ❌ **Linux** - P/Invoke calls will fail with `EntryPointNotFoundException`

## Related Issues

This implementation resolves the challenge of passing file access permissions to child processes on
macOS by:

1. Creating security-scoped bookmarks in the parent process
2. Serializing them as base64 for transmission
3. Having the child process resolve and activate them independently
4. Maintaining the security sandbox while enabling cross-process file access

This is less intrusive than other approaches (like keeping the parent process alive or using
launchd), as it leverages the OS's built-in security-scoped bookmark mechanism.

