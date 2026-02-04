---
slug: whats-new-in-starsky-v070
title: "What’s new in Starsky v0.7.0?"
authors: dion
tags: [photo mangement, software update]
date: 2025-06-23
image: https://media.qdraw.nl/log/de-7-dingen-die-ik-miste-bij-het-beheren-van-mijn-foto-collectie/1000/02_starsky_v052_kl1k.jpg
---

# What’s new in Starsky v0.7.0?

Version 0.7.0 is a major update focused on smarter imports, improved performance, and breaking
changes to configuration. Here’s what’s changed since v0.6.8:

<!-- truncate -->

### 🚨 Breaking Changes

- **Import Structure Rules**
    - The import folder structure is now defined using a JSON object with conditional rules, rather
      than a single string.
    - This allows different folder structures and ColorClass assignment based on image format or
      source (e.g. screenshots, test imports).
    - **Action required:** Update your config to the new rule-based format. See
      the [detailed blog post](https://docs.qdraw.nl/blog/smarter-imports-conditional-rules-structure-colorclass)
      for examples and migration guidance.

- **Exiftool Checksum API**
    - The checksum API used by Exiftool has changed. This breaks automatic setup of the Exiftool
      tool in previous versions.

- **Default thumbnail format: webp**
    - Thumbnails are now generated in webp format by default for improved size and performance.

---

### 🆕 New Features

- **Conditional Import Rules**
    - Define different import folder structures and automatically assign ColorClass based on image
      metadata (origin or format).
    - Great for workflows that separate e.g. screenshots from camera photos or test data.

- **Thumbnail support for video files**
    - Starsky can now view previews for video files. We use ffmpeg to generate thumbnails
      for video files, which are stored in the thumbnail folder.

- **Camera Body Serial in Database**
    - The camera’s serial number is now stored, enabling more detailed cataloguing.

- **Backend Improvements**
    - More robust and clearer import transformation and structure logic.
    - New CompareHelper for import config.

- **Native thumbnail generation**
    - Starsky now uses the platform’s native QuickLook (on macOS) and the Windows shell image
      preview when loading an image, if available.

    - This means:
        - On macOS, Starsky can display QuickLook previews, providing faster and more compatible
          image viewing for supported formats.
        - On Windows, it leverages the native image shell for improved image loading and
          compatibility.

    - Benefit:
        - Native previews typically offer better performance and support for a wider range of image
          types/formats, and make Starsky feel more integrated with your operating system.

---

### 🛠️ Bug Fixes & Improvements

- Replace with default status now truly replaces (fix)
- Fixed backend handling of timeouts (#2189), quote handling, and various edge cases with thumbnails
  and notifications.
- Improved architecture references for trash/metaupdate.
- Tests failing in menu-archive component (#2216)
- Upload menu placement was wrong in readonly mode (#2106)
- Quote handling bug – issues with certain characters not being interpreted correctly (#1510)
- Move modal status 'ExifWriteNotSupported' shown in red (#1891)

---

### 🖥️ Frontend Upgrades

- **React 19**
    - The frontend now runs on React 19 instead of React 18.
- **Refresh Button**
    - New refresh button in the main menu.
- **Upload Button and Accessibility**
    - Fixed issues with upload button in readonly and improved accessibility.

---

### 🗃️ Infrastructure & Tooling

- **.NET 8 SDK 8.0.411**
    - Upgraded to latest LTS SDK/runtime for all backend services.
- Removed old sync tool, that is no longer in use.
- Upgraded various npm packages and internal dependencies.

---

### ⚠️ Upgrade Notes

- Review your import configuration! The new rule-based system is more powerful, but incompatible
  with the old flat string format.
- Revisit any automation or tools relying on the Exiftool checksum API.

---

For more details, see the [history.md](https://docs.qdraw.nl/docs/advanced-options/history) or
the [dedicated blogpost on smarter imports](20250622-smarter-imports-conditional-rules-structure-colorclass.md).

---

**In short:**  
Smarter, more flexible imports. Breaking config change for import structure. Backend and frontend
upgrades. Required config updates for all users!