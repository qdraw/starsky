# Feature: Create New Tenant

## Summary
Added frontend UI and backend API endpoint to allow global administrators to create new tenants directly from the "My Tenants" page.

## Changes Made

### Backend Changes

#### 1. TenantsController.cs - Added Create Endpoint
**File:** `/starsky/Controllers/TenantsController.cs`

**New Endpoint:** `POST /api/tenants/create`

**Features:**
- Accepts JSON request with `slug` and `name` fields
- Validates user is authenticated (has valid session cookie)
- Only allows global administrators to create tenants
- Validates tenant slug format using `ITenantSlugValidator`
- Checks if slug already exists (prevents duplicates)
- Automatically adds creator as Admin of the new tenant
- Returns success response with created tenant details

**Request Format:**
```json
{
  "slug": "my-photos",
  "name": "My Photos"
}
```

**Response Format:**
```json
{
  "success": true,
  "message": "Tenant created successfully",
  "tenant": {
    "slug": "my-photos",
    "name": "My Photos",
    "role": "Admin",
    "isEnabled": true
  }
}
```

**Validation Rules:**
- **Slug:** Lowercase alphanumeric with hyphens, 3-50 characters, must start/end with letter or number
- **Name:** 1-100 characters
- **Permission:** Global admin only

#### 2. Dependency Injection Updates
Added `ITenantSlugValidator` to TenantsController constructor for slug validation.

### Frontend Changes

#### 1. URL Query Helper
**File:** `/clientapp/src/shared/url/url-query.ts`

**New Method:**
```typescript
public UrlTenantCreateApi = (): string => {
  return `${this.prefix}/api/tenants/create`;
};
```

#### 2. My Tenants Page Component
**File:** `/clientapp/src/pages/my-tenants-page.tsx`

**New Features:**
- Added "Create New Tenant" button (visible only to global admins)
- Toggle form for creating new tenant with fields:
  - **Slug Input:** Pattern-validated, max 50 characters
  - **Name Input:** Max 100 characters
- Form validation before submission
- Loading state during creation
- Error handling and display
- Success message with auto-redirect
- Cancel button to close form

**UI Elements:**
- Toggle button to show/hide create form
- Input validation with helpful hints
- Error message display (red)
- Success confirmation (green)
- Submit and Cancel buttons
- Loading state feedback

### Data Types

#### CreateTenantRequest (C# DTO)
```csharp
public class CreateTenantRequest
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    [RegularExpression(@"^[a-z0-9][a-z0-9-]{1,48}[a-z0-9]$")]
    public required string Slug { get; set; }

    [Required]
    [StringLength(100)]
    public required string Name { get; set; }
}
```

#### ITenantMineResponse (TypeScript Interface)
```typescript
interface ITenantMineResponse {
  tenants?: ITenantMineResponseItem[];
  isGlobalAdmin?: boolean;
  isEmpty?: boolean;
  canCreateFirstTenant?: boolean;
}
```

## User Experience

### For Non-Global Admins
- No "Create New Tenant" button visible
- Can view and access existing tenants

### For Global Admins
1. Click "Create New Tenant" button on My Tenants page
2. Enter tenant slug (lowercase, hyphens allowed)
3. Enter tenant name
4. Click "Create Tenant"
5. On success:
   - Button shows "Creating..."
   - Success message displayed
   - Page auto-reloads after 1 second
   - New tenant appears in list
6. On error:
   - Error message displayed
   - User can correct and retry
   - Or click "Cancel" to dismiss form

## Validation & Security

✅ **Authentication:** Session-based cookie validation  
✅ **Authorization:** Global admin only  
✅ **Input Validation:**
  - Slug pattern validation (regex)
  - Slug uniqueness check
  - Length constraints
  - Required field checks  
✅ **Data Persistence:** Auto-adds creator as tenant Admin  
✅ **API Response:** JSON with proper status codes

## Error Handling

| Error | HTTP Status | Message |
|-------|------------|---------|
| Missing slug/name | 400 | "Slug and Name are required" |
| Invalid slug format | 400 | "Invalid tenant slug. Must be lowercase alphanumeric..." |
| Slug already exists | 400 | "Tenant slug already exists" |
| Not authenticated | 401 | "Missing session" |
| Not global admin | 403 | "Only global administrators can create tenants" |

## Testing

To test the feature:

1. **Create Tenant as Global Admin:**
   - Login as global admin user
   - Navigate to "My Tenants" page
   - Click "Create New Tenant"
   - Fill in slug (e.g., "test-photos") and name (e.g., "Test Photos")
   - Click "Create Tenant"
   - Verify success message and page reload
   - New tenant should appear in list

2. **Test Validation:**
   - Try slug with uppercase letters (should be rejected)
   - Try slug with special characters (should be rejected)
   - Try duplicate slug (should be rejected)
   - Try empty fields (should be rejected)

3. **Test for Non-Admin:**
   - Login as non-admin user
   - Navigate to "My Tenants" page
   - Verify no "Create New Tenant" button is visible

## Files Modified

1. `starsky/Controllers/TenantsController.cs` - Added Create endpoint
2. `starsky/clientapp/src/shared/url/url-query.ts` - Added URL helper
3. `starsky/clientapp/src/pages/my-tenants-page.tsx` - Added UI component

## Migration Notes

- No database migrations required
- Backward compatible with existing API
- No breaking changes to existing endpoints
- Works with existing tenant infrastructure


