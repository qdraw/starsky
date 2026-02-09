---
slug: new-feature-timezone-shift-adjust-photo-timestamps-easily
title: "New feature: Timezone Shift: Adjust Photo Timestamps Easily"
authors: dion
tags: [photo mangement, software update]
date: 2026-02-01
image: https://media.qdraw.nl/log/de-7-dingen-die-ik-miste-bij-het-beheren-van-mijn-foto-collectie/1000/02_starsky_v052_kl1k.jpg
---

# New feature: Timezone Shift: Adjust Photo Timestamps Easily

Have you ever returned from a trip, only to find your camera’s clock was set to the wrong timezone? Or maybe you moved to a new country and your photo timestamps no longer match your memories? With Starsky’s new **Timezone Shift** feature, fixing these issues is now quick, accurate, and painless. This feature is avaiable in v0.7.10 or later

<!-- truncate -->

## Why Timezone Shift Matters

Photo timestamps are more than just numbers—they help you organize, search, and relive your memories. But when your camera’s timezone is off, your photo library can become a confusing mess. The Timezone Shift tool is designed to solve this problem for everyone, from casual travelers to professional photographers.

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

## Real-World Example

Imagine you took a trip from Amsterdam to London, but your camera stayed on Amsterdam time. With Timezone Shift, you can select all your London photos, choose the correct conversion, and instantly update every timestamp—no manual editing required.

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

## Tips for Best Results

- Always preview changes before applying them—no surprises!
- If you make a mistake, you can re-run the shift as needed.
- The feature works for both individual files and whole collections.

## Where to Find It

You’ll find the Timezone Shift option in the archive view menu whenever you have one or more files selected. It’s designed to be intuitive, so you can focus on your photos—not on fixing metadata.

---

With Starsky’s Timezone Shift, your photo library stays as accurate as your memories. Try it out and see how easy it is to keep your timestamps in sync with your life!
