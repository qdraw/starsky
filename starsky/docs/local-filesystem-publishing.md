# Local File System Publishing Abstraction

## Overview

This implementation creates an abstraction layer above `FtpService` to support both FTP and local
file system publishing based on the `AppSettingsPublishProfilesRemote` configuration.

## Architecture

### 1. Model Changes

**File**: `starsky.foundation.platform/Models/AppSettingsPublishProfilesRemote.cs`

- Added `LocalFileSystem` to `RemoteCredentialType` enum
- Added `LocalFileSystemCredential` class with:
    - `Path` property for destination directory
    - `SetWarning()` method for security display
- Added `LocalFileSystem` property to `RemoteCredentialWrapper`
- Added `GetLocalFileSystemById()` method to retrieve local filesystem credentials by profile ID
- Updated `DisplaySecurity()` to handle local filesystem credentials

### 2. New Services

**File**: `starsky.feature.webftppublish/Services/LocalFileSystemPublishService.cs`

- Implements `IPublishService` interface
- Copies files to local file system destination instead of FTP
- Key methods:
    - `IsValidZipOrFolder()` - validates input (delegates to helper)
    - `Run()` - orchestrates the copy operation
    - `CopyToLocalFileSystem()` - performs actual file copy with directory creation

**File**: `starsky.feature.webftppublish/Services/PublishServiceSelector.cs`

- Implements `IPublishServiceSelector` interface
- Composite pattern that delegates to appropriate service based on `RemoteCredentialType`
- Key methods:
    - `Run()` - delegates to FTP or LocalFileSystem service
    - `GetPrimaryRemoteTypeForProfile()` - determines which remote type to use from configuration

### 3. Interfaces

**File**: `starsky.feature.webftppublish/Interfaces/IPublishService.cs`

- Base interface for all publish services
- Methods: `IsValidZipOrFolder()`, `Run()`

**File**: `starsky.feature.webftppublish/Interfaces/IPublishServiceSelector.cs`

- Selector interface with optional `preferredType` parameter
- Allows overriding configuration-based type selection

### 4. Helper Classes (Code Reuse)

**File**: `starsky.feature.webftppublish/Helpers/IsValidZipOrFolderHelper.cs`

- Extracted shared validation logic from FtpService
- Validates zip files and folders with manifest
- Used by both FtpService and LocalFileSystemPublishService

**File**: `starsky.feature.webftppublish/Helpers/ExtractZipHelper.cs`

- Extracted shared zip extraction logic
- Handles both folders and zip files
- Creates temp directories for extracted content

### 5. Controller Updates

**File**: `starsky/Controllers/PublishRemoteController.cs`

- Updated to use `IPublishServiceSelector` instead of `IFtpService`
- `PublishFtpAsync()` now supports both "ftp" and "localfilesystem" remote types
- Added `ParseRemoteType()` helper method
- Returns appropriate errors for unsupported types

### 6. Dependency Injection

**File**: `starsky/Startup.cs`

- Added manual registration for `IPublishServiceSelector` → `PublishServiceSelector`
- `LocalFileSystemPublishService` registered via `[Service]` attribute
- `FtpService` maintains existing `IFtpService` registration

## Configuration Example

```json
{
  "app": {
    "publishProfilesRemote": {
      "profiles": {
        "my-profile": [
          {
            "type": "ftp",
            "ftp": {
              "webFtp": "ftp://user:pass@example.com/path"
            }
          },
          {
            "type": "localFileSystem",
            "localFileSystem": {
              "path": "/var/www/html/published"
            }
          }
        ]
      },
      "default": [
        {
          "type": "ftp",
          "ftp": {
            "webFtp": "ftp://user:pass@default.com/path"
          }
        }
      ]
    }
  }
}
```

## API Usage

### Publish to FTP

```
POST /api/publish-remote/remote?itemName=my-item&publishProfileName=my-profile&remoteType=ftp
```

### Publish to Local File System

```
POST /api/publish-remote/remote?itemName=my-item&publishProfileName=my-profile&remoteType=localfilesystem
```

## Testing

### Unit Tests Created

1. **LocalFileSystemPublishServiceTest.cs**
    - Validation tests
    - Copy operation tests
    - Error handling tests

2. **PublishServiceSelectorTest.cs**
    - Delegation logic tests
    - Configuration-based type selection
    - Override with preferred type

3. **AppSettingsPublishProfilesRemoteTest.cs**
    - Credential retrieval tests
    - Security display tests
    - LocalFileSystem credential tests

4. **IsValidZipOrFolderHelperTest.cs**
    - Validation logic tests

5. **ExtractZipHelperTest.cs**
    - Zip extraction tests

### Updated Tests

- **PublishRemoteControllerTests.cs**
    - Updated to use `FakeIPublishServiceSelector`
    - Added test for localfilesystem remote type

### Fake Mocks

- **FakeIPublishServiceSelector.cs** - Mock implementation for testing

## Benefits

1. **Code Reuse**: Shared validation and extraction logic in helpers
2. **Extensibility**: Easy to add new remote types (S3, Azure Blob, etc.)
3. **Type Safety**: Strong typing with `RemoteCredentialType` enum
4. **Backwards Compatibility**: Existing FTP functionality unchanged
5. **Configuration-Driven**: Remote type determined by configuration
6. **Testability**: All components fully unit tested

## Migration Guide

### For Existing FTP Users

No changes required. Existing FTP configurations continue to work.

### For New Local File System Users

1. Add local file system configuration to `publishProfilesRemote`
2. Set `type` to `"localFileSystem"`
3. Specify destination `path`
4. Call API with `remoteType=localfilesystem`

## Future Enhancements

- Add support for S3/Azure Blob Storage
- Add progress reporting for large file copies
- Add dry-run mode for preview
- Add file synchronization (delete removed files)

