# Test Failure Fix Report

## Issue
**Test:** `Login › account already logged in special return url`  
**File:** `src/containers/login.spec.tsx` (line 71-104)  
**Status:** ✅ FIXED

### Error Details
```
Expected: "http://localhost/account/logout?ReturnUrl=/test"
Received: "http://localhost/starsky/account/logout?ReturnUrl=/test"
```

### Root Cause
The `TenantPrefix()` method in `src/shared/url/url-query.ts` was incorrectly adding the `/starsky` prefix to URLs even when the current pathname did not include the prefix.

**Problematic Code:**
```typescript
private TenantPrefix(): string {
  const tenant = this.CurrentTenant();
  if (!tenant) {
    return this.prefix;  // ← BUG: Always returns "/starsky" 
  }
  return `${this.prefix}/${tenant}`;
}
```

### The Problem
When navigating to `/?ReturnUrl=/test` (without the `/starsky` prefix):
1. `UrlHomeIndexPage()` correctly detected that the pathname `/` does NOT include `/starsky`
2. It returned `/test` (without the prefix)
3. But then `UrlLogoutPage()` called `TenantPrefix()` which always added `/starsky` regardless of the current pathname
4. Result: `/starsky/account/logout?ReturnUrl=/test` ❌ (when it should be `/account/logout?ReturnUrl=/test`)

### Solution
Made `TenantPrefix()` follow the same pattern as other URL methods like `UrlHomePage()` and `UrlHomeIndexPage()`, which check if the current pathname includes the prefix before adding it:

**Fixed Code:**
```typescript
private TenantPrefix(): string {
  // Only use prefix if the current pathname includes it
  if (!document.location.pathname.includes(this.prefix)) {
    return "";
  }

  const tenant = this.CurrentTenant();
  if (!tenant) {
    return this.prefix;
  }

  return `${this.prefix}/${tenant}`;
}
```

### Test Results
**Before Fix:**
- 1 Failed: `Login › account already logged in special return url`
- 1726 Passed

**After Fix:**
- ✅ All 1727 tests passed
- ✅ All 231 test suites passed
- ✅ No regressions

### Test Scenarios Validated
1. ✅ **Non-prefixed path:** `/` → URLs use no prefix (`/account/logout`)
2. ✅ **Prefixed path:** `/starsky/` → URLs use prefix (`/starsky/account/logout`)
3. ✅ **Tenant path:** `/starsky/main/` → URLs include tenant (`/starsky/main/account/logout`)
4. ✅ **Return URLs:** Correctly handled in all cases

### Files Modified
- `src/shared/url/url-query.ts` - Updated `TenantPrefix()` method

---

## Summary
This was a conditional checkout bug where the prefix detection wasn't consistently applied across different URL generation methods. The fix ensures all URL generation methods use the same logic for deciding when to include the `/starsky` prefix based on the current pathname.


