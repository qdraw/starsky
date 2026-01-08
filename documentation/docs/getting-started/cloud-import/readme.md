# Cloud Import - Quick Start Guide

## Prerequisites

1. A Dropbox account with API access
2. Starsky application running

## Setup Steps

### 1. Create a Dropbox App

1. Go to https://www.dropbox.com/developers/apps
2. Click "Create App"
3. Choose "Scoped access"
4. Choose "Full Dropbox" access
5. Name your app (e.g., "Starsky Photo Sync")
6. Click "Create App"

### 2. Generate Access Token

1. In your app's settings page, scroll to "OAuth 2"
2. Under "Generated access token", click "Generate"
3. Copy the access token (keep it secure!)

### 3. Configure Starsky

Add to your `appsettings.json`:

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
            "AppSecret": ""
          }
        }
      ]
    }
  }
}
```


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

### 4. Start Starsky

The Cloud Import service will automatically start and begin syncing on the configured schedule.

## Usage

### Automatic Sync

Once configured, the sync will run automatically based on your `SyncFrequencyMinutes` or `SyncFrequencyHours` setting.

### Manual Sync

You can trigger a manual sync using the API:

```bash
curl -X POST https://your-starsky-instance/api/CloudImport/sync \
  -H "Authorization: Bearer YOUR_AUTH_TOKEN"
```

### Check Status

```bash
curl https://your-starsky-instance/api/CloudImport/status \
  -H "Authorization: Bearer YOUR_AUTH_TOKEN"
```

### View Last Result

```bash
curl https://your-starsky-instance/api/CloudImport/last-result \
  -H "Authorization: Bearer YOUR_AUTH_TOKEN"
```

## Configuration Options

### Sync Frequency

Choose between minute-based or hour-based intervals:

**Every 30 minutes:**
```json
{
  "SyncFrequencyMinutes": 30,
  "SyncFrequencyHours": 0
}
```

**Every 6 hours:**
```json
{
  "SyncFrequencyMinutes": 0,
  "SyncFrequencyHours": 6
}
```

> **Note:** If `SyncFrequencyMinutes` is greater than 0, it takes priority over `SyncFrequencyHours`.

### Delete After Import

Set `DeleteAfterImport` to `true` to automatically remove files from Dropbox after successful import:

```json
{
  "DeleteAfterImport": true
}
```

⚠️ **Warning:** This permanently deletes files from your Dropbox. Use with caution!

### Remote Folder

Specify which Dropbox folder to sync from:

```json
{
  "RemoteFolder": "/Camera Uploads"
}
```

Or the root folder:

```json
{
  "RemoteFolder": "/"
}
```

## Monitoring

### Logs

Cloud Import operations are logged with detailed information:

- Sync start/end times
- Files found and processed
- Import successes and failures
- Connection issues
- Errors with stack traces

Check your application logs for entries containing "Cloud Import" or "CloudImport".

### Status Endpoint

The status endpoint provides real-time information:

```json
{
  "enabled": true,
  "provider": "Dropbox",
  "remoteFolder": "/Camera Uploads",
  "syncFrequencyMinutes": 0,
  "syncFrequencyHours": 1,
  "deleteAfterImport": false,
  "isSyncInProgress": false,
  "lastSyncResult": {
    "startTime": "2026-01-05T10:00:00Z",
    "endTime": "2026-01-05T10:05:30Z",
    "triggerType": "Scheduled",
    "filesFound": 10,
    "filesImportedSuccessfully": 8,
    "filesSkipped": 2,
    "filesFailed": 0,
    "errors": [],
    "successfulFiles": ["photo1.jpg", "photo2.jpg", ...],
    "failedFiles": [],
    "success": true
  }
}
```

## Troubleshooting

### Sync Not Running

1. Check that `Enabled` is set to `true`
2. Verify the access token is correct
3. Check application logs for errors
4. Ensure the Dropbox app has the correct permissions

### Connection Failures

- Verify internet connectivity
- Check that the access token hasn't expired
- Ensure Dropbox API is accessible from your server

### Import Failures

- Check that files are valid image formats
- Verify sufficient disk space
- Review import pipeline logs
- Check file permissions

### Files Not Being Deleted

- Verify `DeleteAfterImport` is `true`
- Ensure the access token has delete permissions
- Check logs for delete operation errors

## Best Practices

1. **Start with `DeleteAfterImport: false`** - Test the sync first before enabling automatic deletion
2. **Use environment variables for credentials** - Keep tokens out of config files
3. **Monitor logs regularly** - Watch for sync failures or connection issues
4. **Set appropriate sync intervals** - Balance between timeliness and API rate limits
5. **Backup your Dropbox** - Before enabling delete after import
6. **Test with a subfolder first** - Use a specific folder to verify behavior

## Security

- Keep your access token secure
- Don't commit tokens to version control
- Use environment variables in production
- Regularly rotate access tokens
- Limit Dropbox app permissions to only what's needed
- Enable authentication on API endpoints (already configured)

## Support

For issues or questions:
1. Check application logs
2. Verify configuration matches examples above
3. Test Dropbox connectivity separately


