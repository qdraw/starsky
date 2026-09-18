# Stacks

Stacks are enabled by default. A stack is a single logical photo represented by multiple physical
files on disk that share the same filename stem (everything before the extension) in the same folder.

For example, a camera that saves both a JPEG and a RAW file will produce two files with the same
name. Starsky groups them into one stack so the folder view stays clean while all files remain
accessible.

## What groups files into a stack

The rule is simple: **same folder + same filename stem = same stack**.

```
/2018/IMG_1234.jpg   ─┐
/2018/IMG_1234.arw   ─┤  shown as one item
/2018/IMG_1234.avi   ─┘

/2018/IMG_1235.jpg      separate item (different stem)
```

Any combination of extensions works: `.jpg + .raw`, `.jpg + .avi`, `.jpg + .xmp`, `.arw + .dng`,
and so on. There is no fixed limit on how many files can be in one stack.

## What you see in the folder view

With stacks enabled (the default), the folder shows **one thumbnail per stack**. The app picks
which file to use for the thumbnail:

1. The first file whose extension supports thumbnail generation (e.g. `.jpg`, `.png`, `.webp`)
2. If none qualify (e.g. a pure `.arw + .dng` pair), it falls back to whichever filename sorts
   first alphabetically

The other files in the stack are not deleted — they are simply hidden from the list. Switch to flat
view (see below) to see every file individually.

## What you see in the detail view

When you open a stacked item, all file paths that belong to the stack are available. This is used by
several features:

- **Bulk open in editor** — all stack members are opened together in your external editor
- **Metadata editing** — tags and color class are written to all members
- **Export** — you can export any or all members of the stack

## Switching between stacked and flat view

The `collections` query parameter controls the view mode on every API request:

| URL parameter | Effect |
|---|---|
| `collections=true` | Stacked view — one entry per filename stem (default) |
| `collections=false` | Flat view — every file listed individually |

The frontend stores this preference in the URL so it persists while navigating.

The `collectionsCount` field in the API response always reflects the total number of individual
files in the folder, regardless of the current view mode.

## Open files in an external editor

In desktop mode you can batch-open all files in a stack with your preferred editor. The desktop open
feature respects the stack settings so all members (JPEG, RAW, sidecar, etc.) are passed to the
editor at once.

See [Desktop Open](../getting-started/configuration/desktop-open.md) for more information.
