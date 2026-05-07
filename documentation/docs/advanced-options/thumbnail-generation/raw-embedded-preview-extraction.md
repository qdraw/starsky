---
sidebar_position: 16
---

# RAW Embedded Preview Extraction

Advanced documentation for handling embedded JPEG previews inside RAW image formats such as Canon CR2, Sony ARW, Nikon NEF, Fujifilm RAF, Panasonic RW2, Olympus ORF, and DNG containers.

This page focuses on the technical behavior of JPEG preview extraction, manufacturer-specific encoding quirks, marker parsing, malformed edge cases, and validation strategies used by the application.

---

# Overview

RAW image formats commonly contain one or more embedded JPEG previews used for:

* Fast thumbnail rendering
* Preview generation
* Metadata indexing
* Gallery browsing
* Non-destructive workflows

These previews are usually stored inside TIFF/EXIF structures and encoded using standard or non-standard JPEG variants.

The extractor does not rely on simplistic JPEG heuristics. Instead, it performs marker-level scanning to identify actual JPEG encoding types and determine whether a preview is suitable for extraction.

---

# JPEG SOF Marker Detection

JPEG encoding type is determined by the Start-Of-Frame (SOF) marker.

The application scans marker segments until the first valid SOF marker is discovered.

## Supported SOF Variants

| SOF   | Marker   | Encoding                 | Classification |
| ----- | -------- | ------------------------ | -------------- |
| SOF0  | `0xFFC0` | Baseline DCT             | Lossy          |
| SOF1  | `0xFFC1` | Extended Sequential      | Lossy          |
| SOF2  | `0xFFC2` | Progressive DCT          | Lossy          |
| SOF3  | `0xFFC3` | Lossless Sequential      | Lossless       |
| SOF5  | `0xFFC5` | Differential Sequential  | Lossy          |
| SOF6  | `0xFFC6` | Differential Progressive | Lossy          |
| SOF7  | `0xFFC7` | Arithmetic Lossless      | Lossless       |
| SOF9  | `0xFFC9` | Arithmetic Baseline      | Lossy          |
| SOF10 | `0xFFCA` | Arithmetic Progressive   | Lossy          |
| SOF11 | `0xFFCB` | Arithmetic Lossless      | Lossless       |

---

# Lossless JPEG Handling

The extractor intentionally skips lossless JPEG previews.

Lossless JPEGs inside RAW containers are typically:

* archival representations
* preservation formats
* high-fidelity internal data
* non-preview image streams

## Lossless Markers

The following SOF markers are classified as lossless:

| Marker   | Meaning |
| -------- | ------- |
| `0xFFC3` | SOF3    |
| `0xFFC7` | SOF7    |
| `0xFFCB` | SOF11   |

All other supported SOF variants are treated as lossy preview candidates.

---

# JPEG Marker Parsing

The parser processes JPEG marker segments sequentially.

## Marker Structure

Most JPEG markers use the following layout:

```text
FF XX | Length (2 bytes) | Payload
```

Where:

* `FF XX` = marker code
* Length is big-endian
* Length includes the 2-byte length field itself

Exceptions:

* SOI (`0xFFD8`)
* EOI (`0xFFD9`)
* Restart markers (`0xFFD0-0xFFD7`)

These markers do not contain length fields.

---

# Common Marker Types

| Marker    | Code            | Purpose               |
| --------- | --------------- | --------------------- |
| SOI       | `0xFFD8`        | Start of image        |
| EOI       | `0xFFD9`        | End of image          |
| DHT       | `0xFFC4`        | Huffman tables        |
| DQT       | `0xFFDB`        | Quantization tables   |
| APP0      | `0xFFE0`        | JFIF header           |
| APP1      | `0xFFE1`        | EXIF metadata         |
| APP13     | `0xFFED`        | Photoshop / MakerNote |
| COM       | `0xFFFE`        | Comment metadata      |
| RST0-RST7 | `0xFFD0-0xFFD7` | Restart markers       |

---

# RAW Camera Manufacturer Quirks

Different camera manufacturers embed previews differently.

The extractor includes compatibility logic for these patterns.

---

# Canon RAW (CR2)

Typical Canon preview layout:

```text
SOI
 ├── APP0 (JFIF)
 ├── APP1 (EXIF)
 ├── APP13 (MakerNote)
 ├── DHT
 ├── SOF0
 └── SOS
```

## Known Characteristics

* Multiple APP marker stacks
* Large DQT tables
* Optional COM markers
* Baseline SOF0 previews
* Occasional SOF3 lossless variants

---

# Nikon RAW (NEF/NRW)

Nikon previews sometimes use arithmetic encoding.

## Known Characteristics

* SOF9 arithmetic baseline encoding
* APP13 MakerNote metadata
* Multiple restart markers
* Mixed firmware behavior across generations

Arithmetic encoding remains lossy and should still be treated as preview-capable.

---

# Sony RAW (ARW)

Sony firmware varies significantly across Alpha generations.

## Known Characteristics

* Progressive SOF2 previews
* SOF3 lossless metadata streams
* Concatenated JPEG streams
* Multiple preview resolutions

Some ARW files contain:

```text
EOI -> SOI
```

indicating back-to-back JPEG previews.

---

# Olympus / Panasonic

Olympus ORF and Panasonic RW2 may contain arithmetic lossless previews.

## Known Characteristics

* SOF11 arithmetic lossless encoding
* Mixed SOF0/SOF11 usage
* High-fidelity internal previews

SOF11 previews are skipped by the extractor.

---

# Fujifilm RAW (RAF)

Fujifilm often uses uncommon JPEG variants.

## Known Characteristics

* SOF1 extended sequential encoding
* Non-standard APP0 lengths
* Multiple preview streams

SOF1 is uncommon but still classified as lossy.

---

# Leica / Hasselblad

Scientific and medium-format systems may use hierarchical JPEG encoding.

## Known Characteristics

* SOF5 differential sequential
* SOF6 differential progressive
* Specialized preview workflows

These formats are rare but supported.

---

# Edge Case Handling

The extractor is designed to tolerate malformed or partially corrupted JPEG streams.

---

# Zero-Length Segments

Example:

```text
FFC4 0000
```

Problem:

* invalid marker length
* firmware corruption
* truncated writes

Behavior:

* segment is rejected
* parser safely continues

---

# Incomplete SOF Segments

If insufficient bytes exist for SOF metadata:

* image dimensions cannot be read
* encoding type is unknown
* stream is treated as invalid

---

# Multiple SOF Markers

Some firmware writes multiple SOF markers.

Behavior:

* first valid SOF marker wins
* scanning stops after classification

---

# Reserved Markers

Unknown markers such as:

```text
FFF0
FFF1
```

may appear before the real SOF marker.

Behavior:

* parser skips unsupported markers
* scanning continues safely

---

# Incorrect Segment Lengths

Malformed length fields can cause parser desynchronization.

Mitigation:

* bounded reads
* offset validation
* range checking
* conservative fallback behavior

---

# TIFF Integration

RAW formats typically store JPEG previews inside TIFF containers.

The extraction pipeline works as follows:

1. Parse TIFF Image File Directories (IFDs)
2. Locate JPEG preview offsets
3. Extract candidate preview ranges
4. Scan JPEG markers
5. Detect SOF encoding
6. Skip lossless previews
7. Return valid lossy preview

---

# Non-Seekable Streams

Some sources do not support seeking:

* network streams
* pipes
* HTTP responses
* archive streams

The extractor buffers these streams at entry point level before TIFF parsing begins.

Benefits:

* prevents downstream seek failures
* preserves parser consistency
* allows transparent offset scanning

---

# Performance Considerations

## SOF Scanning

Marker scanning is relatively inexpensive but still measurable when processing large RAW collections.

Recommended optimization:

* cache SOF detection results
* avoid repeated scans of identical preview regions

Typical SOF discovery occurs within:

```text
30-50 bytes after SOI
```

but malformed files may require deeper scanning.

---

# Implementation Recommendations

## Recommended Practices

1. Always scan for actual SOF markers
2. Never rely on fixed-byte heuristics
3. Support uncommon SOF variants
4. Validate all marker lengths
5. Handle malformed streams conservatively
6. Buffer non-seekable streams early
7. Preserve stream position during probing
8. Test against real RAW files

---

# Testing

The application includes comprehensive SOF detection coverage for real-world and malformed JPEG patterns.

## Covered Scenarios

| Scenario                    | Expected Result |
| --------------------------- | --------------- |
| SOF0 baseline JPEG          | Lossy           |
| SOF2 progressive JPEG       | Lossy           |
| SOF3 lossless JPEG          | Lossless        |
| SOF9 arithmetic baseline    | Lossy           |
| SOF11 arithmetic lossless   | Lossless        |
| APP0/APP1/APP13 Canon stack | Lossy           |
| Restart markers before SOF  | Lossy           |
| Zero-length DHT             | Invalid         |
| Truncated SOF               | Invalid         |
| Multiple APP markers        | Supported       |
| Concatenated JPEGs          | Supported       |

## Test Coverage Goals

The tests validate:

* correct SOF classification
* malformed marker handling
* stream safety
* offset validation
* TIFF preview extraction
* non-seekable stream buffering
* manufacturer-specific preview layouts

---

# Summary

RAW embedded preview extraction is significantly more complex than standard JPEG decoding.

Real-world RAW files frequently contain:

* uncommon SOF variants
* malformed marker segments
* concatenated previews
* arithmetic encoding
* lossless JPEG streams
* firmware-specific metadata layouts

The extractor therefore performs full marker-aware JPEG scanning instead of relying on simplified detection heuristics.
