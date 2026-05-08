# Follow-Up Investigation - Leica M8 & CL Issues

**Date:** April 19, 2026  
**Status:** Additional Issues Identified - Diagnostic Tools Created  
**Priority:** HIGH - 2/3 remaining Leica models still broken

---

## Summary

After implementing the first 3 priority fixes, testing reveals:
- ✅ HUAWEI: Still works correctly (no regression)
- ✅ Leica M240: Fixed by initial implementation
- ❌ **Leica CL: Still renders dark and pink**
- ❌ **Leica M8: Still renders complete pink image**

This indicates **additional metadata issues** specific to these older/compact Leica models.

---

## What Was Done

### Diagnostic Tools Created

1. **LeicaDngDiagnostics.cs**
   - Utility class that analyzes DNG metadata
   - Shows exact values for:
     - Black/White levels
     - AsShotNeutral (white balance)
     - ColorMatrix and ForwardMatrix
     - CalibrationIlluminant
   - Detects anomalies and provides recommendations

2. **LeicaDngDiagnosticsTests.cs**
   - Test class to run diagnostics
   - Analyzes M8, CL, and M240 files
   - Outputs detailed metadata comparison

3. **LEICA_M8_CL_INVESTIGATION.md**
   - Investigation guide with hypotheses
   - Expected metadata patterns
   - Action plan for fixes

---

## Root Cause Hypotheses

### Hypothesis #1: AsShotNeutral Inverted or Scaled Differently
- **Symptom:** Pink/magenta cast
- **Cause:** M8/CL might store 1/gains instead of gains
- **Example:** [0.5, 1.0, 0.67] instead of [2.0, 1.0, 1.5]
- **Fix:** Detect and invert if needed

### Hypothesis #2: Black Level Invalid or Unusual
- **Symptom:** Dark image
- **Cause:** Black level might be negative, zero, or per-channel conflicting
- **Example:** [-100] or [0, 0, 0, 0] instead of [60, 50, 50, 60]
- **Fix:** Validate and normalize

### Hypothesis #3: ColorMatrix Missing or Invalid
- **Symptom:** No color correction applied
- **Cause:** ColorMatrix1 might be identity or missing, ForwardMatrix not used
- **Example:** [[1,0,0], [0,1,0], [0,0,1]] (no color mapping)
- **Fix:** Ensure correct matrix path is used

### Hypothesis #4: Compression or Encoding Different
- **Symptom:** Data corruption
- **Cause:** M8/CL might use unusual compression format
- **Fix:** Improve decompression handling

---

## Next Steps - Immediate Actions

### Step 1: Run Diagnostics

```bash
cd /Users/dion/data/git/starsky/starsky
dotnet test starskytest -k "LeicaDngDiagnosticsTests" -v detailed
```

This will output detailed metadata analysis showing:
- M8 file structure and metadata values
- CL file structure and metadata values
- M240 file structure (reference, should be fine)
- Identified anomalies

### Step 2: Analyze Output

Compare the diagnostic output for all three files:
- Are black/white levels in same format?
- Is AsShotNeutral encoded differently?
- Is ColorMatrix1 valid or identity?
- Is ForwardMatrix1 present/different?

### Step 3: Identify Patterns

Determine if issues are:
- **M8-specific** (2008 legacy format)
- **CL-specific** (2017 compact design)
- **Shared** (both use D50 illuminant)
- **Related to compression/encoding**

### Step 4: Implement Targeted Fixes

Based on diagnostic findings, implement specific fixes:
- If AsShotNeutral inverted: Add inversion logic
- If black level invalid: Add validation/correction
- If ColorMatrix missing: Add ForwardMatrix priority
- If compression wrong: Add handling

### Step 5: Add Test Coverage

Create test cases for identified issues:
- Minimal M8-format DNG
- Minimal CL-format DNG
- Edge case metadata values

### Step 6: Validate

Re-test rendering:
- leica_cl_01.dng → Should render properly
- RAW_LEICA_M8.dng → Should render properly
- Verify M240 still works
- Verify HUAWEI still works

---

## Files Created This Session

### Diagnostic Utilities
1. **LeicaDngDiagnostics.cs** (starskytest/...)
   - Metadata analysis utility
   - Detailed metadata extraction
   - Anomaly detection

2. **LeicaDngDiagnosticsTests.cs** (starskytest/...)
   - Test harness for diagnostics
   - Can analyze any DNG file
   - Outputs to test output

### Documentation
1. **LEICA_M8_CL_INVESTIGATION.md**
   - Investigation guide
   - Hypotheses for root causes
   - Expected metadata patterns
   - Action plan

2. **LEICA_DNG_FOLLOW_UP.md** (this file)
   - Summary of findings
   - Next steps
   - Timeline

---

## Why M8 & CL Are Different

### Leica M8 (2008)
- **Age:** 16+ years old (from DNG perspective)
- **Sensor:** 12.2MP (older design)
- **DNG Support:** Early DNG implementation, may have quirks
- **Encoding:** May not follow all modern standards
- **Issues:** Could use non-standard metadata encoding

### Leica CL (2017)
- **Age:** 7 years old (recent but specialized)
- **Sensor:** 24MP (modern)
- **DNG Support:** Modern DNG spec
- **Form Factor:** Compact rangefinder (special requirements)
- **Issues:** Could use specialized metadata layout or tiling

### Comparison with M240 (Working)
- **Age:** 10+ years old but still modern-ish
- **Sensor:** 36.3MP (proven design)
- **DNG Support:** Well-tested implementation
- **Metadata:** Standard encoding, widely compatible

---

## Expected Timeline

| Step | Status | Time |
|------|--------|------|
| 1. Run Diagnostics | ⏳ PENDING | 5 min |
| 2. Analyze Output | ⏳ PENDING | 15 min |
| 3. Identify Pattern | ⏳ PENDING | 15 min |
| 4. Implement Fixes | ⏳ PENDING | 1-2 hours |
| 5. Add Tests | ⏳ PENDING | 1 hour |
| 6. Validate | ⏳ PENDING | 30 min |
| **Total** | **⏳ PENDING** | **3-4 hours** |

---

## Success Criteria

### Before This Session
```
HUAWEI:  ✓ Works
M240:    ✗ Broken (dark, pink)
CL:      ✗ Broken (dark, pink)
M8:      ✗ Broken (complete pink)
Success: 14/17 files (82%)
```

### After Initial 3 Fixes
```
HUAWEI:  ✓ Works (no regression)
M240:    ✓ FIXED
CL:      ✗ Still broken (dark, pink)
M8:      ✗ Still broken (complete pink)
Success: 15/17 files (88%)
```

### Target After M8/CL Fixes
```
HUAWEI:  ✓ Works
M240:    ✓ Works
CL:      ✓ FIXED
M8:      ✓ FIXED
Success: 17/17 files (100%)
```

---

## How to Use Diagnostic Tools

### Basic Usage
```csharp
var output = LeicaDngDiagnostics.AnalyzeDngFile("/path/to/file.dng");
Console.WriteLine(output);
```

### Output Includes
- ✓ File dimensions and bit depth
- ✓ CFA pattern (Bayer layout)
- ✓ Black level values and variance
- ✓ White level values and variance
- ✓ AsShotNeutral (white balance) values
- ✓ ColorMatrix1 values
- ✓ ForwardMatrix1 presence
- ✓ CalibrationIlluminant value
- ⚠️ Detected anomalies (warnings)
- 💡 Recommendations for fixes

### Example Output Format
```
=== Analyzing: RAW_LEICA_M8.dng ===
✓ Loaded successfully

📐 DIMENSIONS:
  Width: 5216
  Height: 3472
  BitsPerSample: 12

🎨 COLOR INFO:
  CFA Pattern: [0, 1, 1, 2]

⚫ BLACK LEVEL:
  Values (1): [0]
  ⚠️  SINGLE VALUE - verify this is intentional

⚪ WHITE LEVEL:
  Values (1): [4095]
  ⚠️  UNUSUAL - white level matches bit depth boundary

💡 ILLUMINANT:
  CalibrationIlluminant1: 0 (Unknown)
  ⚠️  UNKNOWN - defaults to D65 after fix

🤍 WHITE BALANCE (AsShotNeutral):
  Values: [0.5, 1.0, 0.67]
  ⚠️  VALUES < 1 - may indicate inverted encoding
```

---

## Key Insights

### Why Diagnostics Matter

The diagnostic tool shows:
1. **Exact metadata values** - no guessing
2. **Anomaly detection** - flags suspicious values
3. **Recommendations** - suggests what to fix
4. **Comparison ability** - see M8 vs M240 differences

### What We're Looking For

When running diagnostics, look for:
- **Negative numbers** in black/white levels
- **Very small numbers** in white balance
- **Identity matrices** for color correction
- **Missing color data** (ColorMatrix1 = null)
- **Mismatched per-channel values**

### How This Helps

The diagnostics let us:
1. Confirm actual vs hypothesized issues
2. Implement targeted fixes (not guesses)
3. Add appropriate test coverage
4. Prevent regressions in future

---

## Conclusion

The diagnostic tools are ready. The next phase is:
1. Run diagnostics on the problematic files
2. Analyze the actual metadata
3. Implement targeted fixes based on findings
4. Validate all files render correctly

The investigation framework is in place.
Ready to proceed when test files are available.

---

## References

- **Investigation Guide:** LEICA_M8_CL_INVESTIGATION.md
- **Diagnostic Tool:** LeicaDngDiagnostics.cs
- **Test Harness:** LeicaDngDiagnosticsTests.cs
- **Previous Fixes:** IMPLEMENTATION_COMPLETE.md


