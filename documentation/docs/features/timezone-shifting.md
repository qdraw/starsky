# Timezone Shift: Adjust Photo Timestamps Easily

The Timezone Shift feature in Starsky lets you quickly correct the date and time of your photos when your camera was set to the wrong timezone, or when you’ve moved to a different location. This is especially useful for keeping your photo library organized and accurate, no matter where or when your pictures were taken.

## What Can You Do?

- **Batch adjust timestamps** for many photos at once
- **Fix wrong camera timezones** by applying a manual time offset (years, months, days, hours, minutes, seconds)
- **Convert between timezones** (including Daylight Saving Time awareness) if you traveled or moved
- **Preview changes** before applying them, so you know exactly what will happen
- **See warnings or errors** for each file if something can’t be changed

## How It Works

1. **Select your photos** in the archive view.
2. Click the **“Shift photo time”** option in the menu.
3. The Timezone Shift window opens. Choose one of two modes:
    - **Fixed Offset**: Enter how much to shift the time (for example, +3 hours if your camera was set to the wrong timezone).
    - **Timezone Conversion**: Select the original and correct timezone (for example, from “Europe/Amsterdam” to “Europe/London”).
4. Click **“Generate Preview”** to see how the change will affect a sample photo.
5. If you’re happy with the preview, click **“Apply Shift”** to update all selected photos.
6. The app will show you which files were updated, and if there were any issues.

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

## Why Use This Feature?

- **Save time**: Fix hundreds of photos in one go, instead of editing each one manually.
- **Stay organized**: Keep your photo dates accurate for searching, sorting, and sharing.
- **Travel-friendly**: Perfect for photographers who move between timezones or travel often.

## Tips

- You can always preview changes before applying them—no surprises!
- If you make a mistake, you can re-run the shift as needed.
- The feature works for both individual files and whole collections.

## Where to Find It

- The Timezone Shift option is available in the archive view menu when you have one or more files selected.
