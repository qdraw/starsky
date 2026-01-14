# Cloud Import - Quick Start Guide

## Setup with starskyadmincli

You can use `starskyadmincli` to set up Dropbox Cloud Import and obtain a refresh token for secure authentication.

### Steps with starskyadmincli

1. Run the CLI:
   ```bash
   starskyadmincli
   ```
2. Enter your username/email when prompted (e.g., `your-email@gmail.com`). This should be user in starsky
3. Select option `4` for Dropbox Setup.
4. Follow the instructions:
   - Go to [Dropbox App Console](https://www.dropbox.com/developers/apps/create)
   - Choose **Scoped access** and **Full Dropbox**
   - Note your **App key** and **App secret**
   - In the Permissions tab, select:
     - `files.metadata.write`
     - `files.content.write`
     - `files.content.read`
     - `files.metadata.read`
   - Save the app settings.
5. Enter your App Key and App Secret in the CLI when prompted.
6. Open the provided OAuth URL in your browser, authorize, and paste the access code back into the CLI.
7. The CLI will exchange the code for a **refresh token** and display a config snippet to merge into your `appsettings.json`.

Example config to merge:
```json
{
  "app": {
    "CloudImport": {
      "providers": [
        {
          "id": "dropbox-import-example-id",
          "enabled": true,
          "provider": "Dropbox",
          "remoteFolder": "/Camera Uploads",
          "syncFrequencyMinutes": 0,
          "syncFrequencyHours": 0,
          "deleteAfterImport": false,
          "extensions": [],
          "credentials": {
            "refreshToken": "<your-refresh-token>",
            "appKey": "<your-app-key>",
            "appSecret": "<your-app-secret>"
          }
        }
      ]
    }
  }
}
```

> **Tip:** Store credentials using environment variables in production for security.

## Prerequisites

1. A Dropbox account with API access
2. Starsky application running

## Setup Steps

### 1. Create a Dropbox App (Manual)

If not using the CLI, you can manually create a Dropbox app as described above.

### 2. Generate Access Token / Refresh Token

For automated sync, use the OAuth flow to obtain a **refresh token** (recommended). The CLI guides you through this process. If you use the manual method, you may only get a short-lived access token.

### 3. Configure Starsky

Merge the config snippet from the CLI (or above) into your `appsettings.json` under the `app` section. Ensure your credentials and refresh token are correct.


### Environment Variable Configuration Example

```bash
export app__CloudImport__Providers__0__Id="dropbox-import-example-id"
export app__CloudImport__Providers__0__Enabled="true"
export app__CloudImport__Providers__0__Provider="Dropbox"
export app__CloudImport__Providers__0__RemoteFolder="/Camera Uploads"
export app__CloudImport__Providers__0__DeleteAfterImport="false"
export app__CloudImport__Providers__0__Credentials__RefreshToken="<your-refresh-token>"
export app__CloudImport__Providers__0__Credentials__AppKey="<your-app-key>"
export app__CloudImport__Providers__0__Credentials__AppSecret="<your-app-secret>"
```

### 4. Start Starsky

Start the Starsky application. The Cloud Import service will automatically begin syncing on the configured schedule.

## Usage

You can trigger manual syncs and check status using the API endpoints. See below for examples.

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
7. **Use starskyadmincli for secure setup** - It simplifies OAuth and config generation.

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


