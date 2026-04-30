# Security Audit: Multi-Tenant Isolation Analysis

**Date:** April 30, 2026  
**Scope:** Starsky multi-tenant architecture - database, storage, and request handling  
**Conclusion:** **MULTIPLE SECURITY CONCERNS IDENTIFIED**

---

## Executive Summary

The Starsky application implements multi-tenant isolation through:
1. **Database Layer**: EF Core global query filters on `FileIndex` scoped to `TenantId`
2. **HTTP Request Layer**: Tenant extraction from URL path and JWT claims
3. **Storage Layer**: Tenant-scoped storage folders (`StorageFolder/{tenantSlug}/`)
4. **Request-scoped Context**: `ITenantContext` carries tenant identity through DI

**However, several security gaps exist that could allow tenant escape:**

---

## CRITICAL ISSUES

### 🔴 Issue 1: Path Traversal Vulnerability in File Access

**Severity: HIGH**  
**Location**: Controllers accept user-supplied file paths without validation against `..` sequences

**Details:**
- `IndexController.Index()` accepts `filePath` parameter directly from query string (e.g., `?f=/2020/photo.jpg`)
- Path is only normalized with `PathHelper.PrefixDbSlash()` and `PathHelper.RemoveLatestSlash()`
- **NO validation** to prevent `../` directory traversal patterns
- Example attack: `GET /main/api/index?f=/../../../second/2020/photo.jpg`

**Code Evidence:**
```csharp
// IndexController.cs, line 57-62
var subPath = PathHelper.PrefixDbSlash(f);
subPath = PathHelper.RemoveLatestSlash(subPath);
// ... NO validation here against ".." patterns
var singleItem = _query.SingleItem(subPath, ...);
```

**Attack Vector:**
While the **database** layer would filter by `TenantContext.TenantId` and prevent return of cross-tenant data, a careful attacker could:
1. Use path traversal to construct queries against the database
2. Potentially bypass intended query patterns
3. Discover file names/structure of other tenants through error messages or response times

**Database Layer Protection:**
✅ The global query filter on `FileIndex` **DOES** filter by tenant ID:
```csharp
// ApplicationDbContext.cs, line 95-96
modelBuilder.Entity<FileIndexItem>()
    .HasQueryFilter(f => GetCurrentTenantId() == null || f.TenantId == GetCurrentTenantId());
```

**BUT storage access could bypass this if...**

---

### 🔴 Issue 2: Thumbnail and Storage File Access Without Tenant Validation

**Severity: HIGH**  
**Location**: Any file serving endpoint (thumbnails, downloads, exports)

**Details:**
- Thumbnail endpoints likely accept file path parameters
- Storage layer `StorageSubPathFilesystem.ToFullPath()` requires valid `TenantContext`
- **IF TenantContext is null or not set**, files are accessed from the global storage folder (not tenant-isolated)
- **IF file path traversal is successful at storage layer**, could escape tenant folder

**Code Evidence:**
```csharp
// StorageSubPathFilesystem.cs, line 32-33
private string ToFullPath(string path) =>
    _appSettings.DatabasePathToFilePath(path, _tenantContext?.TenantSlug);

// AppSettings.cs, line 1134-1145
public string DatabasePathToFilePath(string databaseFilePath, string? tenantSlug)
{
    if (string.IsNullOrEmpty(tenantSlug))
    {
        return DatabasePathToFilePath(databaseFilePath);  // ⚠️ Falls back to non-tenant path!
    }
    var filepath = StorageFolder + tenantSlug + databaseFilePath;  // Concatenation without normalization
    filepath = PathToFileReplacePathStyle(filepath);
    return filepath;
}
```

**Attack Path:**
```
1. Call storage API with crafted path: /main/api/thumbnail?path=/../../../second/secret.jpg
2. If TenantContext is properly set: Safe (filtered to "main" tenant folder)
3. If TenantContext is null: Falls back to global StorageFolder/secret.jpg
4. If path has ".." sequences: Could escape tenant folder via OS path traversal
   Example: StorageFolder/main/../../second/photo.jpg resolves to StorageFolder/second/photo.jpg
```

**Path Normalization Missing:**
```csharp
// PathToFileReplacePathStyle only handles / vs \ separators, NOT ".." resolution
private static string PathToFileReplacePathStyle(string subPath)
{
    if (Path.DirectorySeparatorChar.ToString() == "\\")
    {
        subPath = subPath.Replace("/", "\\");
    }
    return subPath;  // ⚠️ ".." sequences not normalized away!
}
```

---

### 🔴 Issue 3: TenantContext Can Be Null in Scoped Requests

**Severity: MEDIUM**  
**Location**: `TenantContext.cs` + middleware initialization

**Details:**
- `TenantContext` is scoped and can be set to null
- Some endpoints allow global access (e.g., `/api/tenants/mine`)
- Global endpoints may accept file paths without tenant filtering
- Background jobs explicitly set tenant context but scope management could have race conditions

**Code Evidence:**
```csharp
// TenantContext.cs, line 31-48
public int? TenantId
{
    get
    {
        if (_tenantIdOverrideSet)
        {
            return _tenantIdOverride;  // Can be null!
        }
        // ... falls back to claims or null
    }
}

// ApplicationDbContext.cs, line 95-96
.HasQueryFilter(f => GetCurrentTenantId() == null || f.TenantId == GetCurrentTenantId());
//                    ↑↑↑↑↑↑↑↑↑↑↑↑ ⚠️ When null, ALL rows visible!
```

**Not Exploitable for DB but...**
This design (`null == Admin sees all`) is risky. It means:
- CLI tools and migrations see all tenant data (intentional but dangerous)
- Any middleware exception that fails to set tenant context would expose all data

---

### 🟡 Issue 4: WebSession/Cookie Validation Could Have Gaps

**Severity: MEDIUM**  
**Location**: `TenantSessionAuthenticationMiddleware.cs`

**Details:**
- Session cookie doesn't include tenant information
- Middleware validates session belongs to tenant via database query
- **BUT** if database query is bypassed by cache/race condition, wrong tenant could be used

**Code Evidence:**
```csharp
// TenantSessionAuthenticationMiddleware.cs, line 108-119
if (!await sessionStore.IsTenantActivatedAsync(session.Id, tenant.Id))
{
    // Deny access
    return;
}
```

**Race Condition Risk:**
If session caching doesn't properly invalidate when tenant membership changes, user could access old tenant until cache expires.

---

## MEDIUM SEVERITY ISSUES

### 🟡 Issue 5: Directory Traversal in Sync/Import Operations

**Severity: MEDIUM**  
**Location**: Import, sync, and file manipulation services

**Details:**
- Rename service, import service, etc. accept file paths from requests
- These might not validate against `..` sequences
- Could potentially move files outside tenant boundaries

**Code Evidence:**
```csharp
// RenameService.cs, line 481-484
var inputParentSubFolder = FilenamesHelper.GetParentPath(inputFileSubPath);
var toParentSubFolder = FilenamesHelper.GetParentPath(toFileSubPath);
// No validation that both are within same tenant!
```

---

## LOW SEVERITY ISSUES

### 🟢 Issue 6: IgnoreQueryFilters() Usage

**Severity: LOW**  
**Location**: Test code + possible utility functions

**Details:**
- `IgnoreQueryFilters()` is used in tests (correct usage)
- Should validate it's never used in production code without explicit tenant checking

**Code Evidence:**
```csharp
// ApplicationDbContextTenantIsolationTest.cs, line 86
.IgnoreQueryFilters()  // ✅ Only in test, proper usage
```

Status: ✅ No production usage found

---

## RISK MATRIX

| Attack Vector | Likelihood | Impact | Mitigation |
|---|---|---|---|
| Path traversal in file path parameters | MEDIUM | HIGH | Input validation |
| Storage layer path escape (storage OS access) | MEDIUM | HIGH | Path normalization |
| Access without proper TenantContext | LOW | CRITICAL | Middleware validation |
| Session/cookie tenant validation bypass | LOW | CRITICAL | Rate limiting + DB query |

---

## RECOMMENDED FIXES

### 1. **Add Path Traversal Validation** (HIGH PRIORITY)

```csharp
// Create utility method
public static bool IsValidSubPath(string path)
{
    if (string.IsNullOrEmpty(path))
        return false;
    
    // Reject if contains .. patterns
    if (path.Contains(".."))
        return false;
    
    // Normalize to OS path and verify it stays within base folder
    var normalizedPath = Path.GetFullPath(
        Path.Combine(baseStorageFolder, path));
    
    if (!normalizedPath.StartsWith(baseStorageFolder))
        return false;  // Path traversal detected
    
    return true;
}
```

### 2. **Normalize File Paths Before Use** (HIGH PRIORITY)

```csharp
// In AppSettings.DatabasePathToFilePath()
public string DatabasePathToFilePath(string databaseFilePath, string? tenantSlug)
{
    if (string.IsNullOrEmpty(tenantSlug))
        return DatabasePathToFilePath(databaseFilePath);
    
    // Construct path
    var filepath = StorageFolder + tenantSlug + databaseFilePath;
    filepath = PathToFileReplacePathStyle(filepath);
    
    // NEW: Normalize and validate
    filepath = Path.GetFullPath(filepath);
    var expectedBase = Path.GetFullPath(StorageFolder + tenantSlug);
    
    if (!filepath.StartsWith(expectedBase))
        throw new ArgumentException("Path traversal detected");
    
    return filepath;
}
```

### 3. **Validate Tenant Context in Critical Endpoints** (MEDIUM PRIORITY)

```csharp
// Add authorization attribute or middleware check
[Authorize]
[HttpGet("/api/index")]
public IActionResult Index(string f = "/")
{
    if (_tenantContext?.TenantId == null)
        return Forbid();  // Ensure tenant is set
    
    // ... rest of implementation
}
```

### 4. **Add Path Validation to All Endpoints Accepting Paths** (MEDIUM PRIORITY)

- `IndexController.Index()`
- `SearchController` endpoints
- `ThumbnailController` endpoints
- `ExportController` endpoints
- `ImportController` endpoints
- All file manipulation controllers

---

## PROOF OF CONCEPT (Do Not Execute in Production)

```
# PoC 1: Path Traversal DB Access Attempt
GET /main/api/index?f=/../../../second/2020/test.jpg

Expected: Should be blocked or return empty (database filter protects)
Risk: Database query could behave unexpectedly with ".." in WHERE clause

# PoC 2: Storage Layer Escape (if no path normalization)
GET /main/api/thumbnail?path=/../../second/photo.jpg
→ StorageFolder/main/../../second/photo.jpg
→ Resolves to: StorageFolder/second/photo.jpg (SHOULD BE BLOCKED)

# PoC 3: Session Theft → Tenant Escape
1. Authenticate as user@tenant-main.com (get session cookie)
2. Call /second/api/... endpoints with same session
3. Middleware checks if session is active for "second" tenant
4. If cache/race condition: Could access second tenant with main tenant's session
```

---

## VERIFICATION CHECKLIST

- [ ] All file path parameters validated against `..` patterns
- [ ] `Path.GetFullPath()` used to normalize paths
- [ ] Path escape attempts caught and blocked before storage access
- [ ] TenantContext required and validated in all file-serving endpoints
- [ ] No `IgnoreQueryFilters()` in production code
- [ ] Session validation always performs fresh DB query (no blind cache trust)
- [ ] Storage folder configured as read-only to prevent direct OS access
- [ ] Integration tests verify tenant isolation (file in tenant A cannot be read from tenant B)
- [ ] Penetration test performed on file access endpoints

---

## CONCLUSION

**The database layer provides good tenant isolation via EF Core filters**, but **storage/file access layers lack sufficient input validation** to prevent path traversal attacks. The application is **vulnerable to attempts to escape tenant boundaries** if:

1. File path parameters are not validated
2. Storage paths are not normalized with OS-level path validation
3. TenantContext is not properly validated in critical paths

**Implement the recommended fixes immediately**, especially path validation and normalization.


