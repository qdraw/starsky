---
slug: using-cloud-import-with-dropbox-camera-uploads
title: "New feature: Using Cloud Import with Dropbox Camera Uploads"
authors: dion
tags: [photo mangement, software update]
date: 2026-01-08
image: https://media.qdraw.nl/log/de-7-dingen-die-ik-miste-bij-het-beheren-van-mijn-foto-collectie/1000/02_starsky_v052_kl1k.jpg
---

# New feature: Using Cloud Import with Dropbox Camera Uploads

Cloud Import is a new feature in v0.7.5 or later that adds automated ingestion of files from cloud storage providers to the existing import pipeline. Instead of relying on manual uploads or custom scripts, Cloud Import periodically synchronizes a configured remote folder—such as Dropbox Camera Uploads—detects new files, and imports them using the same validation, error handling, and logging already in place. This makes cloud-based file ingestion a first-class, reliable part of the system rather than an external or ad-hoc process.

{/* truncate */}

One practical use case for Cloud Import is ingesting photos and videos from **Dropbox Camera Uploads**. Camera uploads are convenient, but they quickly turn into a passive inbox: files appear automatically, over time, without a clear moment to act on them.

Cloud Import makes this folder an explicit input source for the import pipeline.

## The Setup

Dropbox Camera Uploads place new photos and videos in a fixed folder (typically `/Camera Uploads`). Cloud Import connects to Dropbox at a configured interval and treats this folder like any other import source.

The configuration is straightforward:

* Provider: `Dropbox`
* Remote folder: `/Camera Uploads`
* Sync interval: typically hourly
* Optional file cleanup after successful import
* Credentials supplied via configuration or environment variables

Once enabled, no further manual steps are required.

## What Happens During a Sync

Each synchronization follows a predictable process:

1. Cloud Import connects to Dropbox using the configured credentials
2. It lists the contents of the `/Camera Uploads` folder
3. Files are filtered by extension (for example `jpg`, `png`, `mp4`)
4. Files that were already processed within the last 24 hours are skipped
5. New files are downloaded
6. Each file is passed into the existing import pipeline

From the import pipeline’s perspective, there is no difference between a manually uploaded file and one coming from Dropbox.

## Handling Continuous Uploads

Camera uploads don’t arrive in batches. Files can appear:

* Individually
* In bursts
* Hours or days apart
* From multiple devices

Cloud Import is designed for this pattern:

* Each file is handled independently
* Failed imports do not block others
* Already-processed files are safely skipped
* Syncs can run repeatedly without duplication

This makes it suitable for long-running, unattended operation.

## Post-Import Cleanup

Depending on how the Dropbox folder is used, cleanup can be enabled or disabled.

* When enabled, files are deleted from Dropbox only after a successful import
* When disabled, files remain in `/Camera Uploads` and act as an archive
* Files that fail to import are never deleted

All cleanup actions are logged explicitly.


## Error Handling and Visibility

Camera uploads are not always clean:

* Files may still be uploading
* Videos may be large
* Network connections may be unstable

Cloud Import handles these cases gracefully:

* Connection and authentication issues are logged and retried on the next sync
* Import failures affect only the individual file
* Each sync records how many files were found, imported, skipped, or failed

This provides a clear operational view without manual checks.

## Why Dropbox Camera Uploads Work Well

Dropbox Camera Uploads are a good match for Cloud Import because:

* Files arrive automatically and consistently
* Folder structure is stable
* File types are predictable
* No manual intervention is expected

Cloud Import turns this passive storage location into an active, reliable input channel.


## Example Configuration

```json
{
  "app": {
    "CloudImport": {
      "Providers": [
        {
          "Id": "dropbox-camera-uploads",
          "Enabled": true,
          "Provider": "Dropbox",
          "RemoteFolder": "/Camera Uploads",
          "SyncFrequencyHours": 1,
          "DeleteAfterImport": false,
          "Extensions": ["jpg", "png", "mp4"],
          "Credentials": {
            "RefreshToken": "",
            "AppKey": "",
            "AppSecret": ""
          }
        }
      ]
    }
  }
}
```

## Summary

Using Cloud Import with Dropbox Camera Uploads provides a stable, automated way to ingest photos and videos as they appear. The folder becomes a reliable input source rather than a place that requires manual checking.

Files are imported once, validated consistently, and handled safely — even when uploads are continuous and unattended.
