# Cloud Sync Multiple Provider Support - Implementation Summary

## Overview

The cloud sync feature has been refactored to support multiple cloud storage providers simultaneously. Users can now configure multiple providers (e.g., multiple Dropbox folders, different cloud services) each with their own sync schedules and settings.

## Changes Made

### 1. Settings Model (`CloudSyncSettings.cs`)

**Before:**
```csharp
public class CloudSyncSettings
{
    public bool Enabled { get; set; }
    public string Provider { get; set; } = "Dropbox";
    public string RemoteFolder { get; set; } = "/";
    // ... other properties
}
```

**After:**
```csharp
public class CloudSyncSettings
{
    public List<CloudSyncProviderSettings> Providers { get; set; } = new();
}

public class CloudSyncProviderSettings
{
    public string Id { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public string Provider { get; set; } = "Dropbox";
    public string RemoteFolder { get; set; } = "/";
    // ... other properties
}
```

### 2. Cloud Sync Result (`CloudSyncResult.cs`)

Added `ProviderId` and `ProviderName` fields to track which provider each sync result belongs to:

```csharp
public class CloudSyncResult
{
    public string ProviderId { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    // ... existing properties
}
```

### 3. Service Interface (`ICloudSyncService.cs`)

**Before:**
```csharp
Task<CloudSyncResult> SyncAsync(CloudSyncTriggerType triggerType);
CloudSyncResult? LastSyncResult { get; }
```

**After:**
```csharp
Task<List<CloudSyncResult>> SyncAllAsync(CloudSyncTriggerType triggerType);
Task<CloudSyncResult> SyncAsync(string providerId, CloudSyncTriggerType triggerType);
Dictionary<string, CloudSyncResult> LastSyncResults { get; }
```

### 4. Cloud Sync Service (`CloudSyncService.cs`)

- Changed from single sync lock to per-provider locks using `ConcurrentDictionary<string, SemaphoreSlim>`
- Implemented `SyncAllAsync()` to sync all enabled providers
- Modified `SyncAsync()` to accept a `providerId` parameter
- Added provider-specific credential initialization for DropboxCloudSyncClient
- Stores last sync results per provider in a dictionary

### 5. Scheduled Service (`CloudSyncScheduledService.cs`)

- Creates independent background tasks for each enabled provider
- Each provider runs on its own schedule
- Tasks run concurrently without blocking each other

### 6. Controller (`CloudSyncController.cs`)

Added new endpoints:

- `GET /api/cloudsync/status` - Returns all providers' status
- `GET /api/cloudsync/status/{providerId}` - Returns specific provider status
- `POST /api/cloudsync/sync` - Triggers sync for all enabled providers
- `POST /api/cloudsync/sync/{providerId}` - Triggers sync for specific provider
- `GET /api/cloudsync/last-results` - Returns all providers' last sync results
- `GET /api/cloudsync/last-result/{providerId}` - Returns specific provider's last result

### 7. Dropbox Client (`DropboxCloudSyncClient.cs`)

- Modified `Enabled` property to check if ANY Dropbox provider exists in configuration
- Added `InitializeClient(string accessToken)` method to dynamically set credentials
- Client no longer depends on a single provider's settings

## Configuration Example

### appsettings.json

```json
{
  "CloudSync": {
    "Providers": [
      {
        "Id": "dropbox-camera",
        "Enabled": true,
        "Provider": "Dropbox",
        "RemoteFolder": "/Camera Uploads",
        "SyncFrequencyHours": 1,
        "DeleteAfterImport": false,
        "Credentials": {
          "AccessToken": "YOUR_TOKEN"
        }
      },
      {
        "Id": "dropbox-screenshots",
        "Enabled": true,
        "Provider": "Dropbox",
        "RemoteFolder": "/Screenshots",
        "SyncFrequencyMinutes": 30,
        "DeleteAfterImport": true,
        "Credentials": {
          "AccessToken": "YOUR_TOKEN"
        }
      }
    ]
  }
}
```

## Benefits

1. **Multiple Sources**: Sync from multiple cloud folders or different cloud services
2. **Independent Schedules**: Each provider can have its own sync frequency
3. **Isolated Execution**: Providers sync independently without blocking each other
4. **Granular Control**: Enable/disable individual providers without affecting others
5. **Better Tracking**: Track sync results separately for each provider
6. **Flexibility**: Add multiple configurations for the same cloud service (e.g., different Dropbox folders)

## Breaking Changes

### Configuration Migration

**Old Configuration:**
```json
{
  "CloudSync": {
    "Enabled": true,
    "Provider": "Dropbox",
    "RemoteFolder": "/Camera Uploads"
  }
}
```

**New Configuration:**
```json
{
  "CloudSync": {
    "Providers": [
      {
        "Id": "my-sync",
        "Enabled": true,
        "Provider": "Dropbox",
        "RemoteFolder": "/Camera Uploads"
      }
    ]
  }
}
```

### API Changes

- `POST /api/cloudsync/sync` now syncs ALL providers and returns `List<CloudSyncResult>`
- Use `POST /api/cloudsync/sync/{providerId}` to sync a specific provider
- `GET /api/cloudsync/last-result` changed to `GET /api/cloudsync/last-results` (plural)

## Files Modified

1. `/starsky.foundation.platform/Models/CloudSyncSettings.cs`
2. `/starsky.feature.cloudsync/CloudSyncResult.cs`
3. `/starsky.feature.cloudsync/Interfaces/ICloudSyncService.cs`
4. `/starsky.feature.cloudsync/Services/CloudSyncService.cs`
5. `/starsky.feature.cloudsync/Services/CloudSyncScheduledService.cs`
6. `/starsky.feature.cloudsync/Controllers/CloudSyncController.cs`
7. `/starsky.feature.cloudsync/Clients/DropboxCloudSyncClient.cs`
8. `/starsky.feature.cloudsync/readme.md`

## Files Created

1. `/starsky.feature.cloudsync/appsettings.example.json` - Example configuration

## Testing

To test the changes:

1. Update your `appsettings.json` with the new configuration format
2. Add multiple provider configurations
3. Start the application
4. Check logs to see scheduled sync tasks starting for each provider
5. Use the API endpoints to manually trigger syncs

## Future Enhancements

- Support for Google Drive
- Support for OneDrive
- Support for Amazon S3
- OAuth refresh token handling
- Webhook support for real-time sync
- File pattern filtering per provider
- Bandwidth throttling per provider

