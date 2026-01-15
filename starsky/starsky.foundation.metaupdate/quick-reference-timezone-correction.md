# EXIF Timezone Correction - Quick Reference

## TL;DR

Feature to correct EXIF timestamps when camera timezone was wrong. Rewrites DateTimeOriginal, DateTimeDigitized, and DateTime fields.

## Files

```
starsky.foundation.metaupdate/
├── Models/
│   └── ExifTimezoneCorrection.cs         # Request/Result models
├── Services/
│   └── ExifTimezoneCorrectionService.cs  # Main service implementation
└── readme-timezone-correction.md         # Full documentation

starskytest/starsky.foundation.metaupdate/
└── Services/
    └── ExifTimezoneCorrectionServiceTest.cs  # 12 comprehensive tests
```

## Quick Usage

```csharp
// 1. Create service (DI)
var service = new ExifTimezoneCorrectionService(readMeta, exifToolCmdHelper, logger);

// 2. Prepare request
var request = new ExifTimezoneCorrectionRequest
{
    RecordedTimezone = "UTC",           // What camera thought
    CorrectTimezone = "Europe/Amsterdam" // Where photo was actually taken
};

// 3. Validate (optional but recommended)
var validation = service.ValidateCorrection(fileIndexItem, request);
if (!string.IsNullOrEmpty(validation.Error)) return;
if (!string.IsNullOrEmpty(validation.Warning)) /* prompt user */;

// 4. Correct
var result = await service.CorrectTimezoneAsync(fileIndexItem, request);

// 5. Check result
if (result.Success)
    Console.WriteLine($"Corrected: {result.OriginalDateTime} → {result.CorrectedDateTime} (Δ {result.DeltaHours}h)");
else
    Console.WriteLine($"Error: {result.Error}");
```

## Algorithm

```
delta = TimeZoneInfo.GetUtcOffset(correctTz, date) - TimeZoneInfo.GetUtcOffset(recordedTz, date)
correctedDateTime = originalDateTime + delta
```

**DST-aware**: Offsets calculated for the specific date, handles summer/winter time automatically.

## Example

```
Photo taken: 14:30 in Amsterdam (GMT+02)
Camera set to: UTC (GMT+00)
EXIF stores: 2024-06-15 14:30:00  ❌

Correction:
  recordedOffset = +00:00
  correctOffset  = +02:00
  delta          = +02:00
  
Result: 2024-06-15 16:30:00  ✅
```

## Existing Functions Used

| Function | Purpose |
|----------|---------|
| `ExifToolCmdHelper.UpdateAsync()` | Write EXIF metadata |
| `ExifToolCmdHelper.UpdateDateTimeCommand()` | Generate `-AllDates` command |
| `ReadMetaExif.GetExifDateTime()` | Read current datetime |
| `TimeZoneInfo.GetUtcOffset()` | Get DST-aware offset |

## EXIF Fields Updated

Via `-AllDates="YYYY:MM:DD HH:MM:SS"`:
- `DateTimeOriginal` (tag 36867)
- `DateTimeDigitized` (tag 36868)  
- `DateTime` (tag 306)
- `xmp:datecreated`

## Edge Cases Handled

✅ DST (summer/winter time)  
✅ Day/month/year rollover  
✅ Negative offsets (subtract time)  
✅ Invalid timezones  
✅ Missing datetime  
✅ Same timezone (delta=0)

## Common Timezones

```
UTC                    → "UTC"
Netherlands (summer)   → "Europe/Amsterdam"  (GMT+02)
Netherlands (winter)   → "Europe/Amsterdam"  (GMT+01)
UK (summer)           → "Europe/London"     (GMT+01)
US East (summer)      → "America/New_York"  (GMT-04)
US West (summer)      → "America/Los_Angeles" (GMT-07)
Japan                 → "Asia/Tokyo"        (GMT+09)
New Zealand (summer)  → "Pacific/Auckland"  (GMT+13)
```

## Tests

```bash
# Run all tests
dotnet test --filter "ExifTimezoneCorrectionServiceTest"

# Run specific test
dotnet test --filter "CorrectTimezoneAsync_ValidCorrection_ShouldSucceed"
```

**Coverage**: 12 tests covering validation, correction, DST, rollover, errors.

## DI Registration

```csharp
services.AddScoped<IExifTimezoneCorrectionService>(provider =>
{
    var readMeta = provider.GetRequiredService<IReadMeta>();
    var exifTool = provider.GetRequiredService<IExifTool>();
    var storage = provider.GetRequiredService<IStorage>();
    var thumbnailStorage = provider.GetRequiredService<IStorage>();
    var thumbnailQuery = provider.GetRequiredService<IThumbnailQuery>();
    var logger = provider.GetRequiredService<IWebLogger>();
    
    var exifToolCmdHelper = new ExifToolCmdHelper(
        exifTool, storage, thumbnailStorage, readMeta, thumbnailQuery, logger);
    
    return new ExifTimezoneCorrectionService(readMeta, exifToolCmdHelper, logger);
});
```

## API Example (Future)

```csharp
[HttpPost("/api/metadata/correct-timezone")]
public async Task<ActionResult<ExifTimezoneCorrectionResult>> CorrectTimezone(
    [FromBody] CorrectTimezoneRequest request)
{
    var fileIndexItem = await _query.GetObjectByFilePathAsync(request.FilePath);
    
    var correctionRequest = new ExifTimezoneCorrectionRequest
    {
        RecordedTimezone = request.RecordedTimezone,
        CorrectTimezone = request.CorrectTimezone
    };
    
    var result = await _service.CorrectTimezoneAsync(fileIndexItem, correctionRequest);
    
    return result.Success ? Ok(result) : BadRequest(result);
}
```

## ⚠️ Important

- **Irreversible**: EXIF is permanently modified
- **Backup**: Always backup before batch operations
- **Validation**: Always validate before applying
- **Preview**: Show before/after to user
- **Naive time**: Result is still naive local time (no offset stored)

## Dependencies

- ✅ No new packages required
- ✅ Uses existing ExifTool integration
- ✅ Uses .NET built-in TimeZoneInfo
- ✅ Integrates with existing EXIF read/write

## What's NOT Implemented (Yet)

- ❌ OffsetTimeOriginal/Digitized/Time fields
- ❌ UI integration
- ❌ Undo functionality
- ❌ CLI tool
- ❌ GPS-based timezone detection

## Next Steps

1. Review `readme-timezone-correction.md` for full details
2. Review `implementation-summary-timezone-correction.md` for integration guide
3. Check tests in `ExifTimezoneCorrectionServiceTest.cs` for examples
4. Test with sample images before deploying
5. Add to DI container
6. Create API endpoint (optional)
7. Build UI (optional)

## Support

Files to check for implementation details:
- Service: `ExifTimezoneCorrectionService.cs`
- Tests: `ExifTimezoneCorrectionServiceTest.cs`
- Docs: `readme-timezone-correction.md`
- Summary: `implementation-summary-timezone-correction.md`

