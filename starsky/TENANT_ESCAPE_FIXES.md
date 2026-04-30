# Tenant Escape: Proposed Security Fixes

## Fix 1: Add Path Validation Helper

Create file: `/starsky.foundation.platform/Helpers/PathSecurityValidator.cs`

```csharp
using System;
using System.IO;

namespace starsky.foundation.platform.Helpers;

/// <summary>
/// Validates file paths to prevent directory traversal attacks
/// </summary>
public static class PathSecurityValidator
{
    /// <summary>
    /// Validates that a database-style path doesn't attempt directory traversal
    /// </summary>
    /// <param name="databasePath">Path in format /folder/subfolder/file.jpg</param>
    /// <returns>True if path is safe, false if it contains traversal sequences</returns>
    public static bool IsValidDatabasePath(string? databasePath)
    {
        if (string.IsNullOrEmpty(databasePath))
            return false;

        // Reject paths with ".." sequences (directory traversal)
        if (databasePath.Contains(".."))
            return false;

        // Reject absolute paths or drive letters (Windows)
        if (Path.IsPathRooted(databasePath))
            return false;

        // Reject null characters
        if (databasePath.Contains('\0'))
            return false;

        return true;
    }

    /// <summary>
    /// Validates that a full file system path stays within an expected root directory
    /// </summary>
    /// <param name="fullPath">Full file system path to validate</param>
    /// <param name="allowedRootPath">Expected root directory</param>
    /// <returns>True if path is safe within root, false if traversal detected</returns>
    public static bool IsPathWithinRoot(string fullPath, string allowedRootPath)
    {
        try
        {
            // Normalize both paths to absolute paths
            var normalizedFullPath = Path.GetFullPath(fullPath);
            var normalizedRoot = Path.GetFullPath(allowedRootPath);

            // Ensure root ends with separator for proper boundary checking
            if (!normalizedRoot.EndsWith(Path.DirectorySeparatorChar))
                normalizedRoot += Path.DirectorySeparatorChar;

            // Check if normalized path starts with root
            return normalizedFullPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase);
        }
        catch (ArgumentException)
        {
            // Invalid path characters
            return false;
        }
    }

    /// <summary>
    /// Validates and sanitizes file extension
    /// </summary>
    /// <param name="filename">Filename to validate</param>
    /// <param name="allowedExtensions">List of allowed extensions (e.g., ".jpg", ".png")</param>
    /// <returns>True if extension is allowed</returns>
    public static bool IsAllowedExtension(string filename, params string[] allowedExtensions)
    {
        if (string.IsNullOrEmpty(filename))
            return false;

        var extension = Path.GetExtension(filename).ToLowerInvariant();
        return Array.Exists(allowedExtensions, ext => ext.Equals(extension, StringComparison.OrdinalIgnoreCase));
    }
}
```

---

## Fix 2: Update IndexController Path Validation

File: `/starsky/Controllers/IndexController.cs`

```csharp
// Add at the beginning of Index() method (after line 49):

[HttpGet("/api/index")]
public IActionResult Index(
    string f = "/",
    string? colorClass = null,
    bool collections = true,
    bool hideDelete = true,
    SortType sort = SortType.FileName)
{
    if (!ModelState.IsValid)
    {
        return BadRequest("Model invalid");
    }

    // NEW: Validate path to prevent directory traversal
    if (!PathSecurityValidator.IsValidDatabasePath(f))
    {
        return BadRequest("Invalid file path - path traversal not allowed");
    }

    // Original code continues below
    var colorClassActiveList = FileIndexItem.GetColorClassList(colorClass);
    var subPath = PathHelper.PrefixDbSlash(f);
    // ... rest of method
}
```

---

## Fix 3: Update StorageSubPathFilesystem Path Validation

File: `/starsky.foundation.storage/Storage/StorageSubPathFilesystem.cs`

```csharp
// Update the ToFullPath method:
private string ToFullPath(string path)
{
    // Validate input path first
    if (!PathSecurityValidator.IsValidDatabasePath(path))
    {
        throw new ArgumentException("Invalid path - potential directory traversal detected", nameof(path));
    }

    var fullPath = _appSettings.DatabasePathToFilePath(path, _tenantContext?.TenantSlug);
    
    // NEW: Validate that result stays within tenant folder
    var tenantRootFolder = _appSettings.DatabasePathToFilePath("/", _tenantContext?.TenantSlug);
    
    if (!PathSecurityValidator.IsPathWithinRoot(fullPath, tenantRootFolder))
    {
        throw new UnauthorizedAccessException(
            $"Path traversal detected: attempted access outside tenant folder. Path: {path}");
    }

    return fullPath;
}
```

---

## Fix 4: Update AppSettings.DatabasePathToFilePath

File: `/starsky.foundation.platform/Models/AppSettings.cs`

```csharp
/// <summary>
///     from relative database path => file location path, scoped to a specific tenant.
///     Storage layout convention: <c>StorageFolder/{tenantSlug}{databaseFilePath}</c>
///     Includes path traversal validation.
/// </summary>
public string DatabasePathToFilePath(string databaseFilePath, string? tenantSlug)
{
    // Validate input to prevent directory traversal
    if (!PathSecurityValidator.IsValidDatabasePath(databaseFilePath))
    {
        throw new ArgumentException(
            "Invalid database file path - contains traversal sequences or invalid characters",
            nameof(databaseFilePath));
    }

    if (string.IsNullOrEmpty(tenantSlug))
    {
        return DatabasePathToFilePath(databaseFilePath);
    }

    // Construct the path
    var filepath = StorageFolder + tenantSlug + databaseFilePath;
    filepath = PathToFileReplacePathStyle(filepath);

    // NEW: Normalize and validate the path stays within tenant folder
    try
    {
        var normalizedPath = Path.GetFullPath(filepath);
        var tenantRoot = Path.GetFullPath(StorageFolder + tenantSlug);

        // Ensure root ends with separator for proper boundary checking
        if (!tenantRoot.EndsWith(Path.DirectorySeparatorChar.ToString()))
            tenantRoot += Path.DirectorySeparatorChar;

        if (!normalizedPath.StartsWith(tenantRoot, System.StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                $"Path traversal detected: normalized path escapes tenant folder. Original: {databaseFilePath}");
        }

        return normalizedPath;
    }
    catch (ArgumentException ex) when (ex.Message.Contains("traversal"))
    {
        throw;  // Re-throw security exceptions
    }
    catch (ArgumentException ex)
    {
        throw new ArgumentException(
            $"Invalid path characters in database file path: {databaseFilePath}",
            nameof(databaseFilePath), ex);
    }
}
```

---

## Fix 5: Add Integration Tests

Create file: `/starskytest/Controllers/PathTraversalSecurityTest.cs`

```csharp
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.Controllers;

[TestClass]
[SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
public sealed class PathTraversalSecurityTest
{
    [TestMethod]
    [DataRow("/../../../../../etc/passwd")]
    [DataRow("/../../../second")]
    [DataRow("/folder/..")]
    [DataRow("/folder/../../../etc")]
    [DataRow("/../../../secret/file.jpg")]
    public void IndexController_RejectPathTraversalAttempts(string maliciousPath)
    {
        var fakeQuery = new FakeIQuery();
        var appSettings = new AppSettings();
        var controller = new IndexController(fakeQuery, appSettings)
        {
            ControllerContext = { HttpContext = new DefaultHttpContext() }
        };

        // Act - should return BadRequest for path traversal attempts
        var result = controller.Index(f: maliciousPath) as BadRequestObjectResult;

        // Assert
        Assert.IsNotNull(result, $"Path traversal attempt not rejected: {maliciousPath}");
        Assert.AreEqual(400, result.StatusCode);
    }

    [TestMethod]
    [DataRow("/2020/12/photo.jpg", true)]
    [DataRow("/nested/folder/file.jpg", true)]
    [DataRow("/", true)]
    public void IndexController_AcceptValidPaths(string validPath, bool shouldAccept)
    {
        var fakeQuery = new FakeIQuery();
        var appSettings = new AppSettings();
        var controller = new IndexController(fakeQuery, appSettings)
        {
            ControllerContext = { HttpContext = new DefaultHttpContext() }
        };

        // Act
        var result = controller.Index(f: validPath);

        // Assert - should NOT return BadRequest for valid paths
        var badRequest = result as BadRequestObjectResult;
        Assert.IsTrue(badRequest == null || shouldAccept == false,
            $"Valid path rejected: {validPath}");
    }

    [TestMethod]
    public void PathSecurityValidator_DetectsTraversalSequences()
    {
        // Arrange
        var testCases = new[]
        {
            ("/../../../etc", false),
            ("/2020/../../../secret", false),
            ("/normal/path", true),
            ("/2020/12/photo.jpg", true),
            ("", false),  // Empty is invalid
            (null, false),  // Null is invalid
            ("/file..jpg", true),  // ".." in filename is OK, just not as traversal
            ("/..", false),  // Explicit traversal
        };

        foreach (var (path, shouldBeValid) in testCases)
        {
            // Act & Assert
            var result = PathSecurityValidator.IsValidDatabasePath(path);
            Assert.AreEqual(shouldBeValid, result,
                $"Path validation failed for: {path ?? "null"}");
        }
    }

    [TestMethod]
    public void PathSecurityValidator_EnsuresPathsStayWithinRoot()
    {
        // Arrange
        var rootPath = "/var/storage/main";
        var testCases = new[]
        {
            ("/var/storage/main/2020/photo.jpg", true),
            ("/var/storage/main/nested/path.jpg", true),
            ("/var/storage/second/secret.jpg", false),  // Different tenant
            ("/var/secret.jpg", false),  // Parent directory
            ("/etc/passwd", false),  // System file
        };

        foreach (var (path, shouldBeWithinRoot) in testCases)
        {
            // Act
            var result = PathSecurityValidator.IsPathWithinRoot(path, rootPath);

            // Assert
            Assert.AreEqual(shouldBeWithinRoot, result,
                $"Root path validation failed for: {path}");
        }
    }
}
```

---

## Implementation Priority

1. **Phase 1 (Immediate):**
   - Add `PathSecurityValidator` class
   - Add validation to `IndexController.Index()`
   - Add integration tests

2. **Phase 2 (This Sprint):**
   - Update all controllers that accept file path parameters
   - Update `AppSettings.DatabasePathToFilePath()`
   - Update storage classes

3. **Phase 3 (Next Sprint):**
   - Audit all file-serving endpoints
   - Add security logging
   - Performance testing with normalization

---

## Validation Checklist

- [ ] `PathSecurityValidator` added and tested
- [ ] All controllers reject `..` sequences
- [ ] Integration tests pass
- [ ] Code review completed
- [ ] No regressions in file functionality
- [ ] Documentation updated
- [ ] Security advisory issued to users (if needed)


