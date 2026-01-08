# Cloud Import

The **Cloud Import** feature enables automated ingestion of files from cloud storage providers into
the existing import pipeline. It supports scheduled synchronization, manual triggering, and robust
error handling while reusing existing infrastructure and validation logic.

---

## Overview

Cloud Import periodically connects to one or more configured cloud storage providers, detects new
files in a remote folder, and imports them using the existing import pipeline. The system is
designed to be **secure**, **idempotent**, and **fault-tolerant**, ensuring that imports are
reliable and repeatable without duplication.

---

## Key Features

### Configuration

Each cloud import provider can be configured independently:

* Enable or disable Cloud Import per provider
* Select a cloud storage provider (e.g. Dropbox)
* Configure a remote folder path
* Set sync frequency (minutes or hours)
* Optional deletion of files after successful import
* Secure handling of provider credentials
* Support for configuration via JSON or environment variables

---

### Scheduling

* Automatic synchronization at configured intervals
* Background execution using a hosted service
* Protection against overlapping executions
* Manual synchronization trigger via authenticated API endpoint

---

### Sync & Import

* Establishes a connection to the configured cloud provider
* Lists available files in the remote folder
* Downloads **only new files** (idempotent behavior)
* Reuses the existing import pipeline
* Applies identical validation and error handling as manual imports

---

### Post-Import Cleanup

* Optional deletion of files after a successful import
* Files that fail to import are **not deleted**
* Cleanup actions are fully logged

---

### Error Handling & Logging

* Graceful handling of connection and authentication failures
* Import failures for individual files do not block other files
* Detailed logging of all sync and import operations
* Sync results include:

    * Start and end time
    * Trigger type (scheduled or manual)
    * Number of files found
    * Files imported, skipped, and failed

---

## Technical Highlights

### Architecture

* **Provider abstraction**
  Interface-based design enables support for multiple cloud providers.

* **Dependency Injection**
  Fully integrated with the existing DI infrastructure.

* **Background Service**
  Scheduling is handled via `IHostedService` for reliability.

* **Idempotency**
  Prevents re-processing of the same file within a 24-hour window.

* **Concurrent Protection**
  A semaphore ensures that sync operations cannot overlap.

---

### Security

* Credentials are supplied via configuration or environment variables
* API endpoints are protected using `[Authorize]`
* Sensitive data (tokens, secrets) is never written to logs

---

### Integration

* Reuses the existing import pipeline (`IImport`)
* Uses existing storage abstractions
* Integrates with the current logging infrastructure
* Compatible with the existing DI registration system

---

## Configuration

### JSON Configuration Example

```json
{
  "app": {
    "CloudImport": {
      "Providers": [
        {
          "Id": "dropbox-camera-uploads",
          "Enabled": true,
          "Provider": "Dropbox",
          "RemoteFolder": "/Camera Uploads",
          "SyncFrequencyMinutes": 0,
          "SyncFrequencyHours": 1,
          "DeleteAfterImport": false,
          "Credentials": {
            "RefreshToken": "",
            "AppKey": "",
            "AppSecret": "",
            "ExpiresAt": null
          }
        }
      ]
    }
  }
}
```

---

### Environment Variable Configuration Example

```bash
export app__CloudImport__Providers__0__Id="dropbox-camera-uploads"
export app__CloudImport__Providers__0__Enabled="true"
export app__CloudImport__Providers__0__Provider="Dropbox"
export app__CloudImport__Providers__0__RemoteFolder="/Camera Uploads"
export app__CloudImport__Providers__0__DeleteAfterImport="false"
export app__CloudImport__Providers__0__Credentials__RefreshToken=""
export app__CloudImport__Providers__0__Credentials__AppKey=""
export app__CloudImport__Providers__0__Credentials__AppSecret=""
```

### Filter on extensions

You can also configure which file extensions to include or exclude during the import process.

```json
{
  "app": {
    "CloudImport": {
      "Providers": [
        {
          "other fields are omitted for brevity": "...",
          "Extensions": ["jpg", "png", "mp4"]
        }
      ]
    }
  }
}
```