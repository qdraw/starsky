---
slug: new-feature-batch-rename-by-date-and-time
title: "New feature: Batch Rename by date and time"
authors: dion
tags: [photo mangement, software update]
date: 2026-01-14
image: https://media.qdraw.nl/log/de-7-dingen-die-ik-miste-bij-het-beheren-van-mijn-foto-collectie/1000/02_starsky_v052_kl1k.jpg
---

# New feature: Batch Rename by date and time: 

Managing large photo collections just got a lot easier.

## Rename Multiple Photos in Seconds


With the new **Batch Rename** feature in v0.7.7 or later, you can now rename multiple photos at once using flexible, powerful patterns — with a live preview before anything is changed. No more manual renaming, no more mistakes, and no more guesswork.

{/* truncate */}

---

## Why Batch Rename?

If you’ve ever returned from a trip, photoshoot, or event with dozens (or hundreds) of photos named something like:

```
DSC03746.JPG
DSC03747.JPG
DSC03748.JPG
```

you know how quickly things become unmanageable.

Batch Rename lets you bring **structure and consistency** to your files in one simple action.

---

## How It Works

### 1. Select Your Photos

Choose one or more photos in the archive.

### 2. Open “Rename Photos”

Open the **More** menu and select **Rename by date and time**.
(The option only appears when files are selected.)

### 3. Define a Rename Pattern

Enter a pattern using placeholders for dates, filenames, and sequence numbers.

For example:

```
{yyyy}{MM}{dd}_{HH}{mm}{ss}_{filenamebase}.{ext}
```

### 4. Preview Before Applying

Click **Preview** to see exactly how your files will be renamed.

* First and last items are shown
* Errors are highlighted per file
* No changes are made yet

### 5. Rename with Confidence

If everything looks good, click **Rename** to apply the changes instantly.

---

## Powerful Pattern Support

Batch Rename supports a rich set of placeholders so you can create meaningful filenames.

### Date & Time

* `{yyyy}` – year
* `{MM}` – month
* `{dd}` – day
* `{HH}`, `{mm}`, `{ss}` – time

### Filename

* `{filenamebase}` – original filename
* `{ext}` – file extension

### Sequence Numbers

* `{seqn}` – incremental number
* `{seqn:N}` – padded sequence (e.g. `{seqn:3}` → `001`)

---

## Preview & Error Handling

One of the most important parts of Batch Rename is **safety**.

* Every rename is validated during preview
* File-specific errors are shown clearly
* Renaming is blocked until all errors are resolved

This ensures you always know *exactly* what will happen before committing changes.

---

## Recent Patterns, Automatically Saved

Your most recently used rename patterns are automatically saved locally and available in a dropdown the next time you open the modal.

* Up to **10 recent patterns**
* Stored in your browser
* No manual saving required

Perfect for recurring workflows.

---

## Designed for Every Device

Batch Rename is built with usability in mind:

* Fully responsive (desktop & mobile)
* Dark mode support
* Keyboard accessible
* Available in **English, Dutch, and German**

---

## Try It Out

Select a few photos, open the menu, and give Batch Rename a try.
It’s a small feature that saves a surprising amount of time — especially if you care about clean, meaningful filenames.

Happy organizing 📸
