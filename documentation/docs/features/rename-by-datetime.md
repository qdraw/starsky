# Rename by date and time (batch)

Rename multiple photos at once using flexible patterns with dates, sequences, and filenames.

---

## Overview

The **Batch Rename** feature allows you to rename multiple photos in one action using a customizable pattern.
You can preview the result before applying changes, catch errors early, and reuse previously used patterns.

**Key benefits**

* Preview before rename
* Flexible pattern syntax
* Automatic sequence numbering
* Recent patterns saved locally
* Fully localized & accessible
* Dark mode & mobile-friendly

---

## How to Use

### 1. Select Photos

Select one or more photos in the archive.

### 2. Open Batch Rename

Open the **More** menu and choose **Rename by date and time**.

> The option only appears when files are selected and is disabled in read-only mode.

### 3. Enter a Rename Pattern

Enter a rename pattern using placeholders (see below), or select one from **Recent patterns**.

Example:

```
{yyyy}{MM}{dd}_{filenamebase}.{ext}
```

### 4. Preview

Click **Preview** to see how files will be renamed.

* Only the first few and last items are shown
* Errors are highlighted per file
* No changes are made yet

### 5. Rename

If no errors are found, click **Rename** to apply the changes.

---

## Rename Pattern Syntax

Patterns consist of **placeholders** that are replaced per file.

### Date & Time

| Placeholder | Description   |
| ----------- | ------------- |
| `{yyyy}`    | 4-digit year  |
| `{MM}`      | Month (01–12) |
| `{dd}`      | Day (01–31)   |
| `{HH}`      | Hour (24h)    |
| `{mm}`      | Minutes       |
| `{ss}`      | Seconds       |

### Filename

| Placeholder      | Description                           |
| ---------------- | ------------------------------------- |
| `{filenamebase}` | Original filename (without extension) |
| `{ext}`          | File extension                        |

### Sequence

| Placeholder | Description                               |
| ----------- | ----------------------------------------- |
| `{seqn}`    | Sequence number                           |

---

## Examples

### Date-based

```
{yyyy}{MM}{dd}_{filenamebase}.{ext}
```

**Result**

```
20240115_photo.jpg
```


---

## Preview & Errors

* Each file is validated during preview
* Errors are shown inline per file
* Renaming is blocked until all errors are resolved

Common errors:

* Invalid pattern
* File not found
* Permission issues

---

## Recent Patterns

* Stored automatically after a successful rename
* Maximum of **10 patterns**
* Saved in browser LocalStorage
* Accessible via the dropdown in the modal


