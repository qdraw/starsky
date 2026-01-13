# Batch Rename Modal - UI Structure

## Modal Layout

```
┌─────────────────────────────────────────┐
│          Rename Photos                  │  ← Title
├─────────────────────────────────────────┤
│                                         │
│  Please enter the photo renaming        │  ← Label
│  string:                                │
│                                         │
│  ┌───────────────────────────────────┐ │
│  │ {yyyy}{MM}{dd}_{filenamebase}{se  │ │  ← Pattern Input
│  └───────────────────────────────────┘ │
│                                         │
│  Recent patterns                        │  ← Dropdown Label
│  ┌───────────────────────────────────┐ │
│  │ -- Select a pattern --         ▼  │ │  ← Dropdown
│  │ {yyyy}{MM}{dd}_{filenamebase}    │ │
│  │ IMG_{seqn:3}.{ext}              │ │
│  │ {yyyy}-{MM}-{dd}_{filenamebase} │ │
│  └───────────────────────────────────┘ │
│                                         │
│  13 photos to rename                    │  ← Count Display
│                                         │
│  ┌─────────────────────────────────┐   │
│  │        Preview                  │   │  ← Preview Button
│  └─────────────────────────────────┘   │
│                                         │
│                                         │
│                    [Cancel]             │
│                                         │
└─────────────────────────────────────────┘
```

## After Preview Generation

```
┌─────────────────────────────────────────┐
│          Rename Photos                  │
├─────────────────────────────────────────┤
│                                         │
│  Please enter the photo renaming        │
│  string:                                │
│                                         │
│  ┌───────────────────────────────────┐ │
│  │ {yyyy}{MM}{dd}_{filenamebase}{se  │ │
│  └───────────────────────────────────┘ │
│                                         │
│  Recent patterns                        │
│  ┌───────────────────────────────────┐ │
│  │ -- Select a pattern --         ▼  │ │
│  └───────────────────────────────────┘ │
│                                         │
│  13 photos to rename                    │
│                                         │
│  ┌─────────────────────────────────┐   │
│  │ DSC03746.JPG → 20200621_DSC0374│   │  ← Preview Items
│  │ DSC03747.JPG → 20200621_DSC0374│   │
│  │ ...                             │   │  ← Ellipsis
│  │ DSC03761.JPG → 20200621_114024_│   │
│  └─────────────────────────────────┘   │
│                                         │
│  ⚠️ Errors found:                       │  ← Error Section (if any)
│  DSC03750.JPG: File not found in DB    │
│  DSC03755.JPG: Permission denied       │
│                                         │
│  ┌──────────┐        ┌──────────────┐ │
│  │ Preview  │        │    Rename    │ │  ← Action Buttons
│  └──────────┘        └──────────────┘ │
│                                         │
│  [                  Cancel              │  ← Cancel Button
│                                         │
└─────────────────────────────────────────┘
```

## Menu Integration

```
Archive View
│
├─ [Selection: 13 photos]
│
├─ More ▼
│  │
│  ├─ Undo Selection
│  ├─ Select All
│  ├─ Download
│  ├─ Publish
│  ├─ ► Rename Photos ◄ ← NEW
│  ├─ Move to Trash
│  ├─ Open in Editor
│  └─ Move File
│
└─ ...
```

## Responsive Design

### Desktop (> 600px)
- Modal width: 400px
- Full horizontal layout
- Button group at bottom

### Mobile (< 600px)
- Modal width: 100% (with margins)
- Stacked form elements
- Full-width buttons

### Dark Mode
- Background: #303030
- Text: #fff
- Borders: #555
- Inputs: #424242 background

## State Indicators

### Button States

**Preview Button:**
- ✓ Enabled: Pattern is not empty
- ✗ Disabled: Pattern empty, loading, or already previewed

**Rename Button:**
- ✓ Enabled: Preview generated, no errors
- ✗ Disabled: No preview, has errors, loading

**Cancel Button:**
- Always enabled (unless loading)

### Visual Feedback

- Loading states show "Loading..." text
- Errors have red background (#ff6b6b)
- Warnings have yellow background (#ffc107)
- Success items show blue text for target filename
- Errors show full error message underneath

## Pattern Syntax Reference

| Placeholder | Description | Example |
|------------|-------------|---------|
| `{yyyy}` | 4-digit year | 2024 |
| `{MM}` | 2-digit month | 01 |
| `{dd}` | 2-digit day | 15 |
| `{HH}` | 24-hour format | 14 |
| `{mm}` | Minutes | 30 |
| `{ss}` | Seconds | 45 |
| `{filenamebase}` | Original filename | photo_001 |
| `{ext}` | File extension | jpg |
| `{seqn}` | Sequence number | 1 |
| `{seqn:N}` | Padded sequence | seqn:3 = 001 |

### Example Patterns

- **Date-based:** `{yyyy}{MM}{dd}_{filenamebase}.{ext}`
  - Result: `20240115_photo.jpg`

- **Full timestamp:** `{yyyy}-{MM}-{dd} {HH}{mm}{ss}_{filenamebase}.{ext}`
  - Result: `2024-01-15 14 30 45_photo.jpg`

- **Sequence:** `IMG_{seqn:3}.{ext}`
  - Result: `IMG_001.jpg`, `IMG_002.jpg`, etc.

- **Mixed:** `{yyyy}_{MM}_{filenamebase}_{seqn:2}.{ext}`
  - Result: `2024_01_photo_01.jpg`
