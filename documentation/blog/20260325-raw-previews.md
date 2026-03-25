---
slug: new-feature-raw-preview-extraction
title: "New feature: RAW Preview Extraction"
authors: dion
tags: [photo mangement, software update]
date: 2026-03-25
image: https://media.qdraw.nl/log/de-7-dingen-die-ik-miste-bij-het-beheren-van-mijn-foto-collectie/1000/02_starsky_v052_kl1k.jpg
---

# RAW Preview Extraction

We are excited to announce our new RAW image thumbnail extraction system! Our implementation provides a lightning-fast, "zero-decode" approach to extracting high-quality JPEG previews directly from various RAW camera formats, including **DNG, Canon CR2/CR3, Nikon NEF, Sony ARW, Fujifilm RAF, and Sigma X3F**.

<!-- truncate -->

## The Philosophy: Zero-Decode

Traditional RAW processing involves heavy decoding and debayering, which is CPU and memory-intensive. Our approach treats RAW files as structured binary containers. By parsing the file's metadata structure (such as TIFF-based IFDs or ISOBMFF boxes), we locate the embedded, full-resolution JPEG previews and stream them directly to disk. This method is orders of magnitude faster and significantly more memory-efficient.

## Key Technical Achievements

### 1. Robust Metadata Traversal
The core of our extraction is a suite of high-performance metadata parsers.
- **Recursive IFD Parsing:** Navigates main Image File Directories (IFDs) and Sub-IFDs to find all available preview candidates.
- **Cycle Detection:** Built-in safeguards against infinite loops in malformed files (Max Depth: 6, Max Visits: 64).
- **Endianness Support:** Automatic detection and handling of both Little-Endian ('II') and Big-Endian ('MM') formats.
- **Modern Containers:** Support for ISOBMFF (CR3) and custom binary containers (RAF, X3F).

### 2. Specialized MakerNote Support
Standard TIFF tags are often not enough. Many camera manufacturers hide their highest-quality previews in proprietary "MakerNote" sections.
- **Sony ARW:** Specialized parsing for Sony private tags (0x2010, 0x2011, 0x2020).
- **Canon CR2:** Support for both IFD-based previews and MakerNote-embedded JPEGs.
- **Smart Selection:** Uses a selection heuristic to choose the largest/best quality JPEG when multiple candidates are found.

### 3. Performance-First Engineering
We've utilized modern C# features to ensure minimal overhead:
- **Zero-Copy with `Span<byte>`:** Heavy use of spans for stack-allocated buffers and on-the-fly header parsing.
- **Memory Efficiency:** Uses `ArrayPool<byte>` to minimize garbage collection pressure when reading larger metadata blocks.
- **Early Exit:** Once the best candidate is identified and verified (by checking JPEG SOI markers), the system exits early to save I/O.

## Implementation Status

| Format | Extension | Implementation | Status |
|--------|-----------|-----------------|--------|
| **DNG** | .dng | TIFF IFD traversal | ✅ Ready |
| **Canon CR2** | .cr2 | TIFF/MakerNote | ✅ Ready |
| **Canon CR3** | .cr3 | ISOBMFF Container | ✅ Ready |
| **Nikon NEF** | .nef | TIFF IFD traversal | ✅ Ready |
| **Sony ARW** | .arw | TIFF/MakerNote | ✅ Ready |
| **Fujifilm RAF**| .raf | Custom container | ✅ Ready |
| **Sigma X3F** | .x3f | Lightweight container| ✅ Ready |

## Verification & Quality

Our implementation is backed by an extensive test suite ensuring reliability across edge cases:
- **Comprehensive Unit Tests** for all extractors, covering:
    - Valid/Invalid magic numbers and headers.
    - Deeply nested or cyclic structures.
    - Specialized MakerNote extraction.
    - Byte-level JPEG marker validation.
- **Integration Tests:** Verifying the full pipeline from raw file to extracted thumbnail in temporary storage.

## What's Next?

Our focus continues to be on broadening support and further optimizing performance:
- **Refining MakerNote parsing** for additional manufacturers.
- **Enhanced fallback mechanisms** for non-standard RAW variants.
- **Further I/O optimizations** for high-concurrency environments.


