# EXIF Timezone Correction - Quick Reference Card

## 🎯 One-Page Summary

### The Problem
📸 Your camera was set to the wrong timezone. All photos have incorrect timestamps.

**Example:** Camera set to UTC+1, you were in UTC+2. All photos show 1 hour behind.

### The Solution
🔧 Use this feature to tell the system two timezones:
1. **RecordedTimezone** - What the camera thought (e.g., "Etc/GMT-1")
2. **CorrectTimezone** - Where you actually were (e.g., "Europe/Amsterdam")

System recalculates all timestamps automatically.

### The Math
```
delta = CorrectOffset - RecordedOffset
newTime = originalTime + delta
```

---

## 📋 Decision Tree: Which Timezones to Use?

```
START: Camera settings wrong?
├─ YES: Forgotten DST?
│  ├─ YES: Camera was set to fixed offset (e.g., UTC+1)
│  │  └─ RecordedTZ: Etc/GMT-1
│  │     CorrectTZ: Europe/Amsterdam
│  └─ NO: Camera set to named timezone but wrong one
│     └─ RecordedTZ: [what camera showed]
│        CorrectTZ: [where you actually were]
└─ NO: Should not use this feature
```

---

## 🌍 Common Timezone Examples

### For RecordedTimezone (What Camera Was Set To)

| Scenario | Timezone ID |
|----------|------------|
| Fixed UTC+1 offset | `Etc/GMT-1` |
| Fixed UTC+2 offset | `Etc/GMT-2` |
| Fixed UTC offset | `UTC` |
| Was set to Amsterdam | `Europe/Amsterdam` |
| Was set to London | `Europe/London` |
| Was set to New York | `America/New_York` |

### For CorrectTimezone (Where You Were)

| Location | Timezone ID |
|----------|------------|
| Netherlands | `Europe/Amsterdam` |
| UK | `Europe/London` |
| France/Paris | `Europe/Paris` |
| Germany/Berlin | `Europe/Berlin` |
| USA East Coast | `America/New_York` |
| USA West Coast | `America/Los_Angeles` |
| Japan | `Asia/Tokyo` |
| Thailand | `Asia/Bangkok` |
| UAE/Dubai | `Asia/Dubai` |

---

## ⚡ Quick Usage

### Via Code
```csharp
var request = new ExifTimezoneCorrectionRequest
{
    RecordedTimezone = "Etc/GMT-1",
    CorrectTimezone = "Europe/Amsterdam"
};

var result = await service.CorrectTimezoneAsync(fileIndexItem, request);

if (result.Success)
    Console.WriteLine($"Fixed! {result.OriginalDateTime} → {result.CorrectedDateTime}");
```

### Via API (When Available)
```http
POST /api/exif/correct-timezone
Content-Type: application/json

{
    "filePaths": ["/photos/img1.jpg", "/photos/img2.jpg"],
    "recordedTimezone": "Etc/GMT-1",
    "correctTimezone": "Europe/Amsterdam"
}
```

### Via CLI (When Available)
```bash
starsky correct-timezone \
    --recorded "Etc/GMT-1" \
    --correct "Europe/Amsterdam" \
    /photos/img1.jpg /photos/img2.jpg
```

---

## ✅ Validation Checklist

Before applying correction:

- [ ] File exists and is readable
- [ ] Image has EXIF DateTime
- [ ] RecordedTimezone is valid IANA ID
- [ ] CorrectTimezone is valid IANA ID
- [ ] Not same timezone (would do nothing)
- [ ] Ready for date change? (if warning shown)

---

## ⚠️ Common Mistakes

| ❌ Wrong | ✅ Right | Why |
|---------|---------|-----|
| `GMT+2` | `Etc/GMT-2` | System expects IANA IDs |
| `CET` | `Europe/Amsterdam` | Use full timezone, not abbreviation |
| Swap both inputs | Recorded first, then correct | Direction matters! |
| `Europe/Amsterdam` for both | Use correct city names | Same TZ = no correction |
| Ignore date rollover warning | Check if day changes | May affect sorting |

---

## 🔍 Diagnosis: Is My Camera Set Wrong?

### Check #1: Photo Timestamp vs Reality
```
Photo EXIF says:    14:00
Actually taken at:  15:00
─────────────────────────
Difference:         -1 hour behind
→ Camera offset is 1 hour low
```

### Check #2: Compare Multiple Photos

| Photo Date | EXIF Time | Actual Time | Consistent? |
|------------|-----------|------------|------------|
| Mar 30 | 14:00 | 14:00 | ✓ Correct |
| Mar 31 | 14:00 | 15:00 | ✗ 1h behind |
| Apr 15 | 14:00 | 15:00 | ✗ 1h behind |

→ Suggests: Forgot DST update on March 31

### Check #3: Check Camera Settings
- Menu → Settings → Date/Time
- Note the timezone setting
- Check if it matches your actual location

---

## 📊 DST Calendar (Europe 2026)

```
March 2026:
  30 Mon - 23:59 (UTC+1)
  31 Tue - 03:00 → 04:00 (spring forward, now UTC+2)

October 2026:
  25 Sun - 02:59 → 02:00 (fall back, now UTC+1)
```

**If you forgot to update camera on March 31:**
- RecordedTZ: Fixed UTC+1 (e.g., `Etc/GMT-1`)
- CorrectTZ: `Europe/Amsterdam` (automatically UTC+2 after March 31)

---

## 🧮 Example: April 15, 2026, Amsterdam

**Your Input:**
```
RecordedTimezone:  Etc/GMT-1
CorrectTimezone:   Europe/Amsterdam
```

**System Calculates:**
```
Photo date: April 15, 2026

RecordedOffset = Etc/GMT-1 on April 15 = +1h (always, no DST)
CorrectOffset = Europe/Amsterdam on April 15 = +2h (DST active)

Delta = +2h - (+1h) = +1h

Original EXIF: 14:00
Corrected: 14:00 + 1h = 15:00 ✓
```

---

## 🔄 Before & After

### Before Correction
```
EXIF DateTime: 14:00 (ambiguous)
OffsetTime: [missing]
→ What timezone is this 14:00 in? Unknown!
```

### After Correction
```
EXIF DateTime: 15:00 (clear)
OffsetTime: +02:00 (optional, documents timezone)
→ This is 15:00 in UTC+2. Clear!
```

---

## 📞 Getting Help

### "How do I pick timezones?"
→ Use the Decision Tree above

### "What timezone is my city?"
→ Look up IANA timezone database at iana.org/time-zones

### "Will my photos change date?"
→ System warns if day/month changes. Example: 23:59 + 2h = next day

### "Can I undo this?"
→ Yes, correction is reversible if you know the original offset
→ Recommendation: Back up photos first

### "Does it change other metadata?"
→ No, only DateTime fields (DateTimeOriginal, DateTimeDigitized, DateTime)
→ Location, orientation, ISO, etc. unchanged

---

## 🚀 Implementation Status

| Feature | Status |
|---------|--------|
| Core algorithm | ✅ Ready |
| Single image correction | ✅ Ready |
| Batch processing | ✅ Ready |
| DST handling | ✅ Ready |
| Validation | ✅ Ready |
| API endpoint | ⚠️ In progress |
| CLI command | ⚠️ Planned |
| Web UI | ⚠️ Planned |
| Tests | ⚠️ Needed |

---

## 📚 Full Documentation

For detailed information, see:
- **User Guide**: `EXIF_TIMEZONE_CORRECTION_GUIDE.md`
- **DST Explained**: `DST_TIMEZONE_CORRECTION_EXAMPLE.md`
- **Offset Fields**: `OFFSET_MISSING_EXPLANATION.md`
- **Implementation**: `TIMEZONE_CORRECTION_IMPLEMENTATION.md`
- **Status**: `FEATURE_IMPLEMENTATION_SUMMARY.md`

---

## 💡 Pro Tips

1. **Always validate first** - Don't apply correction without preview
2. **Batch similar dates** - Photos from same day have same offset
3. **Check for DST** - Different deltas before/after DST transition
4. **Back up originals** - Just in case you need to revert
5. **Log your inputs** - Remember what timezones you used
6. **Test with one photo** - Verify result before correcting batch
7. **Check file system** - Ensure photos are writable before applying

---

## ⏱️ Time Zone Offset Quick Reference

| Timezone | Winter | Summer | DST? |
|----------|--------|--------|------|
| UTC | 0 | 0 | No |
| Europe/London | -0 | +1 | Yes |
| Europe/Amsterdam | +1 | +2 | Yes |
| Europe/Paris | +1 | +2 | Yes |
| America/New_York | -5 | -4 | Yes |
| America/Los_Angeles | -8 | -7 | Yes |
| Asia/Tokyo | +9 | +9 | No |
| Asia/Bangkok | +7 | +7 | No |

---

## 🎯 When to Use This Feature

| Scenario | Use? | Why |
|----------|------|-----|
| Forgot DST update | ✅ YES | Camera stuck on old offset |
| Traveled to new country | ✅ YES | Camera in wrong timezone |
| Manually set wrong offset | ✅ YES | Need to fix manual error |
| Some photos wrong, some right | ⚠️ MAYBE | Batch all, might affect some |
| Photos have correct time | ❌ NO | Don't change correct photos |
| Don't know timezone | ❌ NO | Need user to provide it |

---

## 📞 Summary: How It Works

1. **You tell system:** My camera was set to `Etc/GMT-1`, I was in `Europe/Amsterdam`
2. **System calculates:** For April 15, that's a +1 hour difference
3. **System updates:** All EXIF DateTimes are shifted +1 hour
4. **Result:** Photos show correct time (15:00 instead of 14:00)
5. **Proof:** Real-world moment (13:00 UTC) expressed correctly in both timezones

**Key insight:** Same moment in time, just expressed in different timezone.

---

**Print this page or bookmark it! 📌**

