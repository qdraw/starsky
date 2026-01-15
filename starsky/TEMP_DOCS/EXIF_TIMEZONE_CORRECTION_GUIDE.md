# EXIF Timezone Correction Feature - User & Implementation Guide

## Overview

This feature corrects EXIF timestamps on images that were recorded with the wrong timezone setting. Instead of manually adjusting each image, you can batch-correct multiple images by specifying the timezone the camera thought it was in and the actual timezone where the photos were taken.

---

## Understanding the Feature: The Core Concept

### What Problem Does It Solve?

**Scenario**: Your camera's timezone was set incorrectly. All photos taken during that period have timestamps that don't match the actual moment they were captured.

**Examples:**
- Camera was set to UTC+0 but you were in UTC+2 → all photos are 2 hours behind
- Camera was still in winter time (UTC+1) but DST started and you're now in UTC+2 → all photos are 1 hour behind  
- You traveled to a new country and forgot to update your camera's timezone → all photos from that country have wrong times

**Solution**: This feature calculates the difference between what the camera thought and what it should have been, then shifts all EXIF timestamps accordingly.

---

## Key Concepts: Recorded vs Correct Timezone

### Recorded Timezone
**Definition**: The timezone the camera thought it was in when it recorded the timestamps.

**How to determine it**:
1. Check your camera settings when you took the photos
2. If you can't remember, look for any photo from that period with a known correct time
3. Calculate backwards: If a photo timestamp says 14:00 and you know the actual time was 15:00, the camera was 1 hour behind

### Correct Timezone
**Definition**: The actual timezone where the photos were taken.

**How to determine it**:
1. Where were you geographically when you took the photos?
2. Use that location's timezone (consider DST for that date!)
3. You can specify IANA timezone IDs like: `Europe/Amsterdam`, `America/New_York`, `Asia/Tokyo`, `UTC`

---

## DST (Daylight Saving Time) Handling - Why It Matters

### The Problem with Fixed Offsets
❌ **WRONG**: "My camera is always +1 hour behind, so I'll add 1 hour to all photos"

Why? Daylight Saving Time changes the offset mid-year:
- Winter (UTC+1): offset is +1
- Summer (UTC+2): offset is +2
- The change happens on different dates in different regions

### The Solution: This Feature Is DST-Aware
✅ **RIGHT**: "My camera was set to Europe/Amsterdam timezone. My photos are actually in Europe/Paris timezone. The system will automatically calculate the correct offset for each photo's date."

The feature uses the actual date of each photo to determine the correct offset, accounting for DST transitions.

---

## Example Scenarios

### Scenario 1: Camera Forgot to Adjust for DST

**Situation:**
- Before DST: Camera timezone was set to `Europe/Amsterdam` (UTC+1)
- DST started March 31, 2026: Europe/Amsterdam shifted to UTC+2
- **Problem**: You forgot to update the camera. All photos after March 31 are 1 hour behind.
- Photos taken April 15 show 14:00 but were actually taken at 15:00

**How to Correct:**
- **Recorded Timezone**: `Europe/Amsterdam` (what the camera still thought it was in)
- **Correct Timezone**: `Europe/Amsterdam` (the actual timezone)
- **What Happens**:
  1. System sees photo from April 15
  2. Looks up Europe/Amsterdam offset on April 15 → UTC+2 (DST active)
  3. Looks up Europe/Amsterdam offset on April 15 → UTC+2 (same, so delta=0... wait, that's wrong!)

**Actually, let me reconsider...**

The real issue: **Your camera clock was still set to UTC+1, not the system timezone.**

**Correct Approach:**
- **Recorded Timezone**: `UTC` or `Europe/London` (fixed UTC+0/+1, no DST)
- **Correct Timezone**: `Europe/Amsterdam` (UTC+2 after DST)
- **What Happens**:
  1. Photo from April 15 shows 14:00
  2. Recorded offset (UTC): UTC+0
  3. Correct offset (Europe/Amsterdam on April 15): UTC+2
  4. Delta = (+2) - (+0) = +2 hours
  5. Corrected time: 14:00 + 2:00 = 16:00... still not 15:00!

**The Real Scenario**: The camera manual was set to UTC+1 consistently.
- **Recorded Timezone**: `UTC` (to simulate fixed UTC+1, use UTC as base then add logic... wait, that's confusing)

Let me clarify this properly:

### Scenario 1 (Corrected): Camera Forgot DST Update

**Real situation:**
- You were in Europe/Amsterdam on March 31, 2026 (DST transition day)
- Before DST: the local time was UTC+1, you took a photo at 14:00 local time
- After DST: the local time was UTC+2, you took a photo at 14:00 local time
- **Problem**: Your camera never adjusted for DST
  - Photo 1 (2:00 AM local): shows 01:59 (because it's stored as if UTC+0)
  - Photo 2 (3:00 AM local): shows 02:59 (because it's still pretending UTC+0/+1)
  - Actually, let's use simpler times...

**Simple version:**
- Camera was set to a fixed offset: **UTC+1** (like a manual timezone, not a named timezone that handles DST)
- You're in **Europe/Amsterdam** where DST applies
- On March 31: Europe/Amsterdam transitioned from UTC+1 to UTC+2
- Photos taken after 3:00 AM on March 31 are wrong

**How to fix:**
- **Recorded Timezone**: `UTC` (or `Etc/GMT-1` for fixed UTC+1)
- **Correct Timezone**: `Europe/Amsterdam`
- For photo taken at 14:00 on March 31:
  - Recorded offset (UTC): 0
  - Correct offset (Europe/Amsterdam on March 31): Already UTC+2 by 14:00
  - Delta = 2 - 0 = +2 hours
  - Corrected: 14:00 + 2:00 = 16:00 ✓ (correct!)
- For photo taken at 14:00 on March 30:
  - Recorded offset (UTC): 0
  - Correct offset (Europe/Amsterdam on March 30): UTC+1
  - Delta = 1 - 0 = +1 hour
  - Corrected: 14:00 + 1:00 = 15:00 ✓ (correct!)

---

### Scenario 2: Traveled Without Updating Camera

**Situation:**
- Your camera is set to `America/New_York` (your home timezone)
- You traveled to `Asia/Tokyo` and forgot to update the camera
- All photos show New York time when they should show Tokyo time
- Photo timestamp: 2026-04-15 14:00 (actually taken at 2026-04-16 02:00 Tokyo time)

**How to Correct:**
- **Recorded Timezone**: `America/New_York` (what camera still says)
- **Correct Timezone**: `Asia/Tokyo` (where you actually were)
- On 2026-04-15:
  - New York offset: UTC-4 (EDT after DST starts in March)
  - Tokyo offset: UTC+9
  - Delta = 9 - (-4) = +13 hours
  - Corrected: 14:00 + 13:00 = 27:00 → **2026-04-16 03:00** ✓

---

### Scenario 3: Wrong Timezone Set Manually

**Situation:**
- Your camera timezone was manually set to `UTC+2` (some cameras allow this)
- You were actually in `Europe/Paris` (which is UTC+1 in winter, UTC+2 in summer)
- You took photos on 2026-01-15 (winter, UTC+1)
- All photos show 14:00 but should show 13:00

**How to Correct:**
- **Recorded Timezone**: `UTC` (or any fixed UTC+2 like `Etc/GMT-2`)
- **Correct Timezone**: `Europe/Paris`
- On 2026-01-15:
  - Recorded offset (fixed UTC+2): +2
  - Correct offset (Europe/Paris, winter): +1
  - Delta = 1 - 2 = **-1 hour** (subtract!)
  - Corrected: 14:00 - 1:00 = 13:00 ✓

---

## Implementation Requirements

This feature should support the following in the implementation:

### Input Validation
- ✓ Timezones must be valid IANA timezone IDs (e.g., `Europe/Amsterdam`)
- ✓ Image must have a valid EXIF DateTime
- ✓ File must exist and be writable
- ✓ Warn if recorded and correct timezones are the same (no correction needed)
- ✓ Warn if correction causes a day/month/year rollover

### EXIF Fields to Update
The implementation must rewrite all of these if present:
- `DateTimeOriginal`
- `DateTimeDigitized`
- `DateTime`
- `SubSecTime`, `SubSecTimeOriginal`, `SubSecTimeDigitized` (preserve fractional seconds)
- `OffsetTimeOriginal`
- `OffsetTimeDigitized`
- `OffsetTime` (or create them to document the correct offset)

### DST Handling
- ✓ Calculate offsets based on the actual date of each photo
- ✓ Not fixed offsets (UTC+1 is not the same as Europe/Amsterdam)
- ✓ Handle DST transitions (March/October in Europe)

### Batch Operations
- ✓ Support correcting multiple images at once
- ✓ Return results for each image (success/failure)
- ✓ Log all operations for audit trail

### Safety
- ✓ Provide validation mode (check corrections without writing)
- ✓ Clear error messages if something fails
- ✓ Warn about irreversible changes

---

## API/CLI Usage Example

### Using the Service (C#)
```csharp
var service = new ExifTimezoneCorrectionService(...);

var request = new ExifTimezoneCorrectionRequest
{
    RecordedTimezone = "UTC",           // Camera thought it was UTC+0
    CorrectTimezone = "Europe/Amsterdam"  // Actually in Amsterdam
};

// Validate first (dry-run)
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

// Apply correction
var result = await service.CorrectTimezoneAsync(fileIndexItem, request);
if (result.Success)
{
    Console.WriteLine($"Corrected: {result.OriginalDateTime:yyyy-MM-dd HH:mm:ss} → " +
                      $"{result.CorrectedDateTime:yyyy-MM-dd HH:mm:ss} (Δ {result.DeltaHours:+0.00;-0.00;0.00}h)");
}
```

### Via REST API (Proposed)
```http
POST /api/exif/correct-timezone

{
    "filePaths": ["/photos/2026-03-31-14-00.jpg"],
    "recordedTimezone": "UTC",
    "correctTimezone": "Europe/Amsterdam"
}

Response:
{
    "results": [
        {
            "filePath": "/photos/2026-03-31-14-00.jpg",
            "success": true,
            "originalDateTime": "2026-03-31T14:00:00Z",
            "correctedDateTime": "2026-03-31T16:00:00Z",
            "deltaHours": 2.0,
            "warning": null,
            "error": null
        }
    ]
}
```

---

## The Algorithm Explained

### Step 1: Parse Input
- Get the image's DateTime from EXIF (naive local time)
- Get RecordedTimezone and CorrectTimezone from user input

### Step 2: Calculate Offsets (DST-Aware)
```
recordedOffset = TimeZoneInfo.GetUtcOffset(RecordedTimezone, DateTime)
correctOffset = TimeZoneInfo.GetUtcOffset(CorrectTimezone, DateTime)
```

Example for 2026-04-15:
- `recordedOffset` for `UTC` = 0 hours
- `correctOffset` for `Europe/Amsterdam` = 2 hours (DST active)

### Step 3: Calculate Delta
```
delta = correctOffset - recordedOffset
```

For the example: `delta = 2 - 0 = 2 hours`

### Step 4: Apply Delta
```
correctedDateTime = originalDateTime + delta
```

For the example: `14:00 + 2h = 16:00`

### Step 5: Write Back to EXIF
- Update DateTimeOriginal, DateTimeDigitized, DateTime with corrected value
- Update OffsetTime* fields to reflect the new timezone offset
- Preserve SubSecTime fractional seconds

---

## Why This Works: The EXIF Model

EXIF stores datetimes as **naive local time** with optional **OffsetTime** fields.

- `DateTime` in EXIF = "2026-04-15 14:00:00" (local time, no timezone info)
- `OffsetTime` in EXIF = "+02:00" (optional, tells us the timezone offset at capture)

**The Correction Strategy:**
1. EXIF time is naive, so we can treat it in any timezone context
2. We know what timezone the camera thought it was in (recordedOffset)
3. We know what timezone it actually should be (correctOffset)
4. The **real world moment** = EXIF_time + recordedOffset (convert to UTC)
5. We want: EXIF_time + correctOffset = real_time
6. Therefore: newEXIF_time = EXIF_time + (correctOffset - recordedOffset)

---

## Edge Cases Handled

### Day Rollover
If adding the delta crosses midnight, the date changes:
- Photo taken 2026-04-15 23:00, delta +2h → 2026-04-16 01:00 ✓

### Month/Year Rollover
If adding the delta crosses month or year boundary:
- Photo taken 2026-12-31 22:00, delta +4h → 2027-01-01 02:00 ✓

### DST Transitions
The correct offset is calculated for each photo's actual date:
- March 30 in Europe: offset = UTC+1
- March 31 at 14:00 in Europe: offset = UTC+2 (already transitioned)
- Different deltas apply to photos taken before/after DST transition ✓

### Invalid Timezones
Both timezones must be valid IANA timezone IDs:
- ✓ Valid: `Europe/Amsterdam`, `America/New_York`, `UTC`, `Asia/Tokyo`
- ✗ Invalid: `GMT+2`, `CET`, `PST` (use IANA IDs instead)

### No DateTime in Image
If EXIF has no valid DateTime:
- Error: "Image does not have a valid DateTime in EXIF"

### File Already Correct
If the image already has the correct OffsetTime:
- Warning: "Image already appears to be in the correct timezone"

---

## Common Mistakes to Avoid

### ❌ Mistake 1: Using Fixed Offsets Instead of Timezone Names
**Wrong:**
```
recordedTimezone = "+01:00"  // Not how the system works
correctTimezone = "+02:00"
```

**Right:**
```
recordedTimezone = "UTC"  // Or Etc/GMT-1 for fixed offset
correctTimezone = "Europe/Amsterdam"
```

### ❌ Mistake 2: Swapping Recorded and Correct Timezones
**Wrong:**
```
recordedTimezone = "Europe/Amsterdam"  // You're IN Amsterdam
correctTimezone = "UTC"                 // Camera thought it was in UTC
```
This would shift photos in the wrong direction!

**Right:**
```
recordedTimezone = "UTC"                // Camera thought it was UTC
correctTimezone = "Europe/Amsterdam"    // Actually in Amsterdam
```

### ❌ Mistake 3: Forgetting About DST in Europe
**Wrong:**
```
// All photos from March-October
recordedTimezone = "UTC"
correctTimezone = "Europe/Amsterdam"
// Assuming it's always +1... but it's +2 during DST!
```

**Right:**
```
// System automatically calculates the correct offset for each photo's date
// No manual DST calculation needed
```

### ❌ Mistake 4: Not Validating First
**Wrong:**
```
// Directly apply correction without checking warnings
```

**Right:**
```
var validation = service.ValidateCorrection(item, request);
if (!string.IsNullOrEmpty(validation.Warning))
{
    // Show user the warning (e.g., "Date will change from March 31 to April 1")
}
if (!string.IsNullOrEmpty(validation.Error))
{
    // Stop, there's a real problem
}
// Then apply
```

---

## Summary: The Corrected DST Example

**Original User Example (Corrected):**

> My camera was set to UTC+1, and after DST started (UTC+2), I forgot to update the camera. All my photos after DST started are 1 hour behind.

**Correct Implementation:**

1. **Recorded Timezone**: `Etc/GMT-1` (simulates fixed UTC+1 offset, no DST)
   - Or: `UTC` if camera was set to UTC (0 offset)
   - **Not**: `Europe/Amsterdam` (this DOES handle DST and would be wrong)

2. **Correct Timezone**: `Europe/Amsterdam` (the actual location, with DST)

3. **What Happens**:
   - For photos on March 30 (before DST):
     - Recorded: UTC+1
     - Correct: UTC+1
     - Delta = 0 (no change needed) ✓
   
   - For photos on March 31 at 14:00 (after DST):
     - Recorded: UTC+1 (camera still thinks this)
     - Correct: UTC+2 (DST now active)
     - Delta = +1 hour
     - Result: 14:00 → 15:00 ✓

4. **Result**: Photos taken after DST are now correct! ✓

---

## Next Steps

1. Integrate this service into the metadata update API
2. Create UI component for timezone selection
3. Add batch operation support in the controller
4. Create comprehensive tests for all scenarios
5. Document in user guide with timezone examples for common regions

