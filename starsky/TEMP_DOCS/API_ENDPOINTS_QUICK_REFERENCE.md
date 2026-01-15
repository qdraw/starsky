# Timezone Correction API Endpoints - Quick Reference

## Two Endpoints

### 1. Preview (Dry-Run) - See What Would Change

```
POST /api/exif/correct-timezone/preview
```

**Purpose:** Show corrections without modifying files

**Request:**

```json
{
  "f": "/photos/img1.jpg;/photos/img2.jpg",
  "request": {
    "recordedTimezone": "Etc/GMT-1",
    "correctTimezone": "Europe/Amsterdam"
  }
}
```

**Response (200):**

```json
{
  "request": {...},
  "fileCount": 2,
  "successCount": 2,
  "errorCount": 0,
  "results": [
    {
      "filePath": "/photos/img1.jpg",
      "success": true,
      "originalDateTime": "2024-06-15T14:00:00",
      "correctedDateTime": "2024-06-15T16:00:00",
      "deltaHours": 2.0,
      "warning": "",
      "error": ""
    },
    ...
  ]
}
```

---

### 2. Execute - Apply Corrections in Background

```
POST /api/exif/correct-timezone/execute
```

**Purpose:** Queue batch correction for background processing

**Request:**

```json
{
  "f": "/photos/img1.jpg;/photos/img2.jpg",
  "request": {
    "recordedTimezone": "Etc/GMT-1",
    "correctTimezone": "Europe/Amsterdam"
  }
}
```

**Response (200):**

```json
{
  "request": {...},
  "fileCount": 2,
  "filePaths": ["/photos/img1.jpg", "/photos/img2.jpg"],
  "status": "Processing",
  "message": "Queued 2 files for timezone correction"
}
```

---

## cURL Examples

### Preview

```bash
curl -X POST "http://localhost:5000/api/exif/correct-timezone/preview" \
  -H "Content-Type: application/json" \
  -d '{
    "recordedTimezone": "Etc/GMT-1",
    "correctTimezone": "Europe/Amsterdam"
  }' \
  -G --data-urlencode 'f=/photos/img1.jpg;/photos/img2.jpg'
```

### Execute

```bash
curl -X POST "http://localhost:5000/api/exif/correct-timezone/execute" \
  -H "Content-Type: application/json" \
  -d '{
    "recordedTimezone": "Etc/GMT-1",
    "correctTimezone": "Europe/Amsterdam"
  }' \
  -G --data-urlencode 'f=/photos/img1.jpg;/photos/img2.jpg'
```

---

## Common Scenarios

### Forgot DST Update

```json
{
  "recordedTimezone": "Etc/GMT-1",
  "correctTimezone": "Europe/Amsterdam"
}
```

### International Travel (NYC → Tokyo)

```json
{
  "recordedTimezone": "America/New_York",
  "correctTimezone": "Asia/Tokyo"
}
```

### Budget Camera (Fixed UTC+0 → Correct Location)

```json
{
  "recordedTimezone": "UTC",
  "correctTimezone": "Europe/Amsterdam"
}
```

---

## HTTP Status Codes

| Code | Meaning                                                  |
|------|----------------------------------------------------------|
| 200  | Success (preview results or execution queued)            |
| 400  | Invalid parameters (missing files, invalid timezone IDs) |
| 401  | Unauthorized (not authenticated)                         |

---

## Workflow

### Recommended Workflow

1. **Call Preview First**
   ```
   POST /api/exif/correct-timezone/preview
   ```
    - Review the corrections that would be applied
    - Check for warnings (day/month rollover)
    - Verify delta hours are correct

2. **Review Results**
    - If successful count < total, investigate errors
    - Check warnings for important changes
    - Verify delta calculations match your expectations

3. **Call Execute**
   ```
   POST /api/exif/correct-timezone/execute
   ```
    - Applies corrections to all files
    - Runs in background
    - Returns immediately with processing status

4. **Monitor Completion**
    - Watch WebSocket notifications
    - Check application logs
    - Verify EXIF was updated with ExifTool

---

## Timezone ID Examples

### Europe

- `Europe/Amsterdam` (UTC+1/+2)
- `Europe/London` (UTC+0/+1)
- `Europe/Paris` (UTC+1/+2)
- `Europe/Berlin` (UTC+1/+2)

### Americas

- `America/New_York` (UTC-5/-4)
- `America/Los_Angeles` (UTC-8/-7)
- `America/Toronto` (UTC-5/-4)

### Asia

- `Asia/Tokyo` (UTC+9)
- `Asia/Shanghai` (UTC+8)
- `Asia/Dubai` (UTC+4)
- `Asia/Kolkata` (UTC+5:30)
- `Asia/Kathmandu` (UTC+5:45)

### Fixed Offsets

- `UTC` (UTC+0)
- `Etc/GMT-1` (UTC+1 fixed)
- `Etc/GMT-2` (UTC+2 fixed)
- `Etc/GMT+5` (UTC-5 fixed)

---

## Error Responses

### Missing File Paths

```json
{
  "error": "No input files"
}
```

### Invalid Timezone

```json
{
  "error": "RecordedTimezone and CorrectTimezone are required"
}
```

### Processing Error

```json
{
  "error": "Error during preview: [details]"
}
```

---

## Response Codes Reference

### Preview Success (200)

- Shows each file's preview
- SuccessCount = files that can be corrected
- ErrorCount = files with errors
- Results contain individual file details

### Execute Success (200)

- Returns "Processing" status
- FileCount = number of files queued
- FilePaths = list of files being processed
- Message = human-readable status

---

## Real-World Example

**Scenario:** Forgot to update camera for DST in Europe

**Step 1: Preview**

```bash
POST /api/exif/correct-timezone/preview
{
  "recordedTimezone": "Etc/GMT-1",
  "correctTimezone": "Europe/Amsterdam"
}
```

**Step 2: Review Response**

```
✓ successCount: 45 files
✓ errorCount: 0 files
✓ Delta: 1.0 hour (correct!)
```

**Step 3: Execute**

```bash
POST /api/exif/correct-timezone/execute
{
  "recordedTimezone": "Etc/GMT-1",
  "correctTimezone": "Europe/Amsterdam"
}
```

**Step 4: Monitor**

- Watch logs for "[TimezoneCorrectionController]" entries
- Receive WebSocket notification when complete
- Verify EXIF with: `exiftool -DateTimeOriginal photo.jpg`

---

**Both endpoints are production-ready and fully tested! 🚀**

