# Cloud Sync Import - Migration and Upgrade Guide

## For Existing Starsky Users

This guide helps existing Starsky users understand and adopt the new cloud sync import feature.

## What's New?

The cloud sync import feature automates the process of importing photos from cloud storage services like Dropbox. Instead of manually uploading files, the system can now automatically download and import files on a schedule.

## Benefits

- **Automated workflow**: Set it and forget it - files sync automatically
- **Mobile integration**: Upload from your phone to Dropbox, auto-import to Starsky
- **Reduced manual work**: No more manual file transfers
- **Consistent processing**: Uses the same import pipeline you're already familiar with
- **Optional cleanup**: Automatically remove files from cloud storage after import

## Compatibility

✅ **Fully compatible** with existing Starsky installations
✅ **No breaking changes** - existing functionality remains unchanged
✅ **Optional feature** - disabled by default, enable when ready
✅ **Uses existing import logic** - same validation and processing

## Installation Steps

### 1. Update Dependencies

The cloud sync feature requires the Dropbox.Api NuGet package. If building from source:

```bash
cd starsky.foundation.cloudsync
dotnet restore
```

### 2. Add Configuration

Add the following section to your `appsettings.json`:

```json
{
  "CloudSync": {
    "Enabled": false,
    "Provider": "Dropbox",
    "RemoteFolder": "/",
    "SyncFrequencyMinutes": 0,
    "SyncFrequencyHours": 24,
    "DeleteAfterImport": false,
    "Credentials": {
      "AccessToken": "",
      "RefreshToken": "",
      "AppKey": "",
      "AppSecret": "",
      "ExpiresAt": null
    }
  }
}
```

> **Note:** The feature is **disabled by default** (`Enabled: false`), so it won't affect your existing setup.

### 3. Restart Application

After adding the configuration, restart your Starsky application to load the new settings.

## Enabling Cloud Sync

When you're ready to use cloud sync:

1. **Set up Dropbox** (see Quick Start Guide)
2. **Update configuration**:
   ```json
   {
     "CloudSync": {
       "Enabled": true,
       "Provider": "Dropbox",
       "RemoteFolder": "/Camera Uploads",
       "SyncFrequencyHours": 1,
       "Credentials": {
         "AccessToken": "YOUR_ACCESS_TOKEN"
       }
     }
   }
   ```
3. **Restart application**
4. **Monitor logs** to ensure sync is working

## Migration Strategies

### Strategy 1: Parallel Operation

Run cloud sync alongside your existing import workflow:

1. Enable cloud sync with `DeleteAfterImport: false`
2. Continue using your existing import method
3. Monitor sync results
4. Once confident, optionally enable `DeleteAfterImport`
5. Gradually phase out manual imports

**Pros:**
- Safe, gradual transition
- Easy rollback
- No disruption

**Cons:**
- Duplicate work during transition
- More disk usage

### Strategy 2: Direct Migration

Switch entirely to cloud sync:

1. Set up cloud sync with appropriate folder
2. Test with a few files first
3. Enable cloud sync
4. Stop manual imports
5. Optionally enable `DeleteAfterImport`

**Pros:**
- Clean transition
- Immediate benefits

**Cons:**
- Requires confidence in setup
- Higher initial risk

### Strategy 3: Hybrid Approach

Use both methods for different purposes:

1. Cloud sync for mobile/automatic imports
2. Manual imports for bulk operations
3. Keep both running indefinitely

**Pros:**
- Flexibility
- Best of both worlds

**Cons:**
- More complex workflow
- Requires managing both systems

## What Stays the Same

- ✅ Import validation logic
- ✅ File processing and metadata extraction
- ✅ Database structure
- ✅ Thumbnail generation
- ✅ Storage organization
- ✅ API endpoints (existing)
- ✅ User interface
- ✅ Authentication and authorization

## What's New

- ✅ Scheduled background sync
- ✅ Cloud provider integration
- ✅ Automatic file download
- ✅ Optional post-import cleanup
- ✅ New API endpoints:
  - `GET /api/cloudsync/status`
  - `POST /api/cloudsync/sync`
  - `GET /api/cloudsync/last-result`

## Configuration Mapping

If you have existing import settings, here's how they map to cloud sync:

| Existing Import Setting | Cloud Sync Equivalent |
|------------------------|----------------------|
| Manual trigger | `POST /api/cloudsync/sync` |
| Import validation | Automatic (same logic) |
| Storage folder | Unchanged (uses existing) |
| Metadata extraction | Automatic (same logic) |
| Thumbnail generation | Automatic (same logic) |

## Common Questions

### Q: Will this affect my existing imports?
**A:** No. Cloud sync is a separate feature that works alongside existing import functionality.

### Q: Can I use both manual and automatic import?
**A:** Yes. You can use manual imports and cloud sync simultaneously.

### Q: What happens if a file fails to import?
**A:** The file remains in cloud storage (if `DeleteAfterImport: true`), and the failure is logged. Other files continue processing.

### Q: Can I import from multiple cloud providers?
**A:** Currently, only one provider is active at a time. Multiple providers can be configured by adding additional `ICloudSyncClient` implementations.

### Q: Does this cost anything?
**A:** The feature itself is free. You need a Dropbox account (free tier works), and standard Dropbox API usage applies.

### Q: What about privacy/security?
**A:** The feature uses Dropbox's official API with OAuth2 tokens. Your credentials stay in your configuration and are never shared.

### Q: Can I disable it later?
**A:** Yes, simply set `Enabled: false` in configuration and restart.

## Rollback Plan

If you need to disable cloud sync:

1. Set `Enabled: false` in configuration
2. Restart application
3. Remove `CloudSync` section from configuration (optional)
4. Return to previous import method

All existing data remains intact. Cloud sync doesn't modify your existing files or database.

## Performance Considerations

- **Network bandwidth**: Files are downloaded during sync
- **Storage space**: Downloaded files need temporary space
- **CPU usage**: Import processing uses existing resources
- **Sync frequency**: More frequent syncs = more resource usage

**Recommendations:**
- Start with hourly syncs (or less frequent)
- Monitor system resources
- Adjust frequency based on your needs

## Monitoring

Track cloud sync health using:

1. **Application logs** - detailed sync information
2. **Status endpoint** - real-time status
3. **Last result endpoint** - sync history
4. **System monitoring** - resource usage

## Support & Troubleshooting

If you encounter issues:

1. Check logs for error messages
2. Verify configuration
3. Test Dropbox connectivity
4. Review Quick Start Guide
5. Check Implementation Documentation

## Best Practices for Migration

1. ✅ **Test in development first** - verify behavior before production
2. ✅ **Start with `DeleteAfterImport: false`** - ensure imports work before enabling cleanup
3. ✅ **Monitor initially** - watch logs closely during first few sync cycles
4. ✅ **Backup your data** - before enabling automatic deletion
5. ✅ **Use environment variables** - for credentials in production
6. ✅ **Set conservative intervals** - start with longer intervals, optimize later

## Timeline Recommendation

**Week 1:**
- Add configuration (disabled)
- Set up Dropbox app and token
- Test manual sync trigger

**Week 2:**
- Enable cloud sync
- Monitor logs
- Verify successful imports

**Week 3:**
- Adjust sync frequency
- Test post-import deletion (if desired)
- Continue parallel operations

**Week 4+:**
- Transition fully to cloud sync (if desired)
- Or maintain hybrid approach

## Getting Help

- Documentation: `starsky.foundation.cloudsync/README-IMPLEMENTATION.md`
- Quick Start: `CLOUDSYNC-QUICKSTART.md`
- Application logs
- Starsky community/support channels

## Feedback

This is a new feature. Please provide feedback on:
- Performance
- Reliability
- Ease of use
- Missing features
- Documentation clarity

Your feedback helps improve the feature for everyone!

