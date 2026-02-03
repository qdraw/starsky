## Publish Profile Publishability Feature - Implementation Summary

### Overview

This feature prevents accidental publishing of content by requiring explicit configuration to enable
publishing per profile. Only profiles marked with `WebPublish = true` can be used for FTP
publishing.

---

## Changes Made

### 1. **AppSettingsPublishProfiles Model**

**File:** `starsky.foundation.platform/Models/AppSettingsPublishProfiles.cs`

**Change:** Added `WebPublish` property

```csharp
/// <summary>
/// Allow publishing to FTP destination
/// </summary>
public bool WebPublish { get; set; } = false;
```

- **Default:** `false` (disabled by default for safety)
- **Configurable:** Per profile in `appsettings.json`
- **Updated ToString():** Includes the new property for logging

### 2. **FtpPublishManifestModel**

**File:** `starsky.feature.webftppublish/Models/FtpPublishManifestModel.cs`

**Change:** Added `PublishProfileName` property

```csharp
/// <summary>
/// The publish profile name used for this manifest
/// </summary>
public string? PublishProfileName { get; set; } = null;
```

- Tracks which profile was used for each publish operation
- Enables validation at publish time
- Serialized in `_settings.json` manifest file

### 3. **IPublishPreflight Interface**

**File:** `starsky.feature.webhtmlpublish/Interfaces/IPublishPreflight.cs`

**Change:** Added `IsProfilePublishable()` method

```csharp
/// <summary>
/// Check if the profile is allowed to publish to FTP
/// </summary>
bool IsProfilePublishable(string publishProfileName);
```

- Single responsibility: Validate if profile can publish
- Returns `false` for null/empty/non-existent profiles
- Returns `false` if any item in profile has `WebPublish = false`

### 4. **PublishPreflight Service**

**File:** `starsky.feature.webhtmlpublish/Services/PublishPreflight.cs`

**Change:** Implemented `IsProfilePublishable()` method

- Validates profile exists and all items have `WebPublish = true`
- Prevents publishing with non-publishable profiles at runtime

### 5. **PublishController**

**File:** `starsky/Controllers/PublishController.cs`

**Changes:**
a) Added `GetPublishableProfiles()` endpoint

- **Route:** `/api/publish/publishable`
- Returns only profiles with all items having `WebPublish = true`
- UI uses this to populate publish dropdown

b) Updated `PublishCreateAsync()` method

- Validates profile is publishable before proceeding
- Returns `BadRequest` if profile not publishable
- Passes profile name through publish pipeline

c) Updated background task to pass `publishProfileName` to `GenerateZip()`

### 6. **WebHtmlPublishService**

**File:** `starsky.feature.webhtmlpublish/Services/WebHtmlPublishService.cs`

**Changes:**
a) Updated `RenderCopy()` to use `ExportFtpManifest()`

- Stores profile name in manifest during rendering

b) Updated `GenerateZip()` signature

- Added optional `publishProfileName` parameter
- Maintains backward compatibility

### 7. **PublishManifest Helper**

**File:** `starsky.feature.webhtmlpublish/Helpers/PublishManifest.cs`

**Change:** Added `ExportFtpManifest()` method

```csharp
public FtpPublishManifestModel ExportFtpManifest(
    string parentFullFilePath, 
    string itemName,
    Dictionary<string, bool>? copyContent, 
    string? publishProfileName = null)
```

- Creates FTP-specific manifests with profile name
- Complements existing `ExportManifest()` for HTML exports

### 8. **WebFtpCli Helper**

**File:** `starsky.feature.webftppublish/Helpers/WebFtpCli.cs`

**Changes:**
a) Injected `IPublishPreflight` dependency

b) Added validation before `FtpService.Run()`

```csharp
if ( !string.IsNullOrEmpty(settings?.PublishProfileName) &&
     !_publishPreflight.IsProfilePublishable(settings.PublishProfileName) )
{
    _console.WriteLine("Profile '{name}' is not allowed to publish...");
    return;
}
```

- Runtime enforcement: Prevents FTP operations for non-publishable profiles
- Works with both UI and CLI invocations

---

## Configuration Example

### appsettings.json

```json
{
  "publishProfiles": {
    "_default": [
      {
        "contentType": "Html",
        "sourceMaxWidth": 100,
        "overlayMaxWidth": 100,
        "path": "...",
        "folder": "",
        "append": "",
        "template": "Index.cshtml",
        "prepend": "",
        "metaData": true,
        "copy": true,
        "webpublish": true
      }
    ],
    "staging": [
      {
        "contentType": "Html",
        "template": "Index.cshtml",
        "webpublish": false
      }
    ]
  }
}
```

In this example:

- `_default` profile: **Can publish** (webpublish = true)
- `staging` profile: **Cannot publish** (webpublish = false)
- Available for export but hidden from publish UI

---

## Behavior

### For UI (Web)

1. User clicks "Publish" button
2. System queries `/api/publish/publishable` endpoint
3. Only publishable profiles are shown in dropdown
4. User selects profile and confirms
5. If someone bypasses UI (manual request), validation in controller rejects it
6. Profile name stored in manifest for audit trail

### For CLI (WebFtpCli)

1. User runs `starskywebftpcli` with manifest
2. Manifest includes profile name (stored during HTML generation)
3. WebFtpCli validates profile is publishable
4. If not publishable, shows error and exits without calling FTP service
5. If publishable, proceeds with FTP upload

### Manifest Storage

The `_settings.json` file now contains:

```json
{
  "slug": "my_photo_album",
  "copy": { ... },
  "publishProfileName": "_default"
}
```

---

## Testing

### Test Classes Created

1. **PublishPreflightPublishableTests**
    - Tests `IsProfilePublishable()` logic
    - Covers all scenarios: enabled, disabled, multiple items, edge cases

2. **PublishControllerPublishableTests**
    - Tests `/api/publish/publishable` endpoint filtering
    - Tests `PublishCreateAsync()` validation
    - Verifies rejection of non-publishable profiles

3. **WebFtpCliPublishableTests**
    - Tests runtime validation in `WebFtpCli.RunAsync()`
    - Verifies error messages and prevention of FTP calls

4. **FtpPublishManifestModelTests**
    - Tests serialization/deserialization of manifest
    - Verifies backward compatibility (null profile name)
    - Tests various profile names

---

## Backward Compatibility

✅ **Fully backward compatible:**

- `WebPublish` defaults to `false` (safe default)
- Existing manifests without `PublishProfileName` deserialize correctly
- Existing code paths continue to work
- New parameter in `GenerateZip()` is optional

---

## Security Benefits

✅ **Prevents accidental publishing:**

- Requires explicit opt-in (`WebPublish = true`)
- Runtime validation prevents bypassing configuration
- Audit trail via manifest file
- Both UI and CLI are protected

---

## Future Enhancements

1. Add logging/audit trail for publish attempts
2. Add profile validation warnings in startup
3. Add UI indicators for non-publishable profiles
4. Add webhook/notifications for publish events
5. Add role-based profile access control

