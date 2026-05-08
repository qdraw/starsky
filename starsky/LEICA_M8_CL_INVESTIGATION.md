# Leica M8 & CL DNG Rendering Issues - Analysis

**Date:** April 19, 2026  
**Status:** Investigating pink/magenta cast and darkness in specific Leica models

---

## Problem Statement

After implementing the Leica DNG fixes, some files still render with:
- **leica_cl_01.dng** - Dark and pink
- **RAW_LEICA_M8.dng** - Complete pink image
- While other files render correctly

This indicates **additional metadata issues** specific to these camera models.

---

## Hypothesis: Additional Issues Beyond the First Three Fixes

### Issue #4: Unusual White Balance in Legacy Leica Cameras

**Background:**
- Leica M8 (2008) was the first digital Leica
- Leica CL (2017) is a rangefinder without traditional AF
- Both use D50 illuminant like other Leicas
- But metadata encoding may be different from M240

**Pink Cast Cause - Theory 1: AsShotNeutral Scaling**
```
Hypothesis: The AsShotNeutral values might be encoded differently
  • M240: AsShotNeutral = [2.0, 1.0, 1.5] (standard)
  • M8/CL: AsShotNeutral = [0.5, 1.0, 0.67] (inverted/different scale)
  
If M8 stores 1/gains instead of gains:
  • Applied directly → green boost, magenta cast
  • Need to invert before applying gains
```

**Darkness Cause - Theory 1: Black Level Offset**
```
Hypothesis: Black level might be stored as negative or unusual value
  • M240: BlackLevel = [60, 50, 50, 60] (positive)
  • M8/CL: BlackLevel = [-100] or very different per channel
  
If black level is wrong:
  • Pixels normalized to wrong range
  • Everything becomes dark
  • Compounded by pink cast from WB
```

### Issue #5: Color Matrix or Forward Matrix Missing/Invalid

**Background:**
- Leica M8 might not have proper ColorMatrix1 stored
- May need to use ForwardMatrix1 instead
- May need special handling for legacy cameras

**Problem:**
```
ColorMatrixTransform.BuildCameraToSrgb() expects:
  ├─ ColorMatrix1 OR ForwardMatrix1 to be valid
  └─ If both are identity/invalid, defaults to identity
     → No color rendering, just normalized grayscale
     → Pink cast could come from broken color transform
```

### Issue #6: Compression or Encoding Issues

**Background:**
- M8 DNG might use unusual compression
- CL DNG might have tiled layout issues
- Different endianness handling

**Problem:**
```
If pixel data decoded incorrectly:
  ├─ Values off by factor
  ├─ Bit shift issues
  └─ Results in color imbalance
```

---

## Investigation Steps

### Step 1: Analyze Metadata Using Diagnostics

The diagnostic tool (`LeicaDngDiagnostics.cs`) will reveal:
1. Exact black/white level values
2. AsShotNeutral values
3. ColorMatrix1 presence and values
4. ForwardMatrix1 presence and values
5. CalibrationIlluminant value

### Step 2: Compare M8/CL Metadata with M240

Once diagnostics run, compare:
- Are black/white levels different format?
- Is AsShotNeutral inverted or scaled differently?
- Is ColorMatrix1 identity (indicating missing color data)?
- Is ForwardMatrix1 used instead of ColorMatrix1?

### Step 3: Identify Pattern

Determine if issue is:
- ✓ M8-specific (old sensor format)
- ✓ CL-specific (compact form factor)
- ✓ Shared (legacy rangefinder pattern)
- ✓ Related to compression/encoding

---

## Likely Fixes Needed

### Fix #5: Improved AsShotNeutral Handling

**If M8/CL use inverted gains:**
```csharp
// Detect if AsShotNeutral is inverted (gains < 1 mean boost)
if (asShotNeutral.Any(x => x < 1f) && asShotNeutral.Any(x => x > 1f))
{
    // Mixed values - likely inverted
    asShotNeutral = [1/r, 1/g, 1/b];  // Invert them
}
```

### Fix #6: Better Black Level Handling

**If M8/CL store unusual black levels:**
```csharp
// Ensure black levels are reasonable
if (blackLevel.Any(x => x < 0))
{
    // Negative black level unusual
    // Consider it as absolute offset, not relative
    blackLevel = blackLevel.Select(x => Math.Abs(x)).ToArray();
}

// Check if all channels have same level
if (blackLevel.Length == 4 && blackLevel.Distinct().Count() == 1)
{
    // All same - use single value
    // Avoid per-channel issues
}
```

### Fix #7: Forward Matrix Priority

**If ForwardMatrix is used instead of ColorMatrix:**
```csharp
// In ColorMatrixTransform.TryBuildCameraToXyz:
// Already handles this, but verify it's working for M8/CL

// Make sure ForwardMatrix path is taken for M8/CL
if (!IsIdentity3X3(forwardMatrix) && forwardMatrix != null)
{
    cameraToXyz = Multiply3X3(forwardMatrix, cameraCalibrationInv);
    // This path bypasses ColorMatrix inversion
    // Should work correctly
}
```

---

## Recommended Next Steps

### Immediate (High Priority)

1. **Run Diagnostics**
   ```
   dotnet test LeicaDngDiagnosticsTests.DiagnoseLeicaFiles
   ```
   This will show us:
   - Exact metadata values in M8 and CL files
   - Whether ColorMatrix1 exists
   - Whether AsShotNeutral looks normal
   - Whether black/white levels are unusual

2. **Compare Output**
   - Show diagnostics for:
     - M240 (working)
     - CL (broken)
     - M8 (broken)
   - Identify differences

### Medium Priority (Based on Diagnostics Results)

3. **Implement Targeted Fixes**
   - If AsShotNeutral is inverted: Add inversion logic
   - If black level is unusual: Add validation/normalization
   - If ForwardMatrix used: Verify logic works correctly

4. **Add Test Cases**
   - Create minimal M8-format DNG
   - Create minimal CL-format DNG
   - Test with edge case metadata

### Testing Strategy

5. **Validate Fixes**
   - Re-render leica_cl_01.dng
   - Re-render RAW_LEICA_M8.dng
   - Compare against M240 rendering quality
   - Check color accuracy and brightness

---

## What Makes M8 & CL Different

### Leica M8 (2008)
- First digital Leica
- 12.2MP sensor
- May have had pre-DNG spec metadata encoding
- Could use non-standard tag values
- Might store gains differently

### Leica CL (2017)
- Modern but compact design
- May use tiled encoding
- Could have different metadata layout
- Rangefinder without AF complications

### Expected Metadata Patterns

```
M240 (Working):
  ├─ BlackLevel: [60, 50, 50, 60] ✓
  ├─ WhiteLevel: [4000, 4000, 4000, 4000] ✓
  ├─ AsShotNeutral: [2.0, 1.0, 1.5] ✓
  ├─ ColorMatrix1: Valid ✓
  └─ CalibrationIlluminant1: 23 (D50) ✓

M8/CL (Broken - Hypothesis):
  ├─ BlackLevel: [-100] OR [0, 0, 0, 0] ← Unusual
  ├─ WhiteLevel: [4095] ← Single value
  ├─ AsShotNeutral: [0.5, 1.0, 0.67] ← Inverted?
  ├─ ColorMatrix1: Identity OR missing ← Red flag
  └─ CalibrationIlluminant1: 0 OR missing ← Red flag
```

---

## Action Plan

### Phase 1: Diagnosis (Now)
- [x] Create diagnostic tool
- [ ] Run diagnostics on M8 and CL files
- [ ] Document findings

### Phase 2: Analysis (After Diagnostics)
- [ ] Compare metadata patterns
- [ ] Identify root causes for each model
- [ ] Design targeted fixes

### Phase 3: Implementation (Based on Phase 2)
- [ ] Implement fixes for M8 issues
- [ ] Implement fixes for CL issues
- [ ] Add test coverage

### Phase 4: Validation
- [ ] Re-test M8 files
- [ ] Re-test CL files
- [ ] Verify no regression on M240
- [ ] Verify HUAWEI still works

---

## Expected Outcomes

After completing the investigation and fixes:

✓ leica_cl_01.dng - Proper color, correct brightness  
✓ RAW_LEICA_M8.dng - No pink cast, proper rendering  
✓ All other Leica files - Continue working  
✓ M240 - No regression  
✓ HUAWEI - No regression  

**Goal:** 17/17 DNG files rendering correctly (100% success rate)

---

## Appendix: Diagnostic Tool Usage

The `LeicaDngDiagnostics` class provides:

```csharp
var output = LeicaDngDiagnostics.AnalyzeDngFile(filePath);
Console.WriteLine(output);
```

This will show:
- ✓ Dimensions and bit depth
- ✓ CFA pattern
- ✓ Black/white levels (with variance warnings)
- ✓ Illuminant (with interpretation)
- ✓ White balance values
- ✓ Color matrices (present/absent)
- ⚠️ Detected issues
- 💡 Recommendations

Use this tool to diagnose metadata before implementing fixes.


