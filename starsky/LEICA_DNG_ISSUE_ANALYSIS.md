# Leica DNG Processing Issue - Comprehensive Analysis

**Date:** April 19, 2026  
**Investigation Focus:** Identifying why Leica DNG files render poorly compared to HUAWEI DNG files

---

## Executive Summary

The Leica DNG rendering issue likely stems from **one or more of the following problems** in the `DngSubsetReader.cs` and processing pipeline:

1. **Black/White Level Array Indexing Mismatch** - Leica uses per-channel (or per-CFA-site) black/white levels that may not be properly resolved
2. **Compression Handling** - Leica files often use Deflate/AdobeDeflate compression which may not decompress correctly
3. **CFA Pattern Detection** - Leica's CFA pattern might not match the fallback RGGB assumption
4. **Color Matrix Illuminant Handling** - D50 vs D65 chromatic adaptation may be miscalibrated for Leica
5. **Tiled vs Stripped Data Layout** - Leica may use tiled encoding not properly handled

---

## Architecture Overview

### Processing Pipeline Stages
```
1. ReadTiff (DngSubsetReader.TryLoad)
   ├─ Parse TIFF header
   ├─ Read IFD0 and find raw IFD (in SubIFDs or IFD0)
   └─ Extract compression, dimensions, black/white levels, CFA pattern

2. Extract Raw Image (TryBuildRawImage)
   ├─ Read pixel data from strips/tiles
   ├─ Decompress if needed (Deflate/AdobeDeflate)
   └─ Decode pixels based on BitsPerSample

3. Normalize (RawNormalization.NormalizeBayerToLinear)
   └─ Apply per-channel black/white level correction

4. Demosaic (BilinearDemosaic)
   └─ Convert Bayer pattern to RGB

5. White Balance (WhiteBalance.ApplyInPlace)
   └─ Apply AsShotNeutral gains

6. Color Matrix (ColorMatrixTransform)
   ├─ Resolve camera→XYZ matrix from ColorMatrix1/ForwardMatrix1
   ├─ Handle D50→D65 chromatic adaptation
   └─ Apply calibration matrices

7. Exposure Compensation (Auto-bright)
8. Tone Mapping (sRGB gamma correction)
```

### Key Data Structures

```csharp
DngRawImage
├─ Bayer ushort[,]              // Raw sensor data
├─ BlackLevel float[]            // Per-channel black point (DNG allows array)
├─ WhiteLevel float[]            // Per-channel white point (DNG allows array)
├─ AsShotNeutral float[]         // White balance multipliers
├─ ColorMatrix1 float[3,3]       // XYZ→Camera or Camera→XYZ (varies by marker)
├─ ForwardMatrix1 float[3,3]     // Optional: direct Camera→XYZ transform
├─ CalibrationIlluminant1 ushort // Illuminant (D65=21, D50=23, etc.)
└─ CfaPattern byte[4]           // CFA layout (e.g., [0,1,1,2] = RGGB)
```

---

## Identified Issues

### 1. **Black/White Level Array Resolution Logic** ⚠️ CRITICAL

**Location:** `DngSubsetReader.cs` lines 494-505, `RawNormalization.cs` lines 51-77

**Issue:** The `ResolveLevel()` function has a **fallback chain** that may not match Leica's metadata structure:

```csharp
private static float ResolveLevel(float[] levels, int cfaIndex, int cfaChannel, float fallback)
{
    if (levels.Length == 0) return fallback;           // ✓ Empty = use default
    if (levels.Length == 1) return levels[0];         // ✓ Scalar applies to all
    if (cfaIndex >= 0 && cfaIndex < levels.Length)
        return levels[cfaIndex];                       // Try per-CFA-site first
    if (cfaChannel >= 0 && cfaChannel < levels.Length)
        return levels[cfaChannel];                     // Fallback: per-color
    return levels[^1];                                 // Last resort: last value
}
```

**Leica-Specific Problem:**
- Leica files might store black/white levels as **per-channel values** (e.g., 4 values for RGGB)
- The current logic tries CFA-site indexing first, which indexes into a 2×2 Bayer pattern (0-3)
- If Leica stores values as [R, G, B, B] (4 color channels), the per-CFA-site lookup may grab wrong values:
  - CFA site 0 (R at [0,0]) → levels[0] ✓
  - CFA site 1 (G at [0,1]) → levels[1] ✓
  - CFA site 2 (G at [1,0]) → levels[2] ✗ Should be same G as site 1, gets B instead
  - CFA site 3 (B at [1,1]) → levels[3] ✓

**Impact:** Green channel normalization is **incorrectly scaled**, causing color shifts.

**Solution:** Detect array length to disambiguate:
```csharp
// If length == 4 and CFA is RGGB, use per-channel indexing
// If length == 4 and CFA interpretation needs per-site, use per-site
```

---

### 2. **Compression Type Validation** ⚠️ MEDIUM

**Location:** `DngSubsetReader.cs` lines 377-387

**Current Code:**
```csharp
if (!TryGetUnsigned(input, littleEndian, ifd, TagCompression, out var compression))
{
    error = "Only uncompressed DNG is supported in the subset reader";
    return false;
}

if (compression is not (CompressionUncompressed or CompressionDeflate or CompressionAdobeDeflate))
{
    error = "Only uncompressed DNG is supported in the subset reader";
    return false;  // ← ERROR MESSAGE CONTRADICTS LOGIC!
}
```

**Issue:**
1. **Error message is misleading** - Says "only uncompressed" but code accepts Deflate/AdobeDeflate
2. **Leica files are often Deflate-compressed** (lines 25 in test file: "16bit compressed (3_2).dng")
3. The `Inflate()` function (lines 892-918) has **two-tier decompression logic** that might fail silently

**Deflate Decompression Fallback:**
```csharp
private static byte[]? Inflate(byte[] compressed)
{
    try
    {
        using var input = new MemoryStream(compressed);
        using var zlib = new ZLibStream(input, CompressionMode.Decompress);  // zlib format
        // ...
    }
    catch
    {
        try
        {
            using var deflate = new DeflateStream(input, CompressionMode.Decompress);  // raw DEFLATE
            // ...
        }
        catch { return null; }  // Silent failure!
    }
}
```

**Problem:** If both decompression attempts fail, returns `null` and the pixel decoding fails without clear diagnostics.

**Leica-Specific:** Leica's compression might use **zlib wrapper format** that the `DeflateStream` can't handle, or vice versa.

---

### 3. **CFA Pattern Fallback** ⚠️ MEDIUM

**Location:** `DngSubsetReader.cs` lines 532-535 and `RawNormalization.cs` lines 41-42

**Current Fallback:**
```csharp
var cfaPattern = TryGetByteArray(input, littleEndian, ifd, TagCfaPattern, out var cfa) &&
                 cfa.Length >= 4
    ? cfa.Take(4).ToArray()
    : new byte[] { 0, 1, 1, 2 };  // ← HARDCODED RGGB
```

**Issue:** If Leica's CFA pattern tag is missing or malformed, defaults to RGGB which might be wrong.

**Common CFA Patterns:**
- `[0, 1, 1, 2]` - RGGB (most common)
- `[0, 1, 1, 2]` - GRBG (some cameras)
- `[0, 2, 1, 2]` - GBRG (less common)

**Impact:** If Leica uses GRBG or GBRG, the demosaic will produce color shifts and artifacts.

**Test:** Check Leica test files:
```
/Users/dion/data/testcontent/raws/Leica - M (Typ 240) - 16bit 16bit compressed (3_2).dng
/Users/dion/data/testcontent/raws/leica_cl_01.dng
/Users/dion/data/testcontent/raws/RAW_LEICA_M8.dng
```

---

### 4. **Color Matrix Illuminant Handling** ⚠️ HIGH

**Location:** `DngSubsetReader.cs` lines 566-579, `ColorMatrixTransform.cs` lines 26-59

**Issue:** Leica cameras (especially rangefinders like M8, M240) often have **D50-based color matrices**:

```csharp
// From DngSubsetReader
var calibrationIlluminant1 = 
    TryGetUnsigned(input, littleEndian, ifd, TagCalibrationIlluminant1, out var illumRaw)
        ? (ushort)illumRaw
        : 0;  // ← Falls back to 0 (UNKNOWN) if not present!
```

**From ColorMatrixTransform:**
```csharp
if (calibrationIlluminant != 21)  // 21 = D65
{
    cameraToXyz = Multiply3X3(XyzD50ToD65, cameraToXyz);  // Apply adaptation
}
```

**Leica Problem:**
1. Leica M-series cameras use **D50 illuminant** (illuminant code 23, not 21)
2. If `CalibrationIlluminant1` is missing or set to 0, the D50→D65 conversion is **skipped**
3. Result: **Magenta/red color cast** because XYZ values are D50-based but treated as D65

**Typical Illuminant Values:**
- 17 = D55
- 21 = D65 (default for most digital cameras)
- 23 = D50 (Leica, many rangefinders)
- 0 = Unknown/Not set

---

### 5. **Tiled vs Strip-Based Image Handling** ⚠️ MEDIUM

**Location:** `DngSubsetReader.cs` lines 390-450, `TryReadPixelsTiled` lines 813-890

**Issue:** Leica files may use **tiled storage** instead of strip-based:

```csharp
var hasTiles = TryGetUnsignedArray(input, littleEndian, ifd, TagTileOffsets,
    out var tileOffsets) && tileOffsets.Length > 0;
var hasStrips = TryGetUnsignedArray(input, littleEndian, ifd, TagStripOffsets,
    out var stripOffsets) && stripOffsets.Length > 0;

if (!hasTiles && !hasStrips)
{
    error = "Missing tile or strip data pointers";
    return false;
}
```

**Tiled Pixel Reading Issue:**
```csharp
// Line 824
if (offsets.Count != tilesAcross * tilesDown)
{
    return false;  // ← Validation too strict?
}
```

**Potential Leica Issue:** If tile dimensions don't evenly divide the image dimensions, the calculation might be off by 1.

Example:
- Image: 5520 × 3680
- Tile Size: 512 × 512
- Expected tiles: ceil(5520/512) × ceil(3680/512) = 11 × 8 = 88 tiles
- If Leica stores 88 tiles but calculation gives 11 × 7 = 77, validation fails

---

### 6. **Bits Per Sample Edge Cases** ⚠️ LOW

**Location:** `DngSubsetReader.cs` lines 462-472

**Current Support:**
```csharp
if (bitsPerSample is not (8 or 10 or 12 or 14 or 16))
{
    error = $"Unsupported bits per sample: {bitsPerSample}";
    return false;
}
```

**Leica-Specific:** Leica M8 (2008) used **12-bit RAW**, which is supported. Newer M-series use 12-bit or 16-bit.

**Not a blocker** but worth noting.

---

### 7. **WhiteBalance Gain Calculation from AsShotNeutral** ⚠️ MEDIUM

**Location:** Need to check WhiteBalance.cs implementation

**Theory:** If `AsShotNeutral` is wrong or missing, white balance gains will be incorrect, causing color shifts.

**Leica Issue:** If AsShotNeutral is stored in EXIF IFD (not raw IFD), the current code might not find it.

```csharp
// From DngSubsetReader lines 604-629
private static bool TryGetAsShotNeutral(Stream input, bool littleEndian, IfdDirectory rawIfd,
    IfdDirectory ifd0, out float[] neutral)
{
    if (TryReadNeutralFromIfd(input, littleEndian, rawIfd, out neutral))
        return true;
    
    if (TryReadNeutralFromIfd(input, littleEndian, ifd0, out neutral))
        return true;
    
    if (TryGetUnsigned(input, littleEndian, ifd0, TagExifIfd, out var exifIfdOffset))
    {
        var exifIfd = ReadIfd(input, exifIfdOffset, littleEndian);
        if (exifIfd != null && TryReadNeutralFromIfd(input, littleEndian, exifIfd, out neutral))
            return true;
    }
    
    return false;  // ← Falls back to [1, 1, 1]
}
```

**Good:** It checks three places including EXIF. But if a Leica file stores AsShotNeutral in an unexpected location, white balance will be neutral (no correction).

---

## HUAWEI vs Leica Comparison

### HUAWEI DNG (Test File Renders Well)
- **Source:** `/Users/dion/data/testcontent/raws/HUAWEI - EVA-AL00 - 16bit (4_3).dng`
- **Format:** 16-bit, uncompressed (typically)
- **CFA:** Standard RGGB [0, 1, 1, 2]
- **Illuminant:** D65 (21) - standard smartphone illuminant
- **Black/White Levels:** Likely scalar or standard per-channel
- **Color Matrix:** D65-based, no adaptation needed

### Leica DNG (Test Files Render Poorly)
- **Sources:**
  - `Leica - M (Typ 240) - 16bit 16bit compressed (3_2).dng`
  - `leica_cl_01.dng`
  - `RAW_LEICA_M8.dng`
- **Format:** 12-16 bit, **often Deflate-compressed**
- **CFA:** May deviate from RGGB
- **Illuminant:** D50 (23) - legacy rangefinder illuminant
- **Black/White Levels:** Per-CFA-site, complex array handling
- **Color Matrix:** D50-based, requires chromatic adaptation
- **Tiling:** May use tiled storage (512×512 or 256×256 tiles)

**Key Differences:**
1. **Illuminant mismatch** - D50 vs D65
2. **Compression** - Leica uses Deflate, HUAWEI typically doesn't
3. **Metadata richness** - Leica may have sparse metadata

---

## Root Cause Hypothesis

**PRIMARY SUSPECT:** Black/White Level Array Misalignment + Illuminant Fallback

When processing a Leica file:
1. `BlackLevel` array is stored (e.g., 4 values)
2. `RawNormalization.ResolveLevel()` indexes by CFA site
3. For green channels (CFA sites 1 and 2), it **grabs wrong array indices**
4. Additionally, if `CalibrationIlluminant1` is 0 or missing, **D50→D65 adaptation is skipped**
5. Result: **Dark, color-cast image with magenta/red tint**

---

## Recommended Fixes

### Priority 1: Fix Black/White Level Resolution
```csharp
// In DngSubsetReader.cs or RawNormalization.cs
// Improve ResolveLevel() to detect array interpretation:

private static float ResolveLevel(float[] levels, int cfaIndex, int cfaChannel, 
    byte[] cfaPattern, float fallback)
{
    if (levels.Length == 0) return fallback;
    if (levels.Length == 1) return levels[0];
    
    // If length == 4 and CFA pattern is RGGB, interpret as per-channel
    if (levels.Length == 4 && IsStandardRggb(cfaPattern))
    {
        // Map CFA site to color index
        int colorIndex = CfaColorIndexFromSite(cfaPattern, cfaIndex);
        return levels[colorIndex];
    }
    
    // Per-CFA-site indexing
    if (cfaIndex >= 0 && cfaIndex < levels.Length)
        return levels[cfaIndex];
    
    // Per-color indexing
    if (cfaChannel >= 0 && cfaChannel < levels.Length)
        return levels[cfaChannel];
    
    return levels[^1];
}

private static int CfaColorIndexFromSite(byte[] cfaPattern, int cfaIndex)
{
    // Return color index (0=R, 1=G, 2=B) for given CFA site
    return cfaPattern[cfaIndex];
}
```

### Priority 2: Improve Illuminant Handling
```csharp
// In DngSubsetReader.cs
// Don't default illuminant to 0; try to infer from camera maker

var calibrationIlluminant1 = 
    TryGetUnsigned(input, littleEndian, ifd, TagCalibrationIlluminant1, out var illumRaw)
        ? (ushort)illumRaw
        : TryGetCameraDefaultIlluminant(ifd0)  // New helper
        : (ushort)21;  // Default D65, not 0

private static ushort? TryGetCameraDefaultIlluminant(IfdDirectory ifd0)
{
    // Check camera maker in TIFF tags
    // Leica → 23 (D50)
    // Sony → 21 (D65)
    // etc.
}
```

### Priority 3: Better Compression Diagnostics
```csharp
// In DngSubsetReader.cs - Inflate() function
private static byte[]? Inflate(byte[] compressed, out string? diagnostics)
{
    diagnostics = null;
    try
    {
        using var input = new MemoryStream(compressed);
        using var zlib = new ZLibStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream();
        zlib.CopyTo(output);
        return output.ToArray();
    }
    catch (Exception ex)
    {
        diagnostics = $"zlib failed: {ex.Message}";
        try
        {
            using var input = new MemoryStream(compressed);
            using var deflate = new DeflateStream(input, CompressionMode.Decompress);
            using var output = new MemoryStream();
            deflate.CopyTo(output);
            diagnostics += "; DeflateStream succeeded";
            return output.ToArray();
        }
        catch (Exception ex2)
        {
            diagnostics += $"; DeflateStream failed: {ex2.Message}";
            return null;
        }
    }
}
```

### Priority 4: Add Diagnostic Metadata Logging
```csharp
// In RawDngRealFilesFlowTests or a new diagnostic class
// Log full metadata for failed files:
// - Black/White level arrays (raw values)
// - CFA pattern (hex)
// - Calibration illuminant
// - Color matrix values
// - Compression type
```

---

## Testing Strategy

### 1. Create Unit Tests for Array Resolution
```csharp
[TestMethod]
public void ResolveLevel_WithLeicaFourValueArray_MapsCorrectly()
{
    // RGGB pattern with 4 values
    var levels = new float[] { 100, 50, 50, 100 };  // R, G1, G2, B
    var cfaPattern = new byte[] { 0, 1, 1, 2 };
    
    // Site 0 (R) should get levels[0]
    Assert.AreEqual(100, ResolveLevel(levels, 0, 0, cfaPattern, 0));
    
    // Site 1 (G) should get levels[1]
    Assert.AreEqual(50, ResolveLevel(levels, 1, 1, cfaPattern, 0));
    
    // Site 2 (G) should get levels[1] (same green), NOT levels[2]
    Assert.AreEqual(50, ResolveLevel(levels, 2, 1, cfaPattern, 0));
    
    // Site 3 (B) should get levels[3]
    Assert.AreEqual(100, ResolveLevel(levels, 3, 2, cfaPattern, 0));
}
```

### 2. Create Test DNG Files
Generate minimal DNG files with:
- Leica-style metadata (D50 illuminant, compressed, tiled)
- Known pixel values
- Verify rendering output matches expected

### 3. Run Real File Tests
Execute `RawDngRealFilesFlowTests` with diagnostic logging to identify failure patterns

---

## Conclusion

The Leica DNG rendering issue is likely caused by a **combination** of:
1. **Black/White level array misindexing** (PRIMARY)
2. **Illuminant fallback to 0 instead of inferring D50** (HIGH)
3. **Compression decompression ambiguity** (MEDIUM)
4. **CFA pattern fallback assumptions** (MEDIUM)

Fixing the black/white level resolution logic and adding proper illuminant handling should resolve most Leica rendering issues.

---

## Files to Modify

1. **`DngSubsetReader.cs`**
   - Improve black/white level array handling
   - Add illuminant inference
   - Better error messages for compression

2. **`RawNormalization.cs`**
   - Improve `ResolveLevel()` to handle color-vs-site ambiguity

3. **`ColorMatrixTransform.cs`**
   - Add safety check for unknown illuminants (default to D65)

4. **Test Files**
   - Add Leica-specific test cases
   - Improve diagnostic logging


