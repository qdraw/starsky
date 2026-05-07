# JPEG Embedded Preview Camera Manufacturer Quirks & Edge Cases

This document catalogues the unusual and non-standard JPEG encoding patterns used by camera manufacturers when embedding thumbnails and previews in RAW files (CR2, ARW, DNG, etc.).

## JPEG Encoding Variants by SOF Marker

The Start-Of-Frame (SOF) marker determines the type of JPEG encoding:

| SOF Code | Marker | Type | Lossy/Lossless | Usage | Cameras |
|----------|--------|------|---|---------|---------|
| SOF0 | 0xFFC0 | Baseline DCT | Lossy | Standard JPEG | All |
| SOF1 | 0xFFC1 | Extended Sequential DCT | Lossy | High-quality variant | HP, some scientific |
| SOF2 | 0xFFC2 | Progressive DCT | Lossy | Multi-pass encoding | Mobile phones |
| SOF3 | 0xFFC3 | Lossless | Lossless | No quantization | Some Canon, Sony |
| SOF5 | 0xFFC5 | Differential Sequential | Lossy | Hierarchical baseline | Non-standard |
| SOF6 | 0xFFC6 | Differential Progressive | Lossy | Hierarchical progressive | Non-standard |
| SOF7 | 0xFFC7 | Lossless with arithmetic | Lossless | Advanced lossless | Rare but valid |
| SOF9 | 0xFFC9 | Arithmetic Baseline | Lossy | Patent-restricted encoding | Nikon, some Canon |
| SOF10 | 0xFFCA | Arithmetic Progressive | Lossy | Advanced lossy | Rare |
| SOF11 | 0xFFCB | Arithmetic Lossless | Lossless | Advanced lossless | Olympus, Panasonic |

## Lossless JPEG Detection

The extractor distinguishes between lossy and lossless JPEGs by scanning for the SOF marker:
- **Lossless markers**: SOF3, SOF7, SOF11 (all 0xFFC3, 0xFFC7, 0xFFCB)
- **Lossy markers**: SOF0, SOF1, SOF2, SOF5, SOF6, SOF9, SOF10

Lossless JPEGs are intentionally skipped during preview extraction because they're typically full-resolution preservation formats, not previews.

## Marker Segment Types

All JPEG markers (except SOI, EOI) are followed by a length field and data:
- **Length field**: 2 bytes, big-endian, includes the length field itself
- **Data**: Variable, depends on marker type

Common markers:

| Marker | Code | Name | Purpose | Notes |
|--------|------|------|---------|-------|
| SOI | 0xFFD8 | Start of Image | Required first marker | - |
| EOI | 0xFFD9 | End of Image | Required last marker | - |
| SOF* | 0xFFC0-0xFFCB | Start of Frame | Encoding method | Determines lossy/lossless |
| DHT | 0xFFC4 | Huffman Tables | Decoding info | Required for baseline |
| DQT | 0xFFDB | Quantization Tables | Quality tables | Can appear multiple times |
| RST* | 0xFFD0-0xFFD7 | Restart Markers | Segment boundaries | Optional resynchro points |
| COM | 0xFFFE | Comment | User metadata | Added by some cameras |
| APP0 | 0xFFE0 | JFIF Identifier | Format marker | Often first after SOI |
| APP1 | 0xFFE1 | EXIF Data | Camera metadata | Thumbnail embedded here |
| APP13 | 0xFFED | Photoshop/IPTC | Publisher metadata | Canon/Nikon MakerNote |

## Camera-Specific Patterns

### Canon EOS
- **APP0 + APP1 + APP13 stacking**: Canon embeds multiple marker types sequentially
  - Format: SOI → APP0 (JFIF) → APP1 (EXIF) → APP13 (MakerNote) → DHT → SOF0 → SOS
  - Used in: CR2 JPEG thumbnails
  
- **Very long DQT tables**: Canon high-quality JPEGs include maximum-sized quantization tables (67 bytes)
  
- **Comment markers (COM)**: Some Canon firmware versions add metadata comments

### Nikon
- **Arithmetic encoding (SOF9)**: High-end models (D750+) sometimes use patent-restricted arithmetic encoding
  - Still basiley lossy, not lossless
  - Requires special decoders
  
- **APP13 MakerNote**: NRW and NEF files store preview info in APP13
  
- **Multiple RST markers**: Some Nikon firmware uses restart markers for data alignment

### Sony (Alpha)
- **Lossless SOF3 in metadata**: Some models use SOF3 lossless for high-fidelity thumbnails
  - ARW files may contain lossless previews that should be skipped
  
- **Progressive encoding (SOF2)**: Some Alpha models use multi-pass progressive for faster preview display

### Olympus / Panasonic
- **Arithmetic Lossless (SOF11)**: Advanced lossless encoding for ORF/RW2 files
  - Marked as lossless during scanning
  - Should be skipped by preview extractors

### Fujifilm
- **Extended Sequential DCT (SOF1)**: Some X-series cameras use extended variant
  - Technically lossy but uncommon
  
- **JFIF variants**: Extended APP0 headers with unusual lengths

### Leica / Hasselblad
- **Hierarchical encoding**: Scientific cameras may use SOF5/SOF6 differential encoding

## Edge Cases & Malformations

### 1. Zero-Length Marker Segments
**Issue**: DHT or DQT markers with length=0x0000 (0 bytes after length field)
- **Cause**: Buggy firmware, incomplete writes
- **Impact**: Parser must gracefully skip and continue
- **Detection**: Length field < 2 (minimal valid length)

### 2. Incomplete SOF Marker
**Issue**: SOF marker with insufficient bytes for precision/dimensions
- **Cause**: Truncated file, corrupted TIFF region
- **Impact**: Cannot determine image dimensions or encoding type
- **Detection**: Read attempt fails, treat as non-JPEG

### 3. Multiple Consecutive SOF Markers
**Issue**: More than one SOF in a single JPEG bitstream
- **Cause**: Some firmware writes placeholder markers or encoding restarts
- **Impact**: Scan should find first valid SOF and use that
- **Detection**: Stop scan after finding first lossless/lossy distinction

### 4. Reserved Marker Codes
**Issue**: Undefined markers (0xFFF0, 0xFFF1, etc.) appear before real SOF
- **Cause**: Non-conformant encoders or padding
- **Impact**: Parser must skip unknown markers and continue
- **Detection**: Check marker code; skip if unknown

### 5. Concatenated JPEGs
**Issue**: End-Of-Image (EOI, 0xFFD9) followed immediately by new SOI (0xFFD8)
- **Cause**: Multiple preview resolutions stored back-to-back
- **Impact**: Scan may find multiple candidates
- **Known users**: Sony, Fujifilm (thumbnail + preview pattern)

### 6. Missing or Incorrect Segment Lengths
**Issue**: Length field doesn't match actual segment size
- **Cause**: Encoding error, truncated transfer
- **Impact**: Parser overflow or seek misalignment
- **Mitigation**: Use bounded length checks, validate offsets

## Test Cases & Implementation

### Current Coverage (20+ test cases)
The test suite covers:
1. Baseline JPEG with DHT prefix → Should return `false` (lossy)
2. Lossless JPEG (SOF3) with DHT → Should return `true` (lossless)
3. DHT marker alone without SOF → Should return `false` (incomplete)
4. SOF3 marker without prefix → Should return `true` (lossless)
5. APP0 (JFIF) marker → Should return `false` (lossy)
6. Truncated headers < 4 bytes → Should return `false` (too short)
7. Progressive JPEG (SOF2) → Should return `false` (lossy, multi-pass)
8. Arithmetic baseline (SOF9) → Should return `false` (lossy, arithmetic)
9. Arithmetic progressive (SOF10) → Should return `false` (lossy, arithmetic)
10. Hierarchical lossless (SOF11) → Should return `true` (lossless, arithmetic)
11. Differential sequential (SOF5) → Should return `false` (lossy variant)
12. Differential progressive (SOF6) → Should return `false` (lossy variant)
13. Extended sequential (SOF1) → Should return `false` (lossy extended)
14. Lossless arithmetic (SOF7) → Should return `true` (lossless, arithmetic)
15. APP1 (EXIF) before SOF → Should return `false` (still lossy SOF0)
16. Multiple APP markers (Canon pattern) → Should return `false` (lossy SOF0)
17. DQT quantization table before SOF → Should return `false` (still lossy)
18. Restart markers before SOF → Should return `false` (lossy SOF0)
19. Comment marker before SOF → Should return `false` (lossy SOF0)
20. Zero-length DHT segment → Should return `false` (malformed)
21. Arithmetic lossless (SOF11) → Should return `true` (truly lossless)

### Non-Seekable Stream Handling
- Buffering at `JpegExifPreviewExtractor` entry point converts non-seekable to seekable
- Allows all downstream TIFF/EXIF scanning to work transparently
- Prevents stream position corruption for network/pipe sources

## Recommendations for Implementers

1. **Always scan for actual SOF marker** - Don't use 4-byte heuristics
2. **Cache SOF discovery results** - Scanning is relatively expensive
3. **Skip lossless JPEGs** - They're usually non-preview data
4. **Gracefully handle malformed markers** - Use bounded reads and validation
5. **Buffer non-seekable streams early** - Before passing to TIFF parsers
6. **Track stream position carefully** - Restore after seeking attempts
7. **Support all 16 SOF variants** - Even if rare, they occur in real cameras
8. **Test with actual RAW files** - Synthetic test data may miss edge cases

## References

- JPEG Standard (ITU-T T.81 | ISO/IEC 10918-1)
- JFIF Format (JPEG File Interchange Format, RFC 2046)
- EXIF Specification (Camera & Imaging Products Association)
- Raw Image Format Specs: Canon CR2, Sony ARW, Nikon NEF, Fujifilm RAF, etc.

