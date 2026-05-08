# Embedded RAW Thumbnail Extraction Architecture

This document describes the technical implementation of the `EmbeddedRawThumbnailService`
and its associated extractors. The primary goal of this system is to efficiently extract
high-quality JPEG previews embedded within various RAW camera formats and standard JPEG/TIFF files.

## Overview

The extraction process is coordinated by the `EmbeddedRawThumbnailService`.
It uses a factory-like approach to select the appropriate extractor based on the file's image format
(detected via magic bytes).

### Supported Formats

- **TIFF-based RAWs:** ARW (Sony), CR2 (Canon), NEF (Nikon), DNG (Adobe), and standard TIFF.
- **Fujifilm:** RAF.
- **Canon High Efficiency:** CR3 (ISO/IEC 14496-12 / HEIF container).
- **Sigma:** X3F.
- **Standard JPEG:** JPG/JPEG (extracts embedded EXIF thumbnails).

### Portrait & Rotation Support

All extracted embedded previews automatically support **EXIF-based image orientation and rotation**,
including:

- **Portrait mode:** Images rotated 90° or 270° are correctly oriented.
- **Landscape mode:** Standard and flipped orientations (EXIF values 1-8).
- **Automatic handling:** EXIF orientation metadata from the embedded JPEG is automatically detected
  and applied during thumbnail generation via ImageSharp's `AutoOrient()` method.

This ensures that portrait photographs taken with portrait orientation are displayed correctly without
requiring manual rotation adjustments.

---

## Core Components

### 1. EmbeddedRawThumbnailService

The entry point that orchestrates the extraction.

- **Format Detection:** Uses `ExtensionRolesHelper` to identify the file format by reading the
  initial bytes.
- **Dispatching:** Routes the request to specific extractor implementations.

### 2. TiffEmbeddedPreviewExtractor

Handles formats based on the TIFF structure (ARW, CR2, NEF, DNG, TIFF).

- **Recursive IFD Parsing:** Navigates Image File Directories (IFDs) and Sub-IFDs.
- **MakerNote Support:** Specialized logic for Sony and Canon MakerNotes to find high-resolution
  previews often hidden in proprietary tags.
- **Heuristics:** Uses `SelectBestPreviewHelper` to choose the largest/best quality JPEG from
  multiple candidates found in the TIFF structure.

### 3. RafPreviewExtractor (Fujifilm)

Fujifilm RAF files have a unique header followed by various data blocks.

- **Header Parsing:** Attempts to find the preview offset and length directly from the RAF header (
  at fixed offsets `0x54` and `0x58`).
- **Fallback Scanning:** If the header doesn't point to a valid JPEG, it utilizes
  `ContainerJpegScanner` to perform a byte-level scan.

### 4. ContainerFormatPreviewExtractor (Canon CR3)

Handles ISO-Base Media File Format (ISOBMFF) containers, primarily used by Canon CR3.

- **Structure:** Parses the "boxes" (atoms) of the container.
- **Smart Scanning:** Since CR3 files often store previews in `uuid` or `mdat` boxes, it uses a
  combination of structure parsing and targeted byte scanning to locate the JPEG SOI (`0xFFD8FF`)
  markers.

### 5. LightweightContainerPreviewExtractor (Sigma X3F)

Specifically designed for Sigma X3F files.

- **Tagged Data:** Searches for specific tags in the X3F structure that point to embedded JPEG data.
- **Validation:** Verifies JPEG headers at the resolved offsets to ensure data integrity.

### 6. JpegExifPreviewExtractor

Processes standard JPEG files to extract their internal EXIF thumbnails (App1 segment).

- **Marker Stream Parsing:** Iterates through JPEG markers (`APP0`, `APP1`, etc.) without decoding
  the main image.
- **EXIF Extraction:** Parses the TIFF structure inside the `APP1` segment to find the thumbnail
  IFD (usually IFD1).

---

## Utility Classes

### ContainerJpegScanner

A heavy-duty scanner used as a fallback for RAF and other container formats.

- **Byte-level Scan:** Searches for all occurrences of `0xFFD8FF` (JPEG Start of Image).
- **IPTC/APP13 Validation:** Prefers JPEGs that contain IPTC metadata, as these are typically the
  high-resolution previews intended for display.
- **Stream Verification:** Validates the JPEG stream by following markers until an EOI (`0xFFD9`) is
  found.

### StreamPrimitives

Provides low-level, high-performance stream operations:

- **Big/Little Endian Reading:** Methods for reading 16-bit and 32-bit integers.
- **Safe Seeking:** Ensures seek operations are valid within stream bounds.

### SelectBestPreviewHelper

Logic to compare multiple JPEG candidates.

- **Criteria:** Primarily uses dimensions (if available) or byte size as a proxy for quality.

---

## Technical Details: How RAW Files are Handled

The handling of RAW files follows a "Zero-Decode" philosophy. Instead of using heavy RAW processing
libraries (like LibRaw), the system treats RAW files as binary containers.

1. **Direct Stream Access:** Files are accessed via `IStorage` streams, minimizing memory overhead.
2. **Signature Matching:** Identifies internal structures (TIFF headers, ISOBMFF boxes, RAF
   signatures).
3. **Offset Resolution:** Calculates absolute offsets of embedded JPEGs by parsing the metadata
   trees.
4. **Byte Copying:** Once a valid JPEG range is identified and verified (by checking for `SOI` and
   `EOI` markers), the raw bytes are streamed directly to the output destination.

This approach is extremely fast and has minimal memory requirements, making it suitable for
high-throughput thumbnail generation.
