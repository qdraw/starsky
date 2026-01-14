# EXIF Timezone Correction Feature - Implementation Summary

## Overview

This document provides a complete implementation guide for the EXIF Timezone Correction feature in Starsky. This feature allows users to correct EXIF timestamps for images recorded with the wrong timezone setting.

## Files Created

### 1. Models
**File**: `starsky.foundation.metaupdate/Models/ExifTimezoneCorrection.cs`

Contains two model classes:
- `ExifTimezoneCorrectionRequest`: Input parameters (RecordedTimezone, CorrectTimezone)
- `ExifTimezoneCorrectionResult`: Output result (Success, OriginalDateTime, CorrectedDateTime, DeltaHours, Warning, Error)

### 2. Service
**File**: `starsky.foundation.metaupdate/Services/ExifTimezoneCorrectionService.cs`

Contains:
- `IExifTimezoneCorrectionService`: Interface with methods for single/batch correction and validation
- `ExifTimezoneCorrectionService`: Implementation class

Key methods:
- `CorrectTimezoneAsync(FileIndexItem, ExifTimezoneCorrectionRequest)`: Corrects a single image
- `CorrectTimezoneAsync(List<FileIndexItem>, ExifTimezoneCorrectionRequest)`: Corrects multiple images
- `ValidateCorrection(FileIndexItem, ExifTimezoneCorrectionRequest)`: Validates request before execution
- `CalculateTimezoneDelta(DateTime, string, string)`: DST-aware timezone offset calculation

### 3. Tests
**File**: `starskytest/starsky.foundation.metaupdate/Services/ExifTimezoneCorrectionServiceTest.cs`

Comprehensive test coverage including:
- Validation tests (12 tests covering all error cases)
- Correction tests (positive/negative offsets, DST handling)
- Edge case tests (day rollover, multiple images)
- Error handling tests

### 4. Documentation
**File**: `starsky.foundation.metaupdate/readme-timezone-correction.md`

Complete documentation including:
- Feature overview and problem statement
- Algorithm details
- Usage examples
- Edge cases handled
- API integration examples

## How It Works

### Algorithm

```
1. Validate inputs (timezones, datetime)
2. Get timezone offsets for both timezones at the specific date
3. Calculate delta: correctOffset - recordedOffset
4. Apply delta to DateTime: correctedDateTime = originalDateTime + delta
5. Update FileIndexItem.DateTime with corrected value
6. Write to EXIF using ExifToolCmdHelper.UpdateAsync()
```

### Example Calculation

**Scenario**: Photo taken at 14:30 local time in Amsterdam (GMT+02), but camera was set to UTC (GMT+00)

```
Original EXIF DateTime: 2024-06-15 14:30:00
Recorded Timezone: UTC (offset = +00:00)
Correct Timezone: Europe/Amsterdam (offset = +02:00 in summer)

Delta = +02:00 - 00:00 = +02:00
Corrected DateTime = 14:30:00 + 02:00 = 16:30:00

Result: 2024-06-15 16:30:00
```

## Integration with Existing Code

### Dependencies Used

1. **ExifToolCmdHelper** (`starsky.foundation.writemeta`)
   - Used for writing EXIF metadata
   - Called via `UpdateAsync(FileIndexItem, List<string> comparedNames)`
   - Updates all datetime fields via `-AllDates` command

2. **FileIndexItem** (`starsky.foundation.database`)
   - Standard model for file metadata
   - `DateTime` property stores the timestamp

3. **IReadMeta** (`starsky.foundation.readmeta`)
   - Interface for reading EXIF metadata
   - Passed to service constructor

4. **TimeZoneInfo** (.NET built-in)
   - DST-aware timezone handling
   - No external dependencies required

### Existing Functions Used

**Reading EXIF**:
- `ReadMetaExif.GetExifDateTime()` - Reads DateTime from EXIF
- `ReadMetaExif.ParseSubIfdDateTime()` - Parses EXIF datetime fields

**Writing EXIF**:
- `ExifToolCmdHelper.UpdateAsync()` - Writes EXIF metadata
- `ExifToolCmdHelper.UpdateDateTimeCommand()` - Generates ExifTool command for datetime update

**Command Generated**:
```bash
exiftool -json -overwrite_original \
  -AllDates="2024:06:15 16:30:00" \
  "-xmp:datecreated=2024:06:15 16:30:00" \
  /path/to/image.jpg
```

## EXIF Fields Affected

When updating DateTime, the following EXIF fields are written:
- `DateTimeOriginal` (EXIF tag 36867)
- `DateTimeDigitized` (EXIF tag 36868)
- `DateTime` (EXIF tag 306)
- `xmp:datecreated` (XMP field)

All fields are updated atomically via the `-AllDates` ExifTool parameter.

## Usage Examples

### Single Image Correction

```csharp
// Create service (using dependency injection)
var service = serviceProvider.GetRequiredService<IExifTimezoneCorrectionService>();

// Get image from database
var fileIndexItem = await query.GetObjectByFilePathAsync("/photos/IMG_1234.jpg");

// Create request
var request = new ExifTimezoneCorrectionRequest
{
    RecordedTimezone = "UTC",
    CorrectTimezone = "Europe/Amsterdam"
};

// Validate first (optional but recommended)
var validation = service.ValidateCorrection(fileIndexItem, request);
if (!string.IsNullOrEmpty(validation.Error))
{
    Console.WriteLine($"Cannot correct: {validation.Error}");
    return;
}

if (!string.IsNullOrEmpty(validation.Warning))
{
    Console.WriteLine($"Warning: {validation.Warning}");
    // Prompt user for confirmation
}

// Perform correction
var result = await service.CorrectTimezoneAsync(fileIndexItem, request);

if (result.Success)
{
    Console.WriteLine($"Corrected from {result.OriginalDateTime:yyyy-MM-dd HH:mm:ss} " +
                     $"to {result.CorrectedDateTime:yyyy-MM-dd HH:mm:ss}");
    Console.WriteLine($"Delta: {result.DeltaHours:F2} hours");
}
else
{
    Console.WriteLine($"Error: {result.Error}");
}
```

### Batch Correction

```csharp
// Get multiple images
var fileIndexItems = await query.GetObjectsByFilePathAsync(filePaths);

// Create request
var request = new ExifTimezoneCorrectionRequest
{
    RecordedTimezone = "UTC",
    CorrectTimezone = "Europe/Amsterdam"
};

// Correct all images
var results = await service.CorrectTimezoneAsync(fileIndexItems, request);

// Report results
int successCount = results.Count(r => r.Success);
int failureCount = results.Count(r => !r.Success);

Console.WriteLine($"Success: {successCount}, Failed: {failureCount}");

foreach (var result in results.Where(r => !r.Success))
{
    Console.WriteLine($"Failed: {result.Error}");
}
```

## Dependency Injection Registration

Add to your service registration:

```csharp
// In Startup.cs or Program.cs
services.AddScoped<IExifTimezoneCorrectionService, ExifTimezoneCorrectionService>();
```

The service has dependencies on:
- `IReadMeta` (already registered)
- `ExifToolCmdHelper` (needs to be constructed with dependencies)
- `IWebLogger` (already registered)

Example registration with all dependencies:

```csharp
services.AddScoped<IExifTimezoneCorrectionService>(provider =>
{
    var readMeta = provider.GetRequiredService<IReadMeta>();
    var exifTool = provider.GetRequiredService<IExifTool>();
    var storage = provider.GetRequiredService<ISelectorStorage>().Get(SelectorStorage.StorageServices.SubPath);
    var thumbnailStorage = provider.GetRequiredService<ISelectorStorage>().Get(SelectorStorage.StorageServices.Thumbnail);
    var thumbnailQuery = provider.GetRequiredService<IThumbnailQuery>();
    var logger = provider.GetRequiredService<IWebLogger>();
    
    var exifToolCmdHelper = new ExifToolCmdHelper(
        exifTool, storage, thumbnailStorage, readMeta, thumbnailQuery, logger);
    
    return new ExifTimezoneCorrectionService(readMeta, exifToolCmdHelper, logger);
});
```

## Testing

Run all tests:
```bash
cd /Users/dion/data/git/starsky/starsky
dotnet test --filter "FullyQualifiedName~ExifTimezoneCorrectionServiceTest"
```

Run specific test:
```bash
dotnet test --filter "FullyQualifiedName~ExifTimezoneCorrectionServiceTest.CorrectTimezoneAsync_ValidCorrection_ShouldSucceed"
```

## Edge Cases Handled

✅ **DST (Daylight Saving Time)**
- Offsets calculated based on actual date
- Automatically handles summer/winter time transitions
- Example: Amsterdam is GMT+02 in summer, GMT+01 in winter

✅ **Day/Month/Year Rollover**
- Correctly handles crossing midnight
- Validates and warns user if day changes
- Example: 23:30 + 2 hours = 01:30 next day

✅ **Negative Offsets**
- Supports subtracting time (moving to earlier timezone)
- Example: GMT+02 to GMT+00 = -2 hours

✅ **Missing/Invalid DateTime**
- Validates DateTime.Year >= 2
- Returns error if no valid EXIF datetime exists

✅ **Invalid Timezone IDs**
- Validates both timezone strings using TimeZoneInfo
- Returns descriptive error if timezone not found

✅ **Same Timezone**
- Warns if source and target timezones are identical
- Allows operation but with delta = 0

## Common Timezone IDs

| Region | Timezone ID | Summer Offset | Winter Offset |
|--------|-------------|---------------|---------------|
| UTC | `UTC` | +00:00 | +00:00 |
| Netherlands | `Europe/Amsterdam` | +02:00 | +01:00 |
| UK | `Europe/London` | +01:00 | +00:00 |
| US East | `America/New_York` | -04:00 | -05:00 |
| US West | `America/Los_Angeles` | -07:00 | -08:00 |
| Japan | `Asia/Tokyo` | +09:00 | +09:00 |
| Australia | `Australia/Sydney` | +11:00 | +10:00 |
| New Zealand | `Pacific/Auckland` | +13:00 | +12:00 |

Get all available timezones:
```csharp
var timezones = TimeZoneInfo.GetSystemTimeZones();
foreach (var tz in timezones)
{
    Console.WriteLine($"{tz.Id} - {tz.DisplayName}");
}
```

## Important Notes

⚠️ **Irreversible Operation**
- EXIF timestamps are permanently modified
- Always backup images before batch operations
- Consider implementing undo functionality

⚠️ **No OffsetTime Fields**
- Current implementation doesn't write `OffsetTimeOriginal`, `OffsetTimeDigitized`, or `OffsetTime`
- These fields are optional in EXIF spec
- Future enhancement: Add support for these fields

⚠️ **SubSecTime Preservation**
- Current implementation preserves fractional seconds via ExifTool
- ExifTool automatically maintains SubSecTime fields when using `-AllDates`

⚠️ **Naive Local Time**
- EXIF DateTimeOriginal stores naive local time (no timezone info)
- Correction shifts the stored time value directly
- Result is still naive local time in the correct timezone

## Future Enhancements

1. **OffsetTime Field Support**
   - Read existing OffsetTimeOriginal/Digitized/Time fields
   - Write corrected offset values
   - Validate consistency with corrected datetime

2. **UI Integration**
   - Web interface with timezone picker
   - Before/after preview
   - Batch operation progress indicator
   - Map-based timezone selection

3. **Undo Functionality**
   - Store original values in separate table
   - Allow rollback within time window
   - Version history for metadata changes

4. **Smart Detection**
   - Detect timezone from GPS coordinates
   - Suggest corrections based on location data
   - Auto-detect camera timezone setting errors

5. **CLI Tool**
   - Command-line utility for batch operations
   - Dry-run mode to preview changes
   - CSV export of corrections

## License

This implementation follows the Starsky project license.

## Support

For issues or questions:
1. Check the comprehensive tests in `ExifTimezoneCorrectionServiceTest.cs`
2. Review the documentation in `readme-timezone-correction.md`
3. Examine the algorithm in `ExifTimezoneCorrectionService.cs`
4. Test with a small batch of images first

