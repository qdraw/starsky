# Synchronization

Starsky includes powerful synchronization features to keep your photo library in sync with your file system. The synchronization tools monitor changes, detect new files, and ensure your database accurately reflects what's on disk.

## 🔄 Synchronization Features

### Automatic File System Monitoring

Starsky can automatically detect changes to your photo library:

- **Real-time Detection** - Notices when files are added, modified, or deleted
- **DiskWatcher** - Background service that monitors file system changes
- **Fast Performance** - Optimized to check for duplicate events and filter them (max 20 seconds)
- **Database Updates** - Automatically updates the database when changes are detected

### Manual Synchronization

Use the synchronization CLI tool to manually sync:

- **starskySync CLI** - Command-line tool for synchronization tasks
- **Database Comparison** - Compares file system with database
- **Performance Improvements** - Significantly faster than previous versions
- **Comprehensive Scanning** - Ensures all changes are captured

---

## 🔍 How Synchronization Works

### File System to Database

The sync process ensures your database matches your files:

1. **Scan File System** - Checks all files in your photo directories
2. **Compare with Database** - Identifies differences between disk and database
3. **Update Records** - Adds new files, removes deleted files, updates modified files
4. **Metadata Sync** - Reads metadata from files and updates database

### Database to File System

Starsky stores metadata in the actual image files:

- **IPTC/XMP Standards** - All metadata is written to files
- **Persistent Storage** - Even if you delete the database, metadata remains
- **Recovery** - A rescan will restore all metadata from files
- **Cross-Platform** - Works with any tool that supports IPTC/XMP

---

## ⚡ DiskWatcher

### Real-time File Monitoring

The DiskWatcher service provides automatic synchronization:

- **Event Detection** - Monitors create, modify, delete, and rename events
- **Duplicate Filtering** - Filters out duplicate events within 20 seconds
- **Performance Optimized** - Major rewrite in v0.5.x for better performance
- **Background Operation** - Runs continuously without blocking other operations

### Event Processing

- **File Added** - New files are automatically indexed
- **File Modified** - Metadata changes are detected and updated
- **File Deleted** - Database records are removed
- **File Renamed** - Updates database paths accordingly

---

## 🛠️ starskySync CLI

### Command-Line Synchronization

The `starskySynchronizeCli` tool checks if disk changes are updated in the database:

- **Manual Sync** - Run synchronization on demand
- **Scheduled Jobs** - Integrate with cron or task scheduler
- **Comprehensive** - Scans entire library or specific folders
- **Verification** - Ensures database integrity

### Use Cases

- Run periodic syncs to verify database accuracy
- Recover from DiskWatcher being offline
- Initial scan of large photo libraries
- Verify sync after bulk file operations

---

## 📊 Synchronization Status

### Monitoring Sync Operations

Check the status of synchronization:

- **Real-time Updates** - See sync progress via realtime feature
- **Logs** - Review synchronization logs for details
- **Error Detection** - Identifies files that couldn't be synced
- **Performance Metrics** - Track sync speed and efficiency

---

## 🚀 Use Cases

### External Edits

When you edit photos outside of Starsky:

1. Edit metadata in Lightroom, Photoshop, or Exiftool
2. DiskWatcher detects the file changes
3. Starsky automatically updates the database
4. Changes appear in the interface

### Bulk File Operations

After copying or moving many files:

1. Add files to your photo directory via file manager
2. Run `starskySynchronizeCli` to scan for changes
3. Database is updated with all new files
4. Browse and search newly added photos

### Server Migrations

When moving to a new server:

1. Copy all photo files to new location
2. Install Starsky and create new database
3. Run synchronization to index all files
4. All metadata is restored from file headers

### Backup Recovery

Restore from backup without losing metadata:

1. Restore photo files from backup
2. Database may be outdated or missing
3. Run full synchronization
4. Metadata is read from files and database is rebuilt

---

## 🔍 Technical Details

### Performance Optimizations

- **Duplicate Event Filtering** - Prevents redundant processing (20-second window)
- **Incremental Updates** - Only processes changed files
- **Parallel Processing** - Handles multiple files simultaneously
- **Optimized Queries** - Fast database comparisons

### Metadata Preservation

- **File-based Storage** - All metadata written to IPTC/XMP fields
- **No Lock-in** - Metadata survives database deletion
- **Universal Standards** - Compatible with other photo tools
- **Automatic Recovery** - Rescan restores everything

---

## 🗂️ Related Features

- **[Realtime](realtime.md)** - See sync updates in real-time
- **[Metadata](metadata.md)** - Understanding metadata storage
- **[Import](import.md)** - Import automatically syncs new files
- **[Search](search.md)** - Search synced metadata

---

## 📝 Best Practices

### Regular Synchronization

- Let DiskWatcher run continuously for best results
- Schedule periodic manual syncs as backup verification
- Monitor logs for any sync errors
- Keep large file operations outside peak usage times

### External Editing

- Use IPTC/XMP-compatible tools for external edits
- Allow time for DiskWatcher to detect changes
- Manually trigger sync if changes don't appear immediately
- Verify metadata after bulk external edits

### Large Libraries

- Initial sync of large libraries may take time
- Use `starskySynchronizeCli` for first-time indexing
- Enable DiskWatcher after initial sync completes
- Monitor performance and adjust buffer sizes if needed

---

## ⚠️ Troubleshooting

### Changes Not Appearing

- Check if DiskWatcher is running
- Verify file permissions are correct
- Run manual sync with `starskySynchronizeCli`
- Check logs for error messages

### Performance Issues

- Increase DiskWatcher buffer size (v0.7.2+)
- Reduce number of monitored directories
- Schedule intensive syncs during off-hours
- Check disk I/O performance

### Database Inconsistencies

- Run full synchronization to verify all files
- Check for files that are in database but not on disk
- Verify metadata is correctly written to files
- Review sync logs for processing errors
