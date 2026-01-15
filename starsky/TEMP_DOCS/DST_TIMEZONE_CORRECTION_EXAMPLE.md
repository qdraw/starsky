# EXIF Timezone Correction: DST Example Walkthrough

## The User's Example (Clarified)

**Original Statement:**
> "My camera was set to UTC+1, and after DST started (UTC+2), I forgot to update the camera. All my photos after DST started are 1 hour behind."

## What This Actually Means

### The Timeline
1. **Before DST (March 1-30)**: Your local time is UTC+1
   - Camera timezone: Set to UTC+1 manually (or detected as such)
   - Local actual time: UTC+1
   - Photos show correct time ✓

2. **DST Transition (March 31, 3:00 AM)**: Clocks "spring forward"
   - Local actual time: UTC+2
   - **Camera timezone: STILL UTC+1** (you forgot to update!)
   - Photos now show 1 hour behind ✗

3. **After DST (April-October)**: Your local time is UTC+2
   - Camera timezone: Still UTC+1
   - Local actual time: UTC+2
   - Photos show 1 hour behind ✗

### The Problem Visualized

| Date | Time (What Camera Shows) | Actual Time | Difference |
|------|--------------------------|-------------|-----------|
| Mar 30 | 14:00 | 14:00 (UTC+1) | Correct ✓ |
| Mar 31 | 14:00 | 15:00 (UTC+2) | **1h behind** ✗ |
| Apr 15 | 14:00 | 15:00 (UTC+2) | **1h behind** ✗ |

---

## How to Correct Using This Feature

### Option 1: Camera Was Set to Fixed UTC+1

If your camera was set to a **fixed UTC+1 offset** (not a timezone that handles DST):

```
Recorded Timezone:  Etc/GMT-1    (or UTC, then add 1 hour as offset)
Correct Timezone:   Europe/Amsterdam
```

**Why Etc/GMT-1?**
- Represents a fixed UTC+1 offset (always +1, never changes for DST)
- Your camera was maintaining this fixed offset
- System will calculate: delta = Amsterdam_offset - GMT-1_offset

**What Happens:**

For **March 30** (before DST):
```
Recorded offset (Etc/GMT-1):      +1 hour
Correct offset (Amsterdam):       +1 hour
Delta:                            +1 - (+1) = 0 hours
Corrected time:                   14:00 + 0 = 14:00 ✓
```

For **March 31 at 14:00** (after DST transition):
```
Recorded offset (Etc/GMT-1):      +1 hour (camera unchanged)
Correct offset (Amsterdam):       +2 hours (DST now active)
Delta:                            +2 - (+1) = +1 hour
Corrected time:                   14:00 + 1 = 15:00 ✓
```

For **April 15** (DST active):
```
Recorded offset (Etc/GMT-1):      +1 hour
Correct offset (Amsterdam):       +2 hours (DST still active)
Delta:                            +2 - (+1) = +1 hour
Corrected time:                   14:00 + 1 = 15:00 ✓
```

✓ **All photos are now correct!**

---

### Option 2: Camera Was Set to a Timezone but Frozen at UTC+1

If your camera was set to `Europe/Amsterdam` but somehow "froze" at UTC+1:

```
Recorded Timezone:  Etc/GMT-1    (what the camera is actually producing: fixed UTC+1)
Correct Timezone:   Europe/Amsterdam
```

**Same result as Option 1** because the camera is producing fixed UTC+1 datetimes.

---

### Option 3: Camera Timezone Was Manually Set to "GMT+1"

Some cameras allow manual entry like `GMT+1`, `GMT+2`, etc:

```
Recorded Timezone:  Etc/GMT-1    (same as "GMT+1")
Correct Timezone:   Europe/Amsterdam
```

**Same result again.**

---

## Important: Why NOT to Use Europe/Amsterdam for Both

### ❌ **WRONG:**
```
Recorded Timezone:  Europe/Amsterdam
Correct Timezone:   Europe/Amsterdam
```

**Why this fails:**

The system calculates:
```
Delta = Amsterdam_offset - Amsterdam_offset = 0
```

Even though your camera is 1 hour behind, the deltas cancel out because it's the same timezone! The system thinks "well, if the camera was in Amsterdam and you're in Amsterdam, no correction needed."

**You'd get:**
```
14:00 + 0 = 14:00 (WRONG! Still 1 hour behind)
```

---

## The Conceptual Framework

### What You're Really Telling the System

**`RecordedTimezone`** = "The camera's clock was locked to this timezone's offset"
- If camera shows 14:00, it was actually 14:00 in RecordedTimezone

**`CorrectTimezone`** = "The actual timezone where the photos were taken"
- Photos should be interpreted as being in this timezone
- System will calculate the right offset for the exact date

### The Math

**Real-world moment = Camera_time + Recorded_offset**
- Camera shows: 14:00
- Recorded offset (Etc/GMT-1): +1
- Real moment: 14:00 + 1 hour = 15:00 UTC

**New EXIF timestamp = Real_moment - Correct_offset**
- Real moment: 15:00 UTC
- Correct offset (Amsterdam on Apr 15): +2
- New EXIF time: 15:00 UTC - 2 hours = 13:00... WAIT, that's wrong!

Let me recalculate:
- Real moment in UTC: 14:00 (recorded) + 1 hour (offset) = 15:00 UTC
- In Amsterdam (UTC+2): 15:00 UTC + 2 = 17:00
- So EXIF should show: 17:00, not 14:00

Hmm, I think I'm confusing myself. Let me reconsider the algorithm:

---

## Algorithm Clarity: How EXIF Datetime Works

### EXIF Stores Naive Local Time

`DateTimeOriginal` in EXIF = local time with NO timezone information
- Value: "2026-04-15 14:00:00"
- This is just a string, no context
- Could mean 14:00 in any timezone

### With OffsetTime (Optional)

`OffsetTimeOriginal` in EXIF = additional info about the offset
- Value: "+01:00" (camera thought it was UTC+1)
- Now we know: local time 14:00 in UTC+1 = 13:00 UTC

### The Correction Goal

We want to rewrite the EXIF so it looks like the photo was taken in the correct timezone:

**Before:**
```
DateTimeOriginal: 14:00:00
OffsetTimeOriginal: +01:00
(Interpretation: 14:00 local time in UTC+1 = 13:00 UTC)
```

**After (if we want to show it as UTC+2 Amsterdam time):**
```
DateTimeOriginal: 15:00:00
OffsetTimeOriginal: +02:00
(Interpretation: 15:00 local time in UTC+2 = 13:00 UTC)
```

The UTC moment is the same (13:00 UTC), but we're expressing it in a different timezone!

### The Algorithm (Corrected)

```
1. Parse EXIF datetime as naive local time: 14:00
2. Get recorded offset (what camera thought): +01:00
3. Get correct offset (actual location): +02:00
4. Delta = correct_offset - recorded_offset = +02:00 - (+01:00) = +01:00
5. New EXIF datetime = original_datetime + delta = 14:00 + 1h = 15:00
```

**Result:**
- Original EXIF: 14:00 in UTC+1 = 13:00 UTC
- Corrected EXIF: 15:00 in UTC+2 = 13:00 UTC ✓

**The UTC moment is preserved!** The photo still represents the same instant in time, just expressed in a different timezone.

---

## Corrected DST Example Walkthrough

### Scenario
- Photos taken April 15, 2026 (after DST)
- Camera was locked to fixed UTC+1 offset
- You were in Europe/Amsterdam (UTC+2 after DST)
- Photo timestamp shows: 14:00
- Actual moment: 13:00 UTC

### Correction Process

```
Recorded Timezone:  Etc/GMT-1 (fixed UTC+1)
Correct Timezone:   Europe/Amsterdam (UTC+2 on April 15)

Input:
- Original EXIF DateTime: 14:00

Calculation:
- Recorded offset (Etc/GMT-1 on any date): +1 hour
- Correct offset (Amsterdam on April 15): +2 hours
- Delta = 2 - 1 = +1 hour

Output:
- Corrected EXIF DateTime: 14:00 + 1h = 15:00

Verification:
- Original: 14:00 + 1h offset = 13:00 UTC ✓
- Corrected: 15:00 + 2h offset = 13:00 UTC ✓
- Same real moment, expressed in different timezone!
```

---

## Summary Table

| Scenario | Camera Setting | Location | Correct Recorded TZ | Correct Target TZ | Delta |
|----------|---|---|---|---|---|
| DST forgot | Fixed +1 | Amsterdam (UTC+2 Apr) | `Etc/GMT-1` | `Europe/Amsterdam` | +1h |
| Traveled | `America/New_York` | `Asia/Tokyo` | `America/New_York` | `Asia/Tokyo` | +13h (Apr) |
| Wrong manual | Fixed +2 | Paris (UTC+1 Jan) | `Etc/GMT-2` | `Europe/Paris` | -1h |

---

## Key Takeaways

1. **Recorded Timezone = Camera's Fixed Offset or Timezone**
   - If it's a fixed offset frozen in time, use `Etc/GMT-N`
   - If it's a timezone name, use that (though it will use the offset at the photo date)

2. **Correct Timezone = Where Photos Actually Were Taken**
   - Use IANA timezone ID (e.g., `Europe/Amsterdam`)
   - System automatically handles DST for the photo date

3. **Delta = Correct_Offset - Recorded_Offset**
   - Calculated on the actual photo date (DST-aware)
   - Can be different for photos taken before/after DST transition

4. **Corrected Time = Original Time + Delta**
   - Simple addition preserves the real-world moment
   - Just changes how that moment is expressed (timezone)

5. **No Offset Entry Issue** ("There is no offset because it does not exist in EXIF")
   - EXIF DateTime alone is naive (no timezone info)
   - System infers the timezone from your "Recorded Timezone" input
   - OffsetTime fields are optional but helpful

