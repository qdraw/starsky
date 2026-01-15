# EXIF Timezone Correction Feature

## Overview

This feature allows users to correct EXIF timestamps for images that were recorded in the wrong timezone. It rewrites the stored date/time values so the actual moment in time is correct.

## Problem Statement

When a camera's timezone is incorrectly set, EXIF timestamps are recorded with the wrong local time. For example:
- A photo taken at 14:30 local time in GMT+02 (Amsterdam in summer)
- But the camera was set to GMT+00 (UTC)
- The EXIF data stores "14:30" when it should store "16:30"

## Solution

This feature corrects these timestamps by:
1. Accepting two timezones: "Recorded Timezone" (what the camera thought) and "Correct Timezone" (actual location)
2. Calculating the offset difference between the two timezones (DST-aware)
3. Applying the delta to all EXIF datetime fields
4. Rewriting the EXIF metadata

## Implementation

### Core Components

#### 1. Models (`starsky.foundation.metaupdate/Models/ExifTimezoneCorrection.cs`)

**ExifTimezoneCorrectionRequest**
- `RecordedTimezone`: The timezone the camera thought it was in (e.g., "UTC", "Europe/Amsterdam")
- `CorrectTimezone`: The actual timezone where the photo was taken

**ExifTimezoneCorrectionResult**
- `Success`: Whether the correction succeeded
- `OriginalDateTime`: Original timestamp before correction
- `CorrectedDateTime`: Corrected timestamp after correction
- `DeltaHours`: Time difference applied (in hours)
- `Warning`: Any warnings (e.g., day rollover)
- `Error`: Error message if correction failed

#### 2. Service (`starsky.foundation.metaupdate/Services/ExifTimezoneCorrectionService.cs`)

**IExifTimezoneCorrectionService** interface:
- `CorrectTimezoneAsync(FileIndexItem, ExifTimezoneCorrectionRequest)`: Correct single image
- `CorrectTimezoneAsync(List<FileIndexItem>, ExifTimezoneCorrectionRequest)`: Correct multiple images
- `ValidateCorrection(FileIndexItem, ExifTimezoneCorrectionRequest)`: Validate request before correction

**ExifTimezoneCorrectionService** implementation:
- Uses existing `ExifToolCmdHelper` to write EXIF data
- Uses existing `IReadMeta` to read EXIF data
- Calculates DST-aware timezone offsets using `TimeZoneInfo`

### Algorithm

```csharp
// 1. Validate timezones and datetime
if (!ValidateCorrection()) return error;

// 2. Get timezone offsets at the specific date (handles DST)
var recordedOffset = recordedTz.GetUtcOffset(dateTime);
var correctOffset = correctTz.GetUtcOffset(dateTime);

// 3. Calculate delta
var delta = correctOffset - recordedOffset;

// 4. Apply correction
var correctedDateTime = originalDateTime.Add(delta);

// 5. Update FileIndexItem
fileIndexItem.DateTime = correctedDateTime;

// 6. Write to EXIF using ExifToolCmdHelper
await _exifToolCmdHelper.UpdateAsync(fileIndexItem, ["datetime"]);
```

### EXIF Fields Affected

When `DateTime` is updated, the `ExifToolCmdHelper` writes:
- `DateTimeOriginal`
- `DateTimeDigitized`
- `DateTime`
- `xmp:datecreated`

All fields are updated consistently via the `-AllDates` ExifTool command.

## Usage

### Single Image Correction

```csharp
var service = new ExifTimezoneCorrectionService(readMeta, exifToolCmdHelper, logger);

var fileIndexItem = new FileIndexItem 
{
    FilePath = "/photos/image.jpg",
    DateTime = new DateTime(2024, 6, 15, 14, 30, 0)
};

var request = new ExifTimezoneCorrectionRequest
{
    RecordedTimezone = "UTC",           // Camera was set to UTC
    CorrectTimezone = "Europe/Amsterdam" // Photo was taken in Amsterdam
};

var result = await service.CorrectTimezoneAsync(fileIndexItem, request);

if (result.Success)
{
    Console.WriteLine($"Corrected from {result.OriginalDateTime} to {result.CorrectedDateTime}");
    Console.WriteLine($"Delta: {result.DeltaHours} hours");
}
else
{
    Console.WriteLine($"Error: {result.Error}");
}
```

### Batch Correction

```csharp
var fileIndexItems = new List<FileIndexItem> { /* multiple images */ };
var results = await service.CorrectTimezoneAsync(fileIndexItems, request);

foreach (var result in results)
{
    if (result.Success)
        Console.WriteLine($"✓ {result.CorrectedDateTime}");
    else
        Console.WriteLine($"✗ {result.Error}");
}
```

### Validation

```csharp
var validation = service.ValidateCorrection(fileIndexItem, request);

if (!string.IsNullOrEmpty(validation.Error))
{
    Console.WriteLine($"Cannot correct: {validation.Error}");
}

if (!string.IsNullOrEmpty(validation.Warning))
{
    Console.WriteLine($"Warning: {validation.Warning}");
    // Prompt user for confirmation
}
```

## Edge Cases Handled

### 1. DST (Daylight Saving Time)
- ✅ Offsets are calculated based on the actual date
- ✅ Summer vs. winter time is automatically handled
- Example: Amsterdam is GMT+02 in summer, GMT+01 in winter

### 2. Day/Month/Year Rollover
- ✅ Crossing midnight is handled correctly
- ✅ Validation warns user if day will change
- Example: 23:30 + 2 hours = 01:30 next day

### 3. Missing/Invalid DateTime
- ✅ Validates that DateTime.Year >= 2
- ✅ Returns error if no valid EXIF datetime

### 4. Invalid Timezones
- ✅ Validates both timezone IDs using `TimeZoneInfo.FindSystemTimeZoneById`
- ✅ Returns error if timezone doesn't exist

### 5. Same Timezone
- ✅ Warns if recorded and correct timezones are identical
- ✅ No correction applied (delta = 0)

## Timezone IDs

Use standard timezone IDs from `TimeZoneInfo`:

**Common Timezones:**
- `UTC` - Coordinated Universal Time (GMT+00)
- `Europe/Amsterdam` - Central European Time (GMT+01/+02)
- `Europe/London` - British Time (GMT+00/+01)
- `America/New_York` - Eastern Time (GMT-05/-04)
- `America/Los_Angeles` - Pacific Time (GMT-08/-07)
- `Asia/Tokyo` - Japan Time (GMT+09)
- `Pacific/Auckland` - New Zealand Time (GMT+12/+13)

Get all available timezones:
```csharp
var timezones = TimeZoneInfo.GetSystemTimeZones();
```

## Testing

Comprehensive tests are in `starskytest/starsky.foundation.metaupdate/Services/ExifTimezoneCorrectionServiceTest.cs`:

- ✅ Validation tests (missing fields, invalid timezones, invalid datetime)
- ✅ Correction tests (positive offset, negative offset, zero offset)
- ✅ DST tests (summer time, winter time)
- ✅ Edge case tests (day rollover, month rollover)
- ✅ Batch correction tests
- ✅ Error handling tests

Run tests:
```bash
dotnet test --filter ExifTimezoneCorrectionServiceTest
```

## Integration with Existing Code

This feature integrates seamlessly with existing infrastructure:

1. **Uses `ExifToolCmdHelper`**: Leverages the existing EXIF writing mechanism
2. **Uses `IReadMeta`**: Reads current EXIF data
3. **Uses `FileIndexItem`**: Standard model for file metadata
4. **Uses `IWebLogger`**: Standard logging interface
5. **Follows existing patterns**: Async operations, validation, error handling

## Important Notes

⚠️ **This is an irreversible operation** - Always:
1. Validate before applying
2. Show preview to user
3. Consider backing up images

⚠️ **Timezone mental model**: 
- User thinks: "Photo taken at 14:30 in Amsterdam, but camera stored as UTC"
- EXIF stores: Naive local time (no offset info)
- Correction: Shift the stored time by the delta

⚠️ **No OffsetTime fields**: 
- EXIF standard doesn't require `OffsetTimeOriginal`, etc.
- This feature rewrites the datetime values directly
- Consider adding OffsetTime fields in future enhancement

## Future Enhancements

1. **Read/Write OffsetTime fields**: If present, update `OffsetTimeOriginal`, `OffsetTimeDigitized`, `OffsetTime`
2. **SubSecTime preservation**: Preserve fractional seconds if present
3. **UI Integration**: Add to web interface with timezone picker
4. **Batch preview**: Show before/after preview for all images
5. **Undo functionality**: Store original values for rollback

## API Integration (Future)

```csharp
// Example REST API endpoint
[HttpPost("/api/correct-timezone")]
public async Task<ActionResult<List<ExifTimezoneCorrectionResult>>> CorrectTimezone(
    [FromBody] ExifTimezoneCorrectionApiRequest request)
{
    var fileIndexItems = await _query.GetObjectsByFilePathAsync(request.FilePaths);
    
    var correctionRequest = new ExifTimezoneCorrectionRequest
    {
        RecordedTimezone = request.RecordedTimezone,
        CorrectTimezone = request.CorrectTimezone
    };
    
    var results = await _timezoneCorrectionService.CorrectTimezoneAsync(
        fileIndexItems, 
        correctionRequest);
    
    return Ok(results);
}
```

## Dependencies

No new external dependencies required. Uses:
- `System.TimeZoneInfo` (built-in .NET)
- Existing ExifTool integration
- Existing EXIF read/write infrastructure

## License

Same as Starsky project license.

