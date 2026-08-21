---
slug: whats-new-in-starsky-v080
title: "What's new in Starsky v0.8.0?"
authors: dion
tags: [photo management, software update]
date: 2026-08-21
image: https://media.qdraw.nl/log/de-7-dingen-die-ik-miste-bij-het-beheren-van-mijn-foto-collectie/1000/02_starsky_v052_kl1k.jpg
---

# What's new in Starsky v0.8.0?

Version 0.8.0 is a significant release centered on upgrading to .NET 10, bringing new virtual
folder mapping capabilities, and a series of quality and reliability improvements. Here's what's
changed since v0.7.22:

<!-- truncate -->

---

### 🚨 Breaking Changes

- **Upgrade to .NET 10**
    - Starsky's backend now runs on .NET 10 (Runtime 10.0.11, SDK 10.0.400). .NET 10 is the
      current major version and brings performance improvements across the board.
    - **Action required:** Your hosting environment must have the .NET 10 runtime installed. If
      you are using Docker, pull the updated image. If you run Starsky as a self-hosted service,
      update the runtime before deploying v0.8.0.

---

### 🆕 New Features

- **Virtual Storage Folder Mappings (Symlink)**
    - You can now define custom storage folder mappings in the application settings. This lets you
      map a virtual sub-path inside Starsky's library to a physical folder located anywhere on
      disk — without requiring OS-level symlinks.
    - This is useful for attaching an external drive, a network share, or a dedicated archive
      folder to your Starsky library without moving files or changing how the OS sees them.
    - The mapping is fully managed from the Preferences UI under a new **Storage Folder Mappings**
      panel.
    - Safety guardrails prevent mapping to sensitive system directories (e.g. `/etc`, `/System`,
      `C:\Windows`) to avoid accidental exposure of OS files.
    - The disk watcher picks up changes inside mapped folders automatically, so new files appear
      in the index just like any other library file.

- **HTML Publish: `<figure>` and `LargeImageDefaultSrc` template (PR #3217)**
    - The static HTML publish feature gains a new embedded view `LargeImageDefaultSrc.cshtml` and
      a `Legacy.cshtml` fallback template.
    - Images in published pages are now wrapped in a semantic `<figure>` element, improving
      accessibility and allowing finer CSS control over captions and layout.
    - The published index page now uses a 1000 px max-width container (up from 500 px) for a
      better reading experience on modern displays.

---

### 🛠️ Bug Fixes & Improvements

- **HTML Publish: `width`/`height` attributes were swapped (PR #3217)**
    - A bug caused the `width` and `height` attributes on published `<img>` elements to be
      inverted. Portrait photos were described with landscape dimensions and vice versa. Fixed.

- **ExifTool guard: prevent accidental auto-download (PR #3223)**
    - Added a guard in `ExifToolService` that validates the ExifTool binary before attempting to
      use it. If the binary is absent or fails a basic sanity check, Starsky logs a warning
      instead of silently falling back to an auto-download, which could be unexpected in
      restricted environments.

- **IOException AppSettings robustness (PR #3222)**
    - The `AppSettings` model now handles `IOException` when reading configuration, rather than
      propagating a raw exception on startup. This improves reliability on systems where the
      config file is temporarily locked (e.g. during a deploy or config rotation).

- **Cypress E2E test reliability on Windows (PR #3210)**
    - Fixed a flaky end-to-end test in the detail-view upload flow. On Windows, `blur()` triggers
      an asynchronous `FetchPost` to `/starsky/api/update`, but the test didn't wait for that
      network round-trip before reloading. The test now explicitly waits for the network call,
      making it reliable on both Linux and Windows CI runners.

---

### 🗃️ Infrastructure & Tooling

- **Upgraded to Cypress 20 (PR #3220)**
    - The end-to-end test suite now runs on Cypress 20, the latest major release.

- **RabbitMQ.Client updated to 7.2.2 (PR #3215)**
    - Keeps the message queue client up to date with the latest upstream patches.

- **Code quality pass for .NET 10 (PR #3205, #3207, #3208, #3209)**
    - A series of SonarQube-driven code quality improvements were applied alongside the .NET 10
      migration, reducing technical debt and bringing the codebase in line with .NET 10 analyzer
      rules.

- **Swagger .NET updated (PR #3202)**
    - The API documentation tooling has been updated to match the new .NET 10 stack.

---

### ⚠️ Upgrade Notes

1. **Install the .NET 10 runtime** on your server or update your Docker image before upgrading.
   Starsky will not start on .NET 8 after this release.
2. If you run ExifTool in an air-gapped or restricted environment, verify the binary is in place
   before starting the service — the new guard will log a warning on startup if it cannot be
   found, rather than silently auto-downloading.
3. If you use the HTML publish feature (`starskyWebHtmlCli`), re-publish any existing profiles
   to pick up the corrected `width`/`height` values and the new `<figure>` markup.

---

For the full list of changes, see [history.md](https://docs.qdraw.nl/docs/advanced-options/history).
