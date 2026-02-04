# Modal Timezone Shift Component

The `modal-timezone-shift` is a React component for the Starsky application that enables users to shift the timezone or offset of date/time metadata for one or more selected files (such as images). It provides a modal dialog where users can specify the desired correction, and then applies the changes to all selected files via an API call. This component integrates tightly with Starsky's archive context and file selection system, ensuring a seamless batch operation experience.

## Features

- Shift date/time metadata for multiple files at once
- Choose between timezone-based or offset-based corrections
- See warnings and errors for each file after the operation
- Cleans up caches and updates the UI on success

## How to Use

1. **Import the component:**
  ```tsx
  import ModalTimezoneShift from 'components/organisms/modal-timezone-shift/modal-timezone-shift';
  ```

2. **Provide the required props** (typically from a parent archive or detail view container):
  - `isOpen: boolean` — Whether the modal is open
  - `onClose: () => void` — Callback to close the modal
  - `selectedFiles: string[]` — Array of selected file paths
  - `archiveState: IArchiveProps` — The current archive state (file list, etc)
  - `dispatch: React.Dispatch<ArchiveAction>` — Dispatch function for archive context updates

3. **Render the component:**
  ```tsx
  <ModalTimezoneShift
    isOpen={isModalOpen}
    onClose={handleClose}
    selectedFiles={selectedFilePaths}
    archiveState={archiveState}
    dispatch={archiveDispatch}
  />
  ```

4. **User interaction:**
  - The user opens the modal and selects either a timezone correction or a manual offset.
  - On submit, the component calls the `executeShift` function, which:
    - Sends a POST request to the backend API with the selected files and correction data.
    - Updates the archive state with the new file metadata.
    - Cleans relevant caches and closes the modal on success.
    - Displays any errors or warnings per file in the UI.

5. **Error handling:**
  - If the API returns errors or warnings for specific files, these are displayed in the modal.
  - If the operation fails, a general error message is shown.

## Related Files

- `modal-timezone-shift.tsx` — Main component implementation
- `internal/execute-shift.ts` — Handles the API call and state updates
- `internal/preview-error-files.tsx` — Displays per-file errors/warnings after the operation

## Best Practices

- Always provide the correct `archiveState` and `dispatch` from the archive context
- Use the modal for batch operations on multiple files to ensure consistency
- Handle the `onClose` callback to reset any local state in the parent component if needed

## See Also

- Archive context and file selection documentation
- API documentation for `/meta-time-correct/timezone-execute` and `/meta-time-correct/offset-execute` endpoints

## See Also
- Archive context and file selection documentation.
- API documentation for `/meta-time-correct/timezone-execute` and `/meta-time-correct/offset-execute` endpoints.

# Timezone Shift Feature

## Overview

The Timezone Shift feature allows users to adjust photo timestamps in two ways:

1. **Correct incorrect camera timezone** - Apply a fixed time offset (years, months, days, hours, minutes, seconds)
2. **I moved to a different place** - Convert timestamps between timezones (DST-aware)

## Components

### 1. ModalTimezoneShift

**Location:** `src/components/organisms/modal-timezone-shift/`

Main modal component that handles the timezone shift workflow with multiple steps.

**Features:**

- Mode selection screen
- Fixed offset input with preview
- Timezone conversion with DST awareness
- Preview before execution
- Batch processing for multiple files

**Props:**

```typescript
interface IModalTimezoneShiftProps {
  isOpen: boolean; // Control modal visibility
  handleExit: () => void; // Callback when modal closes
  select: string[]; // Array of file paths to process
  collections?: boolean; // Whether to process collections (default: true)
}
```

**Usage:**

```tsx
<ModalTimezoneShift
  isOpen={true}
  handleExit={() => setIsOpen(false)}
  select={["/photos/img1.jpg", "/photos/img2.jpg"]}
  collections={true}
/>
```

### 2. MenuOptionTimezoneShift

**Location:** `src/components/molecules/menu-option-timezone-shift/`

Menu item that triggers the timezone shift modal.

**Props:**

```typescript
interface IMenuOptionTimezoneShiftProps {
  readOnly: boolean; // Disable if in read-only mode
  select: string[]; // Selected files
  collections?: boolean; // Process collections
}
```

## API Endpoints

### Get Timezones

```
GET /api/meta-time-correct/system-timezones
```

Returns a list of all available timezones:

```json
[
  {
    "id": "Europe/London",
    "displayName": "(UTC+00:00) London tijd"
  },
  {
    "id": "Europe/Amsterdam",
    "displayName": "(UTC+01:00) Amsterdam tijd"
  }
]
```

### Timezone Preview

```
POST /api/meta-time-correct/timezone-preview?f={filePath}&collections={true/false}
Content-Type: application/json

{
  "recordedTimezone": "Europe/Amsterdam",
  "correctTimezone": "Europe/London"
}
```

### Timezone Execute

```
POST /api/meta-time-correct/timezone-execute?f={filePath}&collections={true/false}
Content-Type: application/json

{
  "recordedTimezone": "Europe/Amsterdam",
  "correctTimezone": "Europe/London"
}
```

### Offset Preview

```
POST /api/meta-time-correct/offset-preview?f={filePath}&collections={true/false}
Content-Type: application/json

{
  "year": 0,
  "month": 0,
  "day": 0,
  "hour": 3,
  "minute": 0,
  "second": 0
}
```

### Offset Execute

```
POST /api/meta-time-correct/offset-execute?f={filePath}&collections={true/false}
Content-Type: application/json

{
  "year": 0,
  "month": 0,
  "day": 0,
  "hour": 3,
  "minute": 0,
  "second": 0
}
```

**Response format (all endpoints):**

```json
[
  {
    "success": true,
    "originalDateTime": "2024-08-12T14:32:00",
    "correctedDateTime": "2024-08-12T17:32:00",
    "delta": "+03:00:00",
    "warning": "",
    "error": "",
    "fileIndexItem": { ... }
  }
]
```

## Interfaces

### ITimezone

```typescript
export interface ITimezone {
  id: string; // Timezone ID (e.g., "Europe/London")
  displayName: string; // Display name (e.g., "(UTC+00:00) London tijd")
}
```

### ITimezoneShiftRequest

```typescript
export interface ITimezoneShiftRequest {
  recordedTimezone: string;
  correctTimezone: string;
}
```

### IOffsetShiftRequest

```typescript
export interface IOffsetShiftRequest {
  year: number;
  month: number;
  day: number;
  hour: number;
  minute: number;
  second: number;
}
```

### ITimezoneShiftResult

```typescript
export interface ITimezoneShiftResult {
  success: boolean;
  originalDateTime: string;
  correctedDateTime: string;
  delta: string;
  warning: string;
  error: string;
  fileIndexItem: any;
}
```

## User Flow

1. **Mode Selection**
   - User selects files in archive view
   - Clicks "Shift photo timestamps" from menu
   - Modal opens with two mode options

2. **Mode A: Fixed Offset**
   - User enters offset values (years, months, days, hours, minutes, seconds)
   - Clicks "Generate Preview" to see a sample result
   - Reviews preview showing original and corrected timestamps
   - Clicks "Apply Shift" to execute

3. **Mode B: Timezone Conversion**
   - User selects original timezone from dropdown
   - User selects target timezone from dropdown
   - Clicks "Generate Preview" to see a sample result
   - Reviews preview showing DST-aware conversion
   - Clicks "Apply Shift" to execute

## Preview Strategy

The preview feature uses a **representative sample** approach:

- Only the first selected file is used for preview generation
- This prevents unnecessary server load when processing many files
- The preview shows what will happen to all files with similar characteristics

## Navigation

Users can navigate back and forth between modes without losing input:

- "Back" button returns to mode selection
- State is preserved when switching modes
- Preview is cleared when changing modes

## Styling

The modal uses a clean, step-by-step interface:

- Clear headers for each mode
- Radio buttons for mode selection
- Number inputs for offset values
- Dropdowns for timezone selection
- Preview section with clear before/after display
- Error and warning messages

## Testing

Run tests with:

```bash
npm test modal-timezone-shift
```

## Future Enhancements

- **Rename Files**: Optional step to rename files based on new timestamps
- **Batch operation progress**: Show progress bar for large selections
- **Undo functionality**: Allow users to revert changes
- **History**: Keep track of recently used offset values

## Renaming Filenames After Timestamp Shift

After applying a timezone or offset shift, the modal provides an optional step to rename the affected files based on their new timestamps. This helps keep filenames consistent with the updated metadata.

- **How it works:**
  - After a successful shift, a checkbox labeled "Rename files after shifting timestamps" appears.
  - If checked, a preview list shows the original and new filenames for each file.
  - The user can review and confirm the renaming operation before applying it.
  - Any errors or filename conflicts are displayed in the preview.
  - The renaming step is optional; users can skip it by leaving the checkbox unchecked.

- **UI Details:**
  - The preview uses Material-style checkboxes and a clear mapping from old to new filenames.
  - Errors are highlighted, and the user is prevented from proceeding if there are unresolved issues.

- **API Integration:**
  - The component calls the batch rename API endpoint with the selected files and new names.
  - On success, the archive state is updated and the modal closes.

This feature ensures that filenames remain meaningful and consistent with the new date/time metadata, improving organization and searchability.
