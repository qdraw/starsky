---
slug: smarter-imports-conditional-rules-structure-colorclass
title: "Smarter Imports in Starsky: Conditional Rules for Structure and ColorClass"
authors: dion
tags: [photo management, software update]
date: 2025-06-22
image: https://media.qdraw.nl/log/de-7-dingen-die-ik-miste-bij-het-beheren-van-mijn-foto-collectie/1000/02_starsky_v052_kl1k.jpg
---

# Smarter Imports in Starsky: Conditional Rules for Structure and ColorClass

We’re excited to introduce a powerful new feature in Starsky that gives you more control than ever
over how your photos are imported, organized, and tagged. With the new **ImportTransformation** and
**Structure Rules**, you can now define conditional logic that applies different folder structures
and metadata depending on image format or origin.

This does mean a **breaking change** in v0.7.0 or newer to how the folder structure is defined, so
let’s walk you through what’s new and how you can use it.

<!-- truncate -->

## 🎯 What’s New?

Instead of using a single flat string for your import structure like:

```json
"Structure": "/yyyy/MM/yyyy_MM_dd/yyyyMMdd_HHmmss_{filenamebase}.ext" // legacy
```

> Note: This example works for v0.6.8 or older

We now support **rule-based configuration** using JSON objects. This lets you specify:

* Different folder structures for different image formats (e.g. PNGs)
* Special paths for test imports
* Automatically assigning a `ColorClass` during import
* All while keeping a default fallback in place

---

## 🧠 Example Configuration

Here’s a full example of what your new config might look like:

```json
"ImportTransformation": {
  "Rules": [
    {
      "Conditions": {
        "Origin": "test"
      },
      "ColorClass": 2
    },
    {
      "Conditions": {
        "ImageFormats": [
          "png"
        ]
      },
      "ColorClass": 2
    }
  ]
},
"Structure": {
  "DefaultPattern": "/yyyy/MM/yyyy_MM_dd*/yyyyMMdd_HHmmss_{filenamebase}.ext",
  "Rules": [
    {
      "Conditions": {
        "ImageFormats": [
          "png"
        ]
      },
      "Pattern": "/yyyy/MM/yyyy_MM_dd*/\\screen\\sho\\t/yyyyMMdd_HHmmss_{filenamebase}.ext"
    },
    {
      "Conditions": {
        "Origin": "test"
      },
      "Pattern": "/yyyy/MM/yyyy_MM_dd_\\t/yyyyMMdd_HHmmss_{filenamebase}.ext"
    }
  ]
}
```

---

## 🖼️ What This Does

* If a file was imported from a **test source** (`"Origin": "test"`), it:

    * Is stored under a path like:
      `/2025/06/2025_06_22_t/20250622_153015_example.jpg`
    * Gets assigned `ColorClass: 2`

* If the imported image is a **PNG**, it:

    * Is stored under a path like:
      `/2025/06/2025_06_22/screenshot/20250622_153015_example.png`
    * Also gets `ColorClass: 2`

* All other images fall back to the default structure:

    * `/2025/06/2025_06_22/20250622_153015_example.jpg`

---

## 🚨 Breaking Change Notice

If you were relying on the old flat `"Structure"` string, you’ll need to update your configuration
to the new format. While this adds some complexity, it opens the door for much more intelligent and
flexible imports — tailored to your workflow.

---

## ✅ Why This Matters

This new system is ideal for:

* Automatically classifying screenshots vs photos
* Keeping test imports separate and easy to spot
* Organizing by image type without extra manual steps
* Power users who want fine-grained control over folder structure and metadata tagging

---

## 📌 Final Thoughts

We hope this upgrade gives you more control and automation over your photo management. The new
Import Rules system is just the beginning — and we’re excited to hear what you build with it.

Got ideas or feedback? Join the discussion on GitHub or reach out via the usual channels.

Happy organizing!
— The Starsky Team
