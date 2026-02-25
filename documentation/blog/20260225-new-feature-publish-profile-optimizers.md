---
slug: new-feature-publish-profile-optimizers
title: "New feature: Publish Profile Optimizers"
authors: dion
tags: [photo mangement, software update]
date: 2026-02-25
image: https://media.qdraw.nl/log/de-7-dingen-die-ik-miste-bij-het-beheren-van-mijn-foto-collectie/1000/02_starsky_v052_kl1k.jpg
---

# New feature: Publish Profile Optimizers

Publishing in Starsky just became smarter and more flexible. You can now configure **optimizers** in your publish pipeline, with reusable defaults and per-profile overrides. This gives you better control over image output size and quality, without changing your existing publishing workflow.

<!-- truncate -->

## What is new?

A new configuration section was added:

- `publishProfilesDefaults`
- `profileFeatures.optimization.enabled`
- `optimizers[]`

With this, you can define optimizer behavior once and reuse it across profiles.

## Why this matters

When you publish web images, you usually want two things at the same time:

- Good visual quality
- Smaller file sizes for faster loading

The new optimizer feature helps you achieve both by making optimization a first-class part of the publish profile setup.

## How it works

1. Starsky reads your selected publish profile (for example `_default`).
2. It runs all profile steps in order (`html`, `jpeg`, `publishManifest`, etc.).
3. For image-producing steps, optimizer settings are applied for matching formats (for example `jpg`).
4. Per-step optimizer settings can override defaults when needed.

## Global defaults with `publishProfilesDefaults`

Use defaults to define optimization support once:

```json
"publishProfilesDefaults": {
  "profileFeatures": {
    "optimization": {
      "enabled": true
    }
  },
  "optimizers": [
    {
      "imageFormats": [
        "jpg"
      ],
      "id": "mozjpeg",
      "enabled": false,
      "options": {
        "quality": 80
      }
    }
  ]
}
```

### What each setting does

- `profileFeatures.optimization.enabled`: enables optimizer support for publish profiles
- `optimizers[].id`: optimizer engine (for example `mozjpeg`)
- `optimizers[].imageFormats`: target formats (for example `jpg`)
- `optimizers[].enabled`: default optimizer state
- `optimizers[].options`: optimizer-specific options like `quality`

## Per-profile control in `publishProfiles`

Need different behavior for a specific output variant? Add `optimizers` directly inside that publish step:

```json
{
    "ContentType": "jpeg",
    "SourceMaxWidth": 1000,
    "OverlayMaxWidth": 380,
    "Path": "{AssemblyDirectory}/WebHtmlPublish/EmbeddedViews/qdrawlarge.png",
    "Folder": "1000",
    "Append": "_kl1k",
    "Copy": "true",
    "optimizers": [
        {
            "imageFormats": ["jpg"],
            "id": "mozjpeg",
            "enabled": true,
            "options": {
                "quality": 80
            }
        }
    ]
}
```

This lets you keep safe defaults, while explicitly enabling optimization only where you want it.

## Recommended setup

A practical pattern:

- Keep shared optimizer rules in `publishProfilesDefaults`
- Enable or tune optimizer per publish step when needed
- Use separate profiles (`_default`, `no_logo_2000px`, `no_logo_1000px`) for output variants

This keeps your configuration clean, reusable, and easy to maintain.

## Backward compatibility

Existing publish profiles keep working as before. The optimizer feature is additive and can be introduced gradually in your current setup.

If you are already using publish profiles, this is an easy upgrade that can reduce output size while preserving quality.
