# Batch Rename Feature

## Overview

The batch rename feature allows users to rename multiple photos at once using a pattern-based approach. Users can define a renaming pattern that supports various placeholders for dates, filenames, sequences, and file extensions.

## Components

### 1. ModalBatchRename
**Location:** `src/components/organisms/modal-batch-rename/`

Main modal component that handles the batch rename workflow.

**Features:**
- Pattern input field with support for recent patterns
- Live preview generation before executing the rename
- Error display and validation
- Local storage of recent patterns (up to 10)
- Support for emoji and special characters in patterns

**Props:**
```typescript
interface IModalBatchRenameProps {
  isOpen: boolean;                    // Control modal visibility
  handleExit: () => void;              // Callback when modal closes
  selectedFilePaths: string[];         // Array of file paths to rename
}
```

**Pattern Format Examples:**
- `{yyyy}{MM}{dd}_{filenamebase}{seqn}.{ext}` - Date + filename + sequence
- `{yyyy}-{MM}-{dd} {HH}{mm}{ss}_{filenamebase}.{ext}` - Full timestamp
- `IMG_{seqn:3}.{ext}` - Sequence with padding

**Available Placeholders:**
- `{yyyy}` - Year (4 digits)
- `{MM}` - Month (2 digits)
- `{dd}` - Day (2 digits)
- `{HH}` - Hour (24-hour format)
- `{mm}` - Minute (2 digits)
- `{ss}` - Second (2 digits)
- `{filenamebase}` - Original filename without extension
- `{ext}` - File extension
- `{seqn}` - Sequence number
- `{seqn:N}` - Sequence number with N-digit padding

### 2. MenuOptionBatchRename
**Location:** `src/components/molecules/menu-option-batch-rename/`

Menu option component that appears in the archive menu when files are selected.

**Props:**
```typescript
interface IMenuOptionBatchRenameProps {
  readOnly: boolean;
  state: IArchiveProps;
  selectedFilePaths: string[];
}
```

**Behavior:**
- Only visible when files are selected
- Disabled in read-only mode
- Opens the batch rename modal on click

## Interfaces

### IBatchRenameRequest
```typescript
interface IBatchRenameRequest {
  filePaths: string[];    // Array of file paths to process
  pattern: string;        // Rename pattern string
  collections: boolean;   // Include sidecar files (XMP, etc.)
}
```

### IBatchRenameItem
```typescript
interface IBatchRenameItem {
  sourceFilePath: string;
  targetFilePath: string;
  relatedFilePaths: string[];
  sequenceNumber: number;
  hasError: boolean;
  errorMessage?: string;
}
```

## API Endpoints

### POST /api/BatchRename/preview
Generate a preview of how files will be renamed without making changes.

**Request Body:**
```json
{
  "filePaths": ["string"],
  "pattern": "string",
  "collections": true
}
```

**Response:**
```json
[
  {
    "sourceFilePath": "/test.jpg",
    "targetFilePath": "/2024_01_15_test.jpg",
    "relatedFilePaths": [],
    "sequenceNumber": 0,
    "hasError": false,
    "errorMessage": null
  }
]
```

### POST /api/BatchRename/execute
Execute the batch rename operation.

**Request Body:**
Same as preview endpoint

**Response:**
Array of `IFileIndexItem` with updated file information

## Local Storage

Recent successful rename patterns are stored in localStorage under the key `batch-rename-patterns`. The feature maintains up to 10 most recent patterns, making it easy to reuse common rename patterns.

## Workflow

1. **Select Files** - User selects multiple photos in the archive
2. **Open Modal** - Click "Rename Photos" in the menu
3. **Enter Pattern** - User enters or selects a rename pattern
4. **Generate Preview** - Click "Preview" to see how files will be renamed
5. **Review Preview** - Check the preview for any errors (displayed with error messages)
6. **Execute** - Click "Rename" to apply the changes
7. **Confirmation** - Modal closes and file list updates with new names

## Styling

Batch rename styles are defined in `src/style/css/31-modal-batch-rename.css` and include:
- Input and dropdown styling
- Preview list display
- Error highlighting
- Dark mode support
- Responsive design

## Error Handling

The feature provides detailed error feedback:
- Invalid patterns are caught during preview
- File not found errors are displayed per-file
- Permission errors are reported
- Preview generation errors show a general error message

## Testing

Unit tests are available in:
- `modal-batch-rename.spec.tsx` - Modal component tests
- `menu-option-batch-rename.spec.tsx` - Menu option tests

## Localization

All user-facing strings are localized in `src/localization/localization.json`:
- `MessageBatchRenamePhotos` - Modal title
- `MessageBatchRenameEnterPattern` - Pattern input label
- `MessageBatchRenamePattern` - Pattern field label
- `MessageBatchRenameRecentPatterns` - Recent patterns dropdown label
- `MessageBatchRenamePhotosCount` - File count text
- `MessageBatchRenamePreview` - Preview button text
- `MessageBatchRenameLoadingPreview` - Loading state text
- `MessageBatchRenameError` - Rename button text
- `MessageBatchRenameErrors` - Error section header
- `MessageBatchRenameNoErrors` - No errors found message

## Browser Compatibility

The batch rename feature uses:
- Fetch API for HTTP requests
- LocalStorage for pattern persistence
- Standard DOM APIs for UI interaction

All modern browsers are supported, with fallback error handling for legacy browsers.
