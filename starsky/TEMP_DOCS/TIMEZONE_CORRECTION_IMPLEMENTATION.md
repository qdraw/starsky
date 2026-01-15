# EXIF Timezone Correction: Implementation Details

## Architecture Overview

### Existing Components

1. **Models** (`starsky.foundation.metaupdate/Models/`)
   - `ExifTimezoneCorrectionRequest` - Input parameters
   - `ExifTimezoneCorrectionResult` - Operation result

2. **Interface** (`starsky.foundation.metaupdate/Interfaces/`)
   - `IExifTimezoneCorrectionService` - Contract

3. **Service** (`starsky.foundation.metaupdate/Services/`)
   - `ExifTimezoneCorrectionService` - Implementation

4. **EXIF Writing** (`starsky.foundation.writemeta/Helpers/`)
   - `ExifToolCmdHelper` - Writes EXIF metadata via ExifTool

---

## Core Algorithm: CalculateTimezoneDelta

### How It Works

```csharp
private static TimeSpan CalculateTimezoneDelta(
    DateTime dateTime,
    string recordedTimezone,
    string correctTimezone)
{
    // Get timezone objects
    var recordedTz = TimeZoneInfo.FindSystemTimeZoneById(recordedTimezone);
    var correctTz = TimeZoneInfo.FindSystemTimeZoneById(correctTimezone);

    // Get UTC offsets for the photo date (DST-aware!)
    var recordedOffset = recordedTz.GetUtcOffset(dateTime);
    var correctOffset = correctTz.GetUtcOffset(dateTime);

    // Calculate delta
    var delta = correctOffset - recordedOffset;

    return delta;
}
```

### Key Points

1. **`TimeZoneInfo.GetUtcOffset(dateTime)`** is DST-aware
   - Automatically returns +1 or +2 for Europe/Amsterdam based on date
   - Respects all DST rules for any timezone

2. **Delta is Calculated Per-Photo**
   - Photos before/after DST transition get different deltas
   - System automatically handles this

3. **Order Matters**
   - `delta = correctOffset - recordedOffset`
   - If you reverse them, correction goes the wrong way

### Example: April 15, 2026 in Amsterdam

```csharp
dateTime = new DateTime(2026, 4, 15, 14, 0, 0);
recordedTimezone = "Etc/GMT-1";      // Fixed UTC+1
correctTimezone = "Europe/Amsterdam"; // UTC+2 (DST active)

recordedTz = TimeZoneInfo.FindSystemTimeZoneById("Etc/GMT-1");
correctTz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Amsterdam");

recordedOffset = recordedTz.GetUtcOffset(dateTime);  // +01:00
correctOffset = correctTz.GetUtcOffset(dateTime);   // +02:00

delta = correctOffset - recordedOffset;             // +01:00

correctedTime = dateTime.Add(delta);                // 14:00 + 1h = 15:00
```

---

## Service Flow: CorrectTimezoneAsync

### Step 1: Validate Input

```csharp
public ExifTimezoneCorrectionResult ValidateCorrection(
    FileIndexItem fileIndexItem,
    ExifTimezoneCorrectionRequest request)
{
    // Check file exists
    if (!_storage.ExistFile(fileIndexItem.FilePath!))
    {
        result.Error = "File does not exist";
        return result;
    }

    // Validate timezone IDs are valid
    try
    {
        TimeZoneInfo.FindSystemTimeZoneById(request.RecordedTimezone);
    }
    catch
    {
        result.Error = $"Invalid recorded timezone: {request.RecordedTimezone}";
        return result;
    }

    try
    {
        TimeZoneInfo.FindSystemTimeZoneById(request.CorrectTimezone);
    }
    catch
    {
        result.Error = $"Invalid correct timezone: {request.CorrectTimezone}";
        return result;
    }

    // Check image has DateTime
    if (fileIndexItem.DateTime.Year < 2)
    {
        result.Error = "Image does not have a valid DateTime in EXIF";
        return result;
    }

    // Warn if timezones are the same
    if (request.RecordedTimezone == request.CorrectTimezone)
    {
        result.Warning = "Recorded and correct timezones are the same - no correction needed";
    }

    // Warn if day would change
    var delta = CalculateTimezoneDelta(
        fileIndexItem.DateTime,
        request.RecordedTimezone,
        request.CorrectTimezone);
    var correctedDateTime = fileIndexItem.DateTime.Add(delta);

    if (correctedDateTime.Day != fileIndexItem.DateTime.Day)
    {
        result.Warning = $"Date will change from {fileIndexItem.DateTime:yyyy-MM-dd} " +
                         $"to {correctedDateTime:yyyy-MM-dd}";
    }

    return result;
}
```

### Step 2: Calculate Correction

```csharp
// Calculate the timezone delta
var delta = CalculateTimezoneDelta(
    fileIndexItem.DateTime,
    request.RecordedTimezone,
    request.CorrectTimezone);

result.OriginalDateTime = fileIndexItem.DateTime;
result.DeltaHours = delta.TotalHours;

// Apply the correction
var correctedDateTime = fileIndexItem.DateTime.Add(delta);
result.CorrectedDateTime = correctedDateTime;
```

### Step 3: Write to EXIF

```csharp
// Update the FileIndexItem with corrected DateTime
fileIndexItem.DateTime = correctedDateTime;

// Write the corrected DateTime to EXIF
var comparedNames = new List<string> 
{ 
    nameof(FileIndexItem.DateTime).ToLowerInvariant() 
};

await _exifToolCmdHelper.UpdateAsync(
    fileIndexItem,
    comparedNames,
    false);

result.Success = true;
```

### Step 4: Batch Processing

```csharp
public async Task<List<ExifTimezoneCorrectionResult>> CorrectTimezoneAsync(
    List<FileIndexItem> fileIndexItems,
    ExifTimezoneCorrectionRequest request)
{
    var results = new List<ExifTimezoneCorrectionResult>();

    foreach (var item in fileIndexItems)
    {
        var result = await CorrectTimezoneAsync(item, request);
        results.Add(result);
    }

    return results;
}
```

---

## ExifToolCmdHelper Integration

### Current DateTime Writing

The existing code in `ExifToolCmdHelper.UpdateDateTimeCommand()`:

```csharp
private static string UpdateDateTimeCommand(string command, List<string> comparedNames,
    FileIndexItem updateModel)
{
    if (comparedNames.Contains(nameof(FileIndexItem.DateTime).ToLowerInvariant()) &&
        updateModel.DateTime.Year > 2)
    {
        var exifToolDatetimeString = updateModel.DateTime.ToString(
            "yyyy:MM:dd HH:mm:ss",
            CultureInfo.InvariantCulture);
        
        command += $" -AllDates=\"{exifToolDatetimeString}\" " +
                  $"\"-xmp:datecreated={exifToolDatetimeString}\"";
    }

    return command;
}
```

### What It Does

- `-AllDates=` updates all EXIF datetime fields at once:
  - DateTimeOriginal
  - DateTimeDigitized
  - DateTime
- `-xmp:datecreated=` updates XMP equivalent

### Enhancement Needed (Future)

To fully support timezone correction, we could enhance this to also write:

```csharp
// Write OffsetTime fields (optional but recommended)
command += $" -OffsetTimeOriginal=\"{offsetTimeString}\" ";
command += $" -OffsetTimeDigitized=\"{offsetTimeString}\" ";
command += $" -OffsetTime=\"{offsetTimeString}\" ";
```

Current implementation: ✓ Works fine with `-AllDates`
Future enhancement: Write offset fields to document the timezone

---

## Usage Examples

### Example 1: Single Image

```csharp
var fileIndexItem = new FileIndexItem
{
    FilePath = "/photos/camera.jpg",
    DateTime = new DateTime(2026, 4, 15, 14, 0, 0)
};

var request = new ExifTimezoneCorrectionRequest
{
    RecordedTimezone = "Etc/GMT-1",      // Camera locked to UTC+1
    CorrectTimezone = "Europe/Amsterdam"  // Actually in Amsterdam
};

// Validate
var validation = service.ValidateCorrection(fileIndexItem, request);
if (!string.IsNullOrEmpty(validation.Warning))
{
    Console.WriteLine($"Warning: {validation.Warning}");
}
if (!string.IsNullOrEmpty(validation.Error))
{
    Console.WriteLine($"Error: {validation.Error}");
    return;
}

// Apply
var result = await service.CorrectTimezoneAsync(fileIndexItem, request);

Console.WriteLine($"Success: {result.Success}");
Console.WriteLine($"Original: {result.OriginalDateTime:yyyy-MM-dd HH:mm:ss}");
Console.WriteLine($"Corrected: {result.CorrectedDateTime:yyyy-MM-dd HH:mm:ss}");
Console.WriteLine($"Delta: {result.DeltaHours:+0.00;-0.00;0.00}h");
```

### Example 2: Batch Images

```csharp
var fileIndexItems = new List<FileIndexItem>
{
    new FileIndexItem { FilePath = "/photos/img1.jpg", DateTime = new DateTime(2026, 4, 15, 14, 0, 0) },
    new FileIndexItem { FilePath = "/photos/img2.jpg", DateTime = new DateTime(2026, 4, 16, 09, 30, 0) },
    new FileIndexItem { FilePath = "/photos/img3.jpg", DateTime = new DateTime(2026, 4, 17, 16, 45, 0) }
};

var request = new ExifTimezoneCorrectionRequest
{
    RecordedTimezone = "Etc/GMT-1",
    CorrectTimezone = "Europe/Amsterdam"
};

var results = await service.CorrectTimezoneAsync(fileIndexItems, request);

foreach (var result in results)
{
    if (result.Success)
    {
        Console.WriteLine($"✓ {result.OriginalDateTime:MM-dd HH:mm} → {result.CorrectedDateTime:MM-dd HH:mm}");
    }
    else
    {
        Console.WriteLine($"✗ Error: {result.Error}");
    }
}
```

### Example 3: Validation Only (Dry-Run)

```csharp
var results = new List<ExifTimezoneCorrectionResult>();

foreach (var item in fileIndexItems)
{
    var result = service.ValidateCorrection(item, request);
    results.Add(result);
}

// Show warnings and errors without writing
foreach (var result in results)
{
    if (!string.IsNullOrEmpty(result.Warning))
    {
        Console.WriteLine($"⚠ {result.Warning}");
    }
    if (!string.IsNullOrEmpty(result.Error))
    {
        Console.WriteLine($"✗ {result.Error}");
    }
    else
    {
        Console.WriteLine($"✓ Correction looks good");
    }
}

// User can then decide to proceed or cancel
if (userConfirms)
{
    // Apply corrections
    await service.CorrectTimezoneAsync(fileIndexItems, request);
}
```

---

## Common Timezone IDs (IANA Format)

### Europe
- `Europe/Amsterdam` (UTC+1/+2 with DST)
- `Europe/London` (UTC+0/+1 with DST)
- `Europe/Paris` (UTC+1/+2 with DST)
- `Europe/Berlin` (UTC+1/+2 with DST)

### Americas
- `America/New_York` (UTC-5/-4 with DST)
- `America/Los_Angeles` (UTC-8/-7 with DST)
- `America/Toronto` (UTC-5/-4 with DST)
- `America/Mexico_City` (UTC-6/-5 with DST)

### Asia
- `Asia/Tokyo` (UTC+9, no DST)
- `Asia/Shanghai` (UTC+8, no DST)
- `Asia/Bangkok` (UTC+7, no DST)
- `Asia/Dubai` (UTC+4, no DST)

### Fixed UTC Offsets
- `UTC` (UTC+0)
- `Etc/GMT-1` (fixed UTC+1)
- `Etc/GMT-2` (fixed UTC+2)
- `Etc/GMT+1` (fixed UTC-1)
- `Etc/GMT+5` (fixed UTC-5)

Note: `Etc/GMT` offsets are inverted (confusing but correct per POSIX standard)

---

## Testing Scenarios

### Test 1: DST Transition

```csharp
[Test]
public async Task CorrectTimezone_DSTTransition_DifferentDeltas()
{
    var photoBeforeDst = new FileIndexItem
    {
        DateTime = new DateTime(2026, 3, 30, 14, 0, 0),
        FilePath = "/test/before_dst.jpg"
    };

    var photoAfterDst = new FileIndexItem
    {
        DateTime = new DateTime(2026, 3, 31, 14, 0, 0),
        FilePath = "/test/after_dst.jpg"
    };

    var request = new ExifTimezoneCorrectionRequest
    {
        RecordedTimezone = "Etc/GMT-1",
        CorrectTimezone = "Europe/Amsterdam"
    };

    var resultBefore = await service.CorrectTimezoneAsync(photoBeforeDst, request);
    var resultAfter = await service.CorrectTimezoneAsync(photoAfterDst, request);

    // Before DST: delta should be 0 (both UTC+1)
    Assert.AreEqual(0, resultBefore.DeltaHours);
    Assert.AreEqual(new DateTime(2026, 3, 30, 14, 0, 0), resultBefore.CorrectedDateTime);

    // After DST: delta should be +1 (UTC+1 vs UTC+2)
    Assert.AreEqual(1, resultAfter.DeltaHours);
    Assert.AreEqual(new DateTime(2026, 3, 31, 15, 0, 0), resultAfter.CorrectedDateTime);
}
```

### Test 2: Day Rollover

```csharp
[Test]
public async Task CorrectTimezone_DayRollover_WarnsAboutDateChange()
{
    var photo = new FileIndexItem
    {
        DateTime = new DateTime(2026, 4, 15, 23, 30, 0),
        FilePath = "/test/evening.jpg"
    };

    var request = new ExifTimezoneCorrectionRequest
    {
        RecordedTimezone = "Etc/GMT-5",     // UTC-5
        CorrectTimezone = "Asia/Tokyo"      // UTC+9
    };

    var result = service.ValidateCorrection(photo, request);

    Assert.IsTrue(!string.IsNullOrEmpty(result.Warning));
    Assert.IsTrue(result.Warning.Contains("2026-04-16")); // Day changed
}
```

### Test 3: Invalid Timezone

```csharp
[Test]
public void ValidateCorrection_InvalidTimezone_ReturnsError()
{
    var photo = new FileIndexItem
    {
        DateTime = new DateTime(2026, 4, 15, 14, 0, 0),
        FilePath = "/test/photo.jpg"
    };

    var request = new ExifTimezoneCorrectionRequest
    {
        RecordedTimezone = "Invalid/Timezone",
        CorrectTimezone = "Europe/Amsterdam"
    };

    var result = service.ValidateCorrection(photo, request);

    Assert.IsFalse(result.Success);
    Assert.IsTrue(!string.IsNullOrEmpty(result.Error));
}
```

---

## Future Enhancements

### 1. Preserve SubSecTime (Fractional Seconds)

Current: Updates only to second precision
Enhancement: Preserve milliseconds/microseconds:

```csharp
// Read subsecond info
var subSecTimeOriginal = ReadExifSubSecTime(fileIndexItem);

// Apply to corrected datetime
if (!string.IsNullOrEmpty(subSecTimeOriginal))
{
    var subseconds = ParseSubseconds(subSecTimeOriginal);
    correctedDateTime = correctedDateTime.AddMilliseconds(subseconds);
}

// Write back with SubSecTime tags
command += $" -SubSecTimeOriginal=\"{subseconds}\"";
```

### 2. Write OffsetTime Fields

Current: Only updates DateTime
Enhancement: Write OffsetTimeOriginal, OffsetTimeDigitized, OffsetTime:

```csharp
var offsetTimeString = correctTz.GetUtcOffset(correctedDateTime)
    .ToString("\\+hh\\:mm");

command += $" -OffsetTimeOriginal=\"{offsetTimeString}\" ";
command += $" -OffsetTimeDigitized=\"{offsetTimeString}\" ";
command += $" -OffsetTime=\"{offsetTimeString}\" ";
```

### 3. API Endpoint

```csharp
[HttpPost("api/exif/correct-timezone")]
public async Task<IActionResult> CorrectTimezone(
    [FromBody] ExifTimezoneCorrectionRequest request)
{
    var fileIndexItems = await LoadFileIndexItems(request.FilePaths);
    var results = await _service.CorrectTimezoneAsync(fileIndexItems, request);
    return Ok(results);
}
```

### 4. CLI Command

```csharp
// In starskyimportercli or dedicated timezone CLI
public class TimezoneCommand
{
    public void Execute(
        string[] filePaths,
        string recordedTimezone,
        string correctTimezone,
        bool dryRun = false)
    {
        var items = LoadFileIndexItems(filePaths);
        var request = new ExifTimezoneCorrectionRequest
        {
            RecordedTimezone = recordedTimezone,
            CorrectTimezone = correctTimezone
        };

        if (dryRun)
        {
            // Validate only
            foreach (var item in items)
            {
                var result = service.ValidateCorrection(item, request);
                PrintResult(result);
            }
        }
        else
        {
            // Apply corrections
            var results = service.CorrectTimezoneAsync(items, request).Result;
            foreach (var result in results)
            {
                PrintResult(result);
            }
        }
    }
}
```

---

## Summary

The EXIF Timezone Correction feature provides:

✓ **DST-Aware Calculation** - Correct offset for each photo date
✓ **Batch Processing** - Multiple photos at once
✓ **Validation** - Dry-run mode to preview changes
✓ **Warnings** - Alert about date rollovers and same timezone
✓ **Simple API** - Easy to integrate into controllers/CLI
✓ **Robust Error Handling** - Clear messages for failures
✓ **ExifTool Integration** - Uses existing metadata writing infrastructure

