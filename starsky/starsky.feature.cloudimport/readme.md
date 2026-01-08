# starsky.feature.cloudimport

Cloud Import functionality for automatic import from cloud storage providers (Dropbox, Google Drive,
OneDrive, etc.)

## Features

- **Multiple Providers**: Support for multiple cloud storage providers simultaneously
- **Scheduled Sync**: Automatically download files from cloud storage at configurable intervals
- **Manual Trigger**: API endpoint to manually trigger sync operations for all or specific providers
- **Import Integration**: Reuses existing import pipeline for consistent processing
- **Idempotency**: Prevents re-importing already processed files
- **Concurrent Protection**: Prevents overlapping sync executions per provider
- **Post-Import Cleanup**: Optional deletion of files from cloud storage after successful import
- **Comprehensive Logging**: Detailed logs of sync operations, successes, and failures
- **Provider Abstraction**: Interface-based design supports multiple cloud storage providers

## Configuration

Add the following to your `appsettings.json`:

```json
{
  "CloudImport": {
    "Providers": [
      {
        "Id": "dropbox-camera",
        "Enabled": true,
        "Provider": "Dropbox",
        "RemoteFolder": "/Camera Uploads",
        "SyncFrequencyMinutes": 0,
        "SyncFrequencyHours": 1,
        "DeleteAfterImport": false,
        "Credentials": {
          "AccessToken": "YOUR_DROPBOX_ACCESS_TOKEN",
          "RefreshToken": "",
          "AppKey": "",
          "AppSecret": "",
          "ExpiresAt": null
        }
      },
      {
        "Id": "dropbox-screenshots",
        "Enabled": true,
        "Provider": "Dropbox",
        "RemoteFolder": "/Screenshots",
        "SyncFrequencyMinutes": 30,
        "SyncFrequencyHours": 0,
        "DeleteAfterImport": true,
        "Credentials": {
          "AccessToken": "YOUR_DROPBOX_ACCESS_TOKEN",
          "RefreshToken": "",
          "AppKey": "",
          "AppSecret": "",
          "ExpiresAt": null
        }
      }
    ]
  }
}
```

### Configuration Options

Each provider in the `Providers` array supports:

- **Id**: Unique identifier for this provider configuration (required)
- **Enabled**: Enable or disable this Cloud Import provider (default: `false`)
- **Provider**: Cloud storage provider name (e.g., "Dropbox", "GoogleDrive", "OneDrive")
- **RemoteFolder**: Remote folder path to sync from (default: "/")
- **SyncFrequencyMinutes**: Sync frequency in minutes (takes priority if > 0)
- **SyncFrequencyHours**: Sync frequency in hours (used if SyncFrequencyMinutes is 0, default: 24)
- **DeleteAfterImport**: Delete files from cloud storage after successful import (default: `false`)
- **Credentials**: Provider-specific credentials (stored securely)

## API Endpoints

### Get Status

```
GET /api/CloudImport/status
```

Returns current Cloud Import configuration and status.

### Trigger Manual Sync

```
POST /api/CloudImport/sync
```

Manually trigger a sync operation.

### Get Last Result

```
GET /api/CloudImport/last-result
```

Get the result of the last sync operation.

## Supported Providers

### Dropbox

#### Setup

1. Create a Dropbox App at https://www.dropbox.com/developers/apps
2. Generate an access token
3. Add the access token to configuration

```json
{
  "CloudImport": {
    "Providers": [
      {
        "Id": "my-dropbox-sync",
        "Enabled": true,
        "Provider": "Dropbox",
        "RemoteFolder": "/Camera Uploads",
        "SyncFrequencyHours": 1,
        "Credentials": {
          "AccessToken": "YOUR_DROPBOX_ACCESS_TOKEN"
        }
      }
    ]
  }
}
```

#### Multiple Dropbox Folders

You can sync multiple Dropbox folders by adding multiple provider configurations:

```json
{
  "CloudImport": {
    "Providers": [
      {
        "Id": "dropbox-camera",
        "Enabled": true,
        "Provider": "Dropbox",
        "RemoteFolder": "/Camera Uploads",
        "SyncFrequencyHours": 1,
        "Credentials": {
          "AccessToken": "YOUR_DROPBOX_ACCESS_TOKEN"
        }
      },
      {
        "Id": "dropbox-documents",
        "Enabled": true,
        "Provider": "Dropbox",
        "RemoteFolder": "/Documents/Photos",
        "SyncFrequencyHours": 6,
        "Credentials": {
          "AccessToken": "YOUR_DROPBOX_ACCESS_TOKEN"
        }
      }
    ]
  }
}
```

### Adding New Providers

Implement the `ICloudImportClient` interface:

```csharp
[Service(typeof(ICloudImportClient), InjectionLifetime = InjectionLifetime.Scoped)]
public class MyCloudImportClient : ICloudImportClient
{
    public string Name => "MyProvider";
    public bool Enabled { get; }
    
    public Task<IEnumerable<CloudFile>> ListFilesAsync(string remoteFolder) { }
    public Task<string> DownloadFileAsync(CloudFile file, string localFolder) { }
    public Task<bool> DeleteFileAsync(CloudFile file) { }
    public Task<bool> TestConnectionAsync() { }
}
```

## Architecture

### Components

- **CloudImportService**: Main service orchestrating sync operations
- **CloudImportScheduledService**: Background service for scheduled syncs
- **ICloudImportClient**: Interface for cloud provider implementations
- **CloudImportController**: API controller for manual operations

### Flow

1. **Scheduled Service** triggers sync at configured intervals
2. **CloudImportService** acquires lock to prevent concurrent execution
3. **List Files** from cloud storage using provider client
4. **Download** each file to temporary folder
5. **Import** using existing import pipeline
6. **Cleanup** delete from cloud if DeleteAfterImport is enabled
7. **Log Results** and update LastSyncResult

## Testing

Comprehensive unit tests are available in:

- `starskytest/starsky.feature.cloudimport/Services/CloudImportServiceTest.cs`
- `starskytest/starsky.feature.cloudimport/Controllers/CloudImportControllerTest.cs`
- `starskytest/starsky.feature.cloudimport/Services/CloudImportScheduledServiceTest.cs`

Run tests:

```bash
dotnet test starskytest/starskytest.csproj --filter FullyQualifiedName~CloudImport
```

## Security

- Credentials are stored in configuration and should be protected
- Use environment variables for production: `app__CloudImport__Credentials__AccessToken`
- Access tokens should be kept secure and rotated regularly
- API endpoints require authentication via `[Authorize]` attribute

## Error Handling

The service handles various error scenarios:

- Connection failures
- Authentication errors
- Download failures
- Import failures
- Delete failures

All errors are logged and included in sync results without blocking other files.

## Performance Considerations

- Downloads happen sequentially to avoid overwhelming the network
- Temporary files are cleaned up after processing
- Idempotency check prevents re-processing files within 24 hours
- Concurrent execution is prevented via semaphore

## Future Enhancements

- Support for Google Drive
- Support for OneDrive
- Refresh token handling
- Selective sync based on file patterns
- Progress reporting
- Webhook support for real-time sync

