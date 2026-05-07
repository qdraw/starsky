# JPEG SOF Marker Test Matrix & Camera Manufacturer Reference

## Quick Reference: SOF Markers Used by Cameras

### Common Patterns by RAW Format

#### Canon CR2
| Scenario | SOF Marker | Type | Test Case |
|----------|-----------|------|-----------|
| Standard preview | SOF0 (0xFFC0) | Baseline DCT | Case 1 |
| With APP0+APP1+APP13 | SOF0 | Baseline DCT | Case 20 |
| High-quality | SOF0 + large DQT | Baseline DCT | Case 7 |
| Lossless option | SOF3 (0xFFC3) | Lossless | Case 2 |

#### Sony ARW
| Scenario | SOF Marker | Type | Test Case |
|----------|-----------|------|-----------|
| Standard preview | SOF0 (0xFFC0) | Baseline DCT | Case 1 |
| Progressive fast display | SOF2 (0xFFC2) | Progressive DCT | Case 7 |
| Lossless preserving | SOF3 (0xFFC3) | Lossless | Case 2 |

#### Nikon NEF/NRW
| Scenario | SOF Marker | Type | Test Case |
|----------|-----------|------|-----------|
| Standard preview | SOF0 (0xFFC0) | Baseline DCT | Case 1 |
| High-end (D750+) | SOF9 (0xFFC9) | Arithmetic Baseline | Case 8 |
| With MakerNote | SOF0 + APP13 | Baseline DCT | Case 20 |

#### Olympus ORF
| Scenario | SOF Marker | Type | Test Case |
|----------|-----------|------|-----------|
| Standard preview | SOF0 (0xFFC0) | Baseline DCT | Case 1 |
| Advanced lossless | SOF11 (0xFFCB) | Arithmetic Lossless | Case 4 |

#### Panasonic RW2
| Scenario | SOF Marker | Type | Test Case |
|----------|-----------|------|-----------|
| Standard preview | SOF0 (0xFFC0) | Baseline DCT | Case 1 |
| Arithmetic lossless | SOF11 (0xFFCB) | Arithmetic Lossless | Case 4 |

#### Fujifilm RAF
| Scenario | SOF Marker | Type | Test Case |
|----------|-----------|------|-----------|
| Standard preview | SOF0 (0xFFC0) | Baseline DCT | Case 1 |
| Extended variant | SOF1 (0xFFC1) | Extended Sequential | Case 18 |

#### Leica DNG
| Scenario | SOF Marker | Type | Test Case |
|----------|-----------|------|-----------|
| Standard preview | SOF0 (0xFFC0) | Baseline DCT | Case 1 |
| Hierarchical | SOF5/SOF6 | Differential modes | Cases 11, 12 |

---

## Test Coverage Matrix

```
┌─────────────────────────────────────────────────────────────────────────┐
│              JPEG SOF Detection Test Coverage (20 Cases)                │
├──────┬──────────────────────────┬──────────────────┬──────────────────┤
│ Case │ Marker Sequence          │ Expected Result  │ Camera Type      │
├──────┼──────────────────────────┼──────────────────┼──────────────────┤
│ 1    │ SOI → DHT → SOF0         │ false (lossy)    │ All mainstream   │
│ 2    │ SOI → DHT → SOF3         │ true (lossless)  │ Canon, Sony       │
│ 3    │ SOI → DHT (incomplete)   │ false (no SOF)   │ Malformed/crashed │
│ 4    │ SOI → SOF3 (direct)      │ true (lossless)  │ All              │
│ 5    │ SOI → APP0               │ false (lossy)    │ Standard JPEG     │
│ 6    │ SOI → (too short)        │ false (< 4 bytes)│ Truncated file    │
│ 7    │ SOI → SOF2               │ false (lossy)    │ Mobile, some Sony │
│ 8    │ SOI → SOF9               │ false (lossy)    │ Nikon D750+       │
│ 9    │ SOI → SOF10              │ false (lossy)    │ Rare/scientific   │
│ 10   │ SOI → SOF11              │ true (lossless)  │ Olympus, Panasonic│
│ 11   │ SOI → SOF5               │ false (lossy)    │ Non-standard      │
│ 12   │ SOI → SOF6               │ false (lossy)    │ Non-standard      │
│ 13   │ SOI → SOF1               │ false (lossy)    │ HP, scientific    │
│ 14   │ SOI → SOF7               │ true (lossless)  │ Rare but valid    │
│ 15   │ SOI → DHT(len=0) → SOF0  │ false (corrupt)  │ Old/buggy firmware│
│ 16   │ SOI → RST markers → SOF0 │ false (lossy)    │ Some JPEG encoders│
│ 17   │ SOI → APP0(extended)     │ false (lossy)    │ Canon, Fujifilm   │
│ 18   │ SOI → COM → SOF0         │ false (lossy)    │ Canon firmware    │
│ 19   │ SOI → SOF(incomplete)    │ false (corrupt)  │ Truncated/crashed │
│ 20   │ SOI → APP0/1/13 → SOF0   │ false (lossy)    │ Canon EOS pattern │
└──────┴──────────────────────────┴──────────────────┴──────────────────┘
```

---

## Lossless vs. Lossy Distinction

### TRUE Lossless (Should be skipped by preview extractors)
- **SOF3** (0xFFC3): Lossless Sequential
- **SOF7** (0xFFC7): Lossless with arithmetic coding
- **SOF11** (0xFFCB): Hierarchical lossless with arithmetic

### FALSE Lossless (All lossy variants)
- **SOF0** (0xFFC0): Baseline DCT
- **SOF1** (0xFFC1): Extended Sequential DCT
- **SOF2** (0xFFC2): Progressive DCT
- **SOF5** (0xFFC5): Differential Sequential
- **SOF6** (0xFFC6): Differential Progressive
- **SOF9** (0xFFC9): Arithmetic Baseline
- **SOF10** (0xFFCA): Arithmetic Progressive

---

## Marker Detection Implementation

### Step 1: Locate JPEG SOI
```
0xFF 0xD8 → Start of Image (search for this)
```

### Step 2: Skip Marker Segments Until SOF Found
For each marker after SOI:
- Read marker code (0xXX after 0xFF)
- If SOF marker (0xC0-0xCB), determine encoding:
  - 0xC0, 0xC1, 0xC2, 0xC5, 0xC6, 0xC9, 0xCA → **False** (lossy)
  - 0xC3, 0xC7, 0xCB → **True** (lossless)
  - 0xC4, 0xC8 → **Not SOF** (DHT and DAC markers), skip
- If NOT SOF + NOT DHT/DAC:
  - Read 2-byte length field
  - Skip length-2 bytes
- If EOF or stream end → **False** (no SOF found)

### Step 3: Handle Edge Cases
- Stream < 4 bytes → **False**
- Non-seekable stream → Buffer to memory first
- Out-of-range offset → **False**
- Malformed length field → Conservative **False**

---

## TIFF Marker Variants (Not to be confused with JPEG)

TIFF files (DNG, CR2 metadata sections) use different tags:
- TIFF is a container format storing multiple images and metadata
- JPEG is the compression codec inside TIFF
- SOF markers belong to JPEG only, not TIFF IFD tags

When scanning TIFF regions:
1. Parse TIFF IFD (Image File Directory)
2. For each preview candidate (IFD entry):
   - Extract JPEG offset + length
   - Search within that range for SOF marker
   - Apply lossless detection if needed

---

## Known Firmware Issues

### Canon Firmware
- **Older EOS models**: Sometimes write incomplete DHT segments
- **Newer 5D/1D series**: Always use full APP marker stacks
- **PowerShot**: May use progressive SOF2 for speed

### Nikon Firmware
- **D800+ series**: Arithmetic encoding SOF9 in high-quality mode
- **Older D700/D600**: Always baseline SOF0
- **1-series professional**: Mix of SOF0 and SOF9

### Sony Firmware
- **Early Alpha models**: Inconsistent between SOF0 and SOF3
- **A7 series (2013+)**: Standardized on baseline SOF0
- **A9 series (2017+)**: Progressive SOF2 for preview speed

### Olympus Firmware
- **Early E-system**: Mainly SOF0
- **OM-D series**: SOF11 lossless for high-fidelity thumbnails available

### Panasonic Firmware
- **GH series**: Mix of SOF0 and SOF11 depending on aspect ratio

---

## Performance Considerations

### Scanning Overhead
- **Worst case**: Large TIFF section with many markers before SOF
  - Solution: Cache SOF position after first detection
  - Typical: 30-50 bytes before SOF marker

### Non-Seekable Stream Buffering
- **Overhead**: One full memory copy per extraction
- **Benefit**: Eliminates downstream seeking failures
- **Trade-off**: Justified for network/pipe sources

### Multiple JPEG Detection
- **ConcatenatedJPEG pattern**: EOI (0xFFD9) → SOI (0xFFD8)
- **Handling**: Continue scan after SOI, collect multiple candidates

---

## Future Extensions

Potential additional camera quirks to monitor:
1. **Phase One IQ/XF** - Proprietary hierarchical encoding
2. **Hasselblad H5/H6** - Scientific SOF5/SOF6 variants
3. **Pentax/Ricoh** - Unusual APP marker sequences
4. **DJI/Mobile drones** - Progressive SOF2 for speed
5. **GoPro/Action cams** - Arithmetic encoding variants

