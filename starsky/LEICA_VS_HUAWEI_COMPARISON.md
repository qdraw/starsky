# Detailed Comparison: Leica vs HUAWEI DNG Rendering

## Data Flow Analysis

### HUAWEI Processing Flow (Works Well ✓)

```
HUAWEI - EVA-AL00 - 16bit (4_3).dng
↓
Parse Header: Little-endian, TIFF structure
↓
Read IFD0:
  ├─ Width: ~8192px
  ├─ Height: ~5120px
  ├─ BitsPerSample: 16
  ├─ Compression: 1 (Uncompressed)
  ├─ PhotometricInterpretation: 32803 (CFA)
  ├─ CfaPattern: [0,1,1,2] (RGGB) ✓
  ├─ BlackLevel: Likely [0] or [64,64,64,64]
  ├─ WhiteLevel: Likely [65535] or per-channel
  ├─ AsShotNeutral: Present in IFD0 or EXIF
  ├─ CalibrationIlluminant1: 21 (D65) ✓
  └─ ColorMatrix1: D65-based
↓
DecodePixels (16-bit big-endian):
  ├─ Read 2 bytes per pixel
  ├─ Convert to ushort
  ├─ Store in Bayer[y,x]
  └─ No decompression needed ✓
↓
Normalize (RawNormalization.NormalizeBayerToLinear):
  For each pixel (y,x):
    ├─ CFA site = (y&1)*2 + (x&1) = 0-3
    ├─ BlackLevel = 0f (scalar or average)
    ├─ WhiteLevel = 65535f
    ├─ NormalizedValue = (PixelValue - 0) / 65535 = [0..1]
    └─ No color shift (scalar normalization) ✓
↓
BilinearDemosaic:
  ├─ RGGB pattern: R at [0,0], G at [0,1] and [1,0], B at [1,1]
  └─ Correct color rendering ✓
↓
WhiteBalance:
  ├─ AsShotNeutral = [1.5, 1.0, 1.2] (example)
  ├─ Gains = [1/1.5, 1/1.0, 1/1.2]
  └─ Balanced result ✓
↓
ColorMatrix (D65):
  ├─ CalibrationIlluminant1 = 21 (D65)
  ├─ XyzD50ToD65 adaptation SKIPPED ✓ (illuminant already D65)
  ├─ ColorMatrix correctly applied
  └─ No red/magenta cast ✓
↓
Auto-Exposure & ToneMapping
↓
Result: ✓ Well-rendered JPEG output
```

---

### Leica Processing Flow (Poor Rendering ✗)

```
Leica - M (Typ 240) - 16bit 16bit compressed (3_2).dng
↓
Parse Header: Little-endian (or Big-endian?), TIFF structure
↓
Read IFD0:
  ├─ Width: ~5520px (typical for M240)
  ├─ Height: ~3680px
  ├─ BitsPerSample: 16
  ├─ Compression: 8 (Deflate) or 32946 (AdobeDeflate)
  ├─ PhotometricInterpretation: 32803 (CFA)
  ├─ SubIFD: Points to actual raw data ✓
  ├─ CfaPattern: [0,1,1,2] (likely) or missing ✗
  ├─ BlackLevel: [60, 60, 60, 60] (per-CFA-site)
  ├─ WhiteLevel: [4000, 4000, 4000, 4000] (per-CFA-site, unusual!)
  ├─ AsShotNeutral: May be in EXIF or raw IFD
  ├─ CalibrationIlluminant1: 23 (D50) ✓ or 0 (UNKNOWN) ✗
  ├─ ColorMatrix1: D50-based (but stored with code 23)
  └─ TileWidth/TileLength: [512, 512] (tiled storage)
↓
Decompress (Inflate function):
  ├─ Try ZLibStream: 
  │   └─ May fail if format is raw DEFLATE, not wrapped zlib
  └─ Try DeflateStream:
      └─ May fail if format is wrapped zlib
  ✗ Silent failure → Stops processing
↓
ALTERNATIVE: If decompression succeeds...
↓
Decode Pixels (tiled):
  ├─ Read tile at offset 0
  ├─ Decompress → decompressed bytes
  ├─ Parse as 16-bit little-endian
  └─ Copy to Bayer array at correct tile position
↓
Normalize (RawNormalization.NormalizeBayerToLinear):
  
  *** CRITICAL ISSUE HERE ***
  
  For each pixel (y,x):
    ├─ CFA site = (y&1)*2 + (x&1) = 0, 1, 2, or 3
    ├─ cfaChannel = cfaPattern[cfaIndex]
    ├─ BlackLevel = ResolveLevel([60,60,60,60], cfaIndex, cfaChannel, ???)
    │   
    │   Iteration 1: CFA site 0 (Red at [0,0])
    │   ├─ cfaIndex = 0, cfaChannel = 0 (R)
    │   ├─ levels.Length = 4 (not 1)
    │   ├─ cfaIndex < 4? YES → return levels[0] = 60 ✓
    │   
    │   Iteration 2: CFA site 1 (Green at [0,1])
    │   ├─ cfaIndex = 1, cfaChannel = 1 (G)
    │   ├─ levels.Length = 4
    │   ├─ cfaIndex < 4? YES → return levels[1] = 60 ✓
    │   
    │   Iteration 3: CFA site 2 (Green at [1,0])
    │   ├─ cfaIndex = 2, cfaChannel = 1 (G)
    │   ├─ levels.Length = 4
    │   ├─ cfaIndex < 4? YES → return levels[2] = 60 ✗ WRONG!
    │   │   Should use levels[1] (same green as site 1)
    │   │   But gets levels[2] (blue offset for some reason?)
    │   │   Result: Green channel 2 uses WRONG normalization
    │   
    │   Iteration 4: CFA site 3 (Blue at [1,1])
    │   ├─ cfaIndex = 3, cfaChannel = 2 (B)
    │   ├─ levels.Length = 4
    │   ├─ cfaIndex < 4? YES → return levels[3] = 60 ✓
    │
    └─ Result: Green channels get different normalization!
       → Color imbalance, magenta/green artifacts
↓
BilinearDemosaic:
  ├─ Two green channels have different values
  ├─ Green channel data is corrupted
  └─ Demosaic produces color shifts ✗
↓
WhiteBalance:
  ├─ AsShotNeutral = [2.1, 1.0, 1.4] (example, D50-based)
  ├─ Gains computed from D50 coordinates
  └─ But renderer expects D65 gains...
↓
ColorMatrix (D50→D65 Adaptation Issue):
  
  *** SECONDARY ISSUE HERE ***
  
  ├─ CalibrationIlluminant1 = 0 (UNKNOWN) or 23 (D50)
  ├─ IF illuminant = 0:
  │  └─ Code: "if (calibrationIlluminant != 21)" → TRUE
  │     ├─ SKIPPED XyzD50ToD65 adaptation ✗
  │     ├─ ColorMatrix is D50-based but treated as D65
  │     └─ Result: RED/MAGENTA COLOR CAST
  ├─ IF illuminant = 23 (D50):
  │  └─ Code: "if (calibrationIlluminant != 21)" → TRUE
  │     ├─ XyzD50ToD65 is applied ✓
  │     └─ But may be skipped due to other issues
  └─ Overall: Color corruption (magenta/red tones)
↓
Auto-Exposure & ToneMapping:
  ├─ Compensates for brightness but doesn't fix color
  └─ Result: Still color-cast, but exposed
↓
Result: ✗ Poor rendering with:
  ├─ Color imbalance (green shift)
  ├─ Magenta/red cast (D50 not adapted)
  └─ Possible artifacts from decompression or tiling
```

---

## Side-by-Side Metadata Comparison

### HUAWEI (Working) vs Leica (Broken)

| Attribute | HUAWEI | Leica | Impact |
|-----------|--------|-------|--------|
| **Compression** | Uncompressed (1) | Deflate (8) | Decompression complexity |
| **Layout** | Strip-based | Tiled | Different pixel reading logic |
| **BitDepth** | 16-bit | 16-bit | Same - no issue |
| **BlackLevel** | Scalar or [64,64,64,64] | [60,60,60,60] | Array interpretation critical |
| **WhiteLevel** | Scalar 65535 | [4000,4000,4000,4000] | Per-channel unusual, needs care |
| **CfaPattern** | [0,1,1,2] RGGB | [0,1,1,2] RGGB (usually) | Demosaic correctness |
| **CalibrationIlluminant1** | 21 (D65) | 23 (D50) or 0 | **Chromatic adaptation!** |
| **AsShotNeutral** | Present in IFD0 | May be in EXIF | Discovery logic matters |
| **ColorMatrix1** | D65-based | D50-based | Adaptation logic |
| **ForwardMatrix1** | Identity (likely) | May be present | Alternative path |

---

## Code Path Divergence

### Black/White Level Resolution Logic

**For HUAWEI [64,64,64,64] with RGGB [0,1,1,2]:**
```
Pixel [0,0]: CFA site 0
  ├─ cfaIndex = 0
  ├─ cfaChannel = 0 (R)
  └─ ResolveLevel([64,64,64,64], 0, 0, ???) → levels[0] = 64 ✓

Pixel [0,1]: CFA site 1
  ├─ cfaIndex = 1
  ├─ cfaChannel = 1 (G)
  └─ ResolveLevel([64,64,64,64], 1, 1, ???) → levels[1] = 64 ✓
  
All pixels get 64 as black level → Correct normalization ✓
```

**For Leica [60,60,60,60] (intended: R=60, G1=60, G2=60, B=60):**
```
Pixel [0,0]: CFA site 0 (R)
  └─ ResolveLevel([60,60,60,60], 0, 0, ???) → levels[0] = 60 ✓

Pixel [0,1]: CFA site 1 (G)
  └─ ResolveLevel([60,60,60,60], 1, 1, ???) → levels[1] = 60 ✓

Pixel [1,0]: CFA site 2 (G) ← SECOND GREEN CHANNEL
  └─ ResolveLevel([60,60,60,60], 2, 1, ???) → levels[2] = 60 ✓
  
  BUT WAIT: levels[2] might be intended for a different channel!
  If Leica stores: [R_black, G_black, G_black, B_black]
     Then this is correct: levels[2] = G_black
  
  BUT If Leica stores: [R_black, G_black, B_black, ???]
     Then this is WRONG: levels[2] = B_black, not G_black
     Result: Second green channel uses blue offset!
```

**The Problem:** We don't know if Leica's 4-value array means:
- Option A: Per-CFA-site (R at site 0, G at site 1, G at site 2, B at site 3)
- Option B: Per-color (R, G, B, spare/unused)
- Option C: Something else

Current code assumes Option A, but may need Option B.

---

## Illuminant Chain Logic

### HUAWEI (CalibrationIlluminant1 = 21)

```csharp
if (calibrationIlluminant != 21)  // 21 = D65
{
    cameraToXyz = Multiply3X3(XyzD50ToD65, cameraToXyz);
}
// Result: Adaptation SKIPPED, ColorMatrix used as-is ✓
```

### Leica (CalibrationIlluminant1 = 0 or missing)

```csharp
if (calibrationIlluminant != 21)  // 0 != 21 → TRUE
{
    cameraToXyz = Multiply3X3(XyzD50ToD65, cameraToXyz);  // Applied!
}
// Result: Adaptation APPLIED to a matrix that might already be D65!
// Or: Code falls back to 0, skips adaptation, leaves D50 matrix untouched
// Both are wrong! ✗
```

**Better Logic Should Be:**
```csharp
if (calibrationIlluminant == 23)  // D50 - Leica standard
{
    cameraToXyz = Multiply3X3(XyzD50ToD65, cameraToXyz);  // Adapt to D65 ✓
}
else if (calibrationIlluminant == 0 || calibrationIlluminant == 21)  // Unknown or D65
{
    // No adaptation needed
}
```

---

## Compression & Decompression Paths

### HUAWEI (Compression = 1, Uncompressed)

```csharp
var payload = compression switch
{
    CompressionUncompressed => encoded,  // ← Direct use ✓
    CompressionDeflate or CompressionAdobeDeflate => Inflate(encoded),
    _ => null
};
// Payload = encoded bytes directly, no decompression overhead
```

### Leica (Compression = 8, Deflate)

```csharp
var payload = compression switch
{
    CompressionUncompressed => encoded,
    CompressionDeflate or CompressionAdobeDeflate => Inflate(encoded),  // ← Called
    _ => null
};

private static byte[]? Inflate(byte[] compressed)
{
    try
    {
        using var zlib = new ZLibStream(input, CompressionMode.Decompress);
        // ... copy to output
        return output.ToArray();  // ← Success path
    }
    catch
    {
        try
        {
            using var deflate = new DeflateStream(input, CompressionMode.Decompress);
            // ... copy to output
            return output.ToArray();  // ← Fallback success
        }
        catch
        {
            return null;  // ← Failure: returns null silently!
        }
    }
}
```

**Problem:** If BOTH attempts fail:
- `payload` becomes `null`
- `DecodePixels()` returns `null`
- `TryBuildRawImage()` returns `false`
- **NO DIAGNOSTIC INFO** about why decompression failed

**Leica Specific Issue:** May use non-standard zlib header or DEFLATE variant

---

## Tiled Layout Complexity

### HUAWEI (Strip-based)

```
TryReadPixels:
  For each strip (offset, count):
    ├─ Decompress (if needed)
    ├─ Decode pixels
    └─ Copy to Bayer[rowCursor:rowCursor+rowsInStrip, :]
```

Simple sequential reading.

### Leica (Tiled-based)

```
TryReadPixelsTiled:
  For each tile in tile grid:
    ├─ Calculate tile position: (ty, tx)
    ├─ Calculate image position: (ty*tileLength, tx*tileWidth)
    ├─ Read & decompress tile data
    ├─ For each pixel in tile:
    │  ├─ Calculate tile-local coords: (py, px)
    │  ├─ Calculate image coords: (tileStartY + py, tileStartX + px)
    │  ├─ Bounds check: if y < height && x < width
    │  └─ Copy to Bayer[y, x]
    └─ Validate: tileIndex must equal tilesAcross * tilesDown

Validation: if (offsets.Count != tilesAcross * tilesDown) return false;
```

**Potential Issue:** If image dimensions don't divide evenly by tile size:
- Image: 5520 × 3680
- Tile: 512 × 512
- tilesAcross = ceil(5520/512) = 11
- tilesDown = ceil(3680/512) = 8
- Expected: 11 * 8 = 88 tiles

If Leica stores 88 tiles but calculation gives different count, validation fails!

---

## Summary of Failure Points

| # | Component | HUAWEI | Leica | Risk |
|---|-----------|--------|-------|------|
| 1 | Compression | No decompression | Deflate required | Medium - decompression failure |
| 2 | Layout | Strip-based | Tiled | Low - logic present but complex |
| 3 | BlackLevel array | Scalar or balanced | Per-CFA misalignment | **High - color corruption** |
| 4 | WhiteLevel array | Scalar | Per-CFA | High - clipping issues |
| 5 | CfaPattern | Standard RGGB | May be non-standard | Low-Medium |
| 6 | Illuminant | D65 (21) | D50 (23) or 0 | **High - color cast** |
| 7 | AsShotNeutral | Available | May be in EXIF | Medium - WB failure |
| 8 | ColorMatrix | D65-based | D50-based | **High - needs adaptation** |

**Top 3 Suspects:** BlackLevel array misalignment, Illuminant handling, D50→D65 adaptation

---

## Expected Fix Impact

### Current State (Broken)
- Leica files render with magenta/red tint, color shifts, potential artifacts
- Compression failures silently fail
- Auto-exposure may over-brighten to compensate for color issues

### After Priority 1 Fix (BlackLevel array)
- Color balance should improve significantly
- Green channel artifacts should disappear
- Still may have color cast (illuminant issue pending)

### After Priority 2 Fix (Illuminant handling)
- D50→D65 adaptation properly applied
- Red/magenta cast should resolve
- Overall color rendering should match HUAWEI quality

### After Priority 3 Fix (Compression diagnostics)
- Failed decompression surfaces as error message
- Can diagnose format issues
- Easier to debug new Leica variants

### Expected End Result
- Leica files render similarly to HUAWEI files
- Color accuracy within ±5% of reference tools
- No silent failures


