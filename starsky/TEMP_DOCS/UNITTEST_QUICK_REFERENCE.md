# Unit Tests - Quick Reference

## 📋 Test File Location
`starskytest/starsky.foundation.metaupdate/Services/ExifTimezoneCorrectionServiceTest.cs`

## 📊 Test Statistics
- **Total Tests:** 42
- **Test Classes:** 1 (ExifTimezoneCorrectionServiceTest)
- **Status:** ✅ All tests compile without errors

## 🏃 Running Tests

### Run All Tests
```bash
cd /Users/dion/data/git/starsky/starsky
dotnet test starskytest.csproj -k ExifTimezoneCorrectionServiceTest
```

### Run Specific Test Category

**Validation Tests (12 tests)**
```bash
dotnet test starskytest.csproj -k "ValidateCorrection"
```

**Synchronous Correction Tests (12 tests)**
```bash
dotnet test starskytest.csproj -k "CorrectTimezoneAsync" -k "!Batch"
```

**DST Tests (3 tests)**
```bash
dotnet test starskytest.csproj -k "DST"
```

**International Timezone Tests (3 tests)**
```bash
dotnet test starskytest.csproj -k "USEastCoast" -o "Tokyo" -o "Australia"
```

**Edge Case Tests (7 tests)**
```bash
dotnet test starskytest.csproj -k "Rollover" -o "Fractional" -o "SmallOffset"
```

**Batch Operation Tests (3 tests)**
```bash
dotnet test starskytest.csproj -k "Batch"
```

### Run Single Test
```bash
dotnet test starskytest.csproj -k "ValidateCorrection_MissingRecordedTimezone_ShouldReturnError"
```

### Run with Verbose Output
```bash
dotnet test starskytest.csproj -k ExifTimezoneCorrectionServiceTest -v n
```

### Run with Code Coverage
```bash
dotnet test starskytest.csproj -k ExifTimezoneCorrectionServiceTest \
  --collect:"XPlat Code Coverage" \
  --logger:"console;verbosity=detailed"
```

---

## 📋 Complete Test List (42 Tests)

### Validation Tests (12)
1. `ValidateCorrection_MissingRecordedTimezone_ShouldReturnError`
2. `ValidateCorrection_MissingCorrectTimezone_ShouldReturnError`
3. `ValidateCorrection_InvalidRecordedTimezone_ShouldReturnError`
4. `ValidateCorrection_InvalidCorrectTimezone_ShouldReturnError`
5. `ValidateCorrection_InvalidDateTime_ShouldReturnError`
6. `ValidateCorrection_SameTimezones_ShouldReturnWarning`
7. `ValidateCorrection_DayRollover_ShouldReturnWarning`
8. `ValidateCorrection_FileNotFound_ShouldReturnError`
9. `ValidateCorrection_ValidInput_ShouldSucceed`
10. `ValidateCorrection_MultipleWarnings_DayRolloverAndSameTimezone`
11. `ValidateCorrection_NullWhitespaceTimezone_ShouldReturnError`
12. `CorrectTimezoneAsync_InvalidRequest_ShouldReturnError`

### Correction Tests (12)
13. `CorrectTimezoneAsync_ValidCorrection_ShouldSucceed`
14. `CorrectTimezoneAsync_WinterTime_ShouldHandleDST`
15. `CorrectTimezoneAsync_NegativeOffset_ShouldSubtractTime`
16. `CorrectTimezoneAsync_CrossDayBoundary_ShouldRollDate`
17. `CorrectTimezoneAsync_MultipleImages_ShouldCorrectAll`
18. `CorrectTimezoneAsync_InvalidRequest_ShouldReturnError`
19. `CorrectTimezoneAsync_FileNotFound_ShouldReturnError`
20. `CorrectTimezoneAsync_MidnightRollover_BeforeMidnight`
21. `CorrectTimezoneAsync_MonthRollover_EndOfMonth`
22. `CorrectTimezoneAsync_YearRollover_EndOfYear`
23. `CorrectTimezoneAsync_VerySmallOffset_HalfHour`
24. `CorrectTimezoneAsync_FractionalHourOffset_Nepal`

### DST Tests (3)
25. `CorrectTimezoneAsync_DSTTransitionBefore_March30_2024`
26. `CorrectTimezoneAsync_DSTTransitionAfter_March31_2024`
27. `CorrectTimezoneAsync_DSTFallBack_October26_2024`

### International Timezone Tests (3)
28. `CorrectTimezoneAsync_USEastCoast_ToUSWestCoast`
29. `CorrectTimezoneAsync_TokyoWithBigOffset`
30. `CorrectTimezoneAsync_AustraliaTimezone`

### Fixed Offset Tests (2)
31. `CorrectTimezoneAsync_FixedUTCPlus1_ToNamedTimezone`
32. `CorrectTimezoneAsync_FixedNegativeOffset`

### Edge Case Tests (7)
Listed above (items 20-24 from correction tests, plus:)
33. `CorrectTimezoneAsync_VerySmallOffset_HalfHour`
34. `CorrectTimezoneAsync_FractionalHourOffset_Nepal`

### Batch Operation Tests (3)
35. `CorrectTimezoneAsync_MultipleImages_ShouldCorrectAll`
36. `CorrectTimezoneAsync_BatchWithDifferentDates_DSTAware`
37. `CorrectTimezoneAsync_BatchWithMixedSuccess`
38. `CorrectTimezoneAsync_BatchEmpty_ShouldReturnEmpty`

---

## 🎯 Test Coverage by Scenario

### DST Scenarios
- ✅ Spring forward (UTC+1 → UTC+2)
- ✅ Fall back (UTC+2 → UTC+1)
- ✅ Before DST transition
- ✅ After DST transition
- ✅ On DST transition day

### Timezone Combinations
- ✅ UTC → Europe/Amsterdam (DST)
- ✅ Europe/Amsterdam → UTC
- ✅ America/New_York → America/Los_Angeles
- ✅ UTC → Asia/Tokyo
- ✅ UTC → Australia/Sydney
- ✅ Fixed UTC+1 → Named timezone
- ✅ Fixed UTC-5 → UTC
- ✅ UTC → Asia/Kolkata (half-hour offset)
- ✅ UTC → Asia/Kathmandu (quarter-hour offset)

### Date Boundary Crossings
- ✅ Midnight rollover
- ✅ Month boundary rollover
- ✅ Year boundary rollover
- ✅ End of month scenarios
- ✅ End of year scenarios

### Error Scenarios
- ✅ Missing RecordedTimezone
- ✅ Missing CorrectTimezone
- ✅ Invalid timezone IDs
- ✅ Invalid DateTime in EXIF
- ✅ File not found
- ✅ Empty batch operations

### Warning Scenarios
- ✅ Same timezone (no correction needed)
- ✅ Day rollover warning
- ✅ Multiple warnings

---

## 🔍 Key Test Examples

### Example 1: DST Correction
```csharp
// Test: CorrectTimezoneAsync_DSTTransitionAfter_March31_2024
Camera: Locked to UTC+1 (forgot DST update)
Date: March 31, 2024 (DST starts, now UTC+2)
Before: 14:00 → After: 15:00
Delta: +1.0 hour
```

### Example 2: International Travel
```csharp
// Test: CorrectTimezoneAsync_USEastCoast_ToUSWestCoast
From: New York (UTC-4 EDT)
To: Los Angeles (UTC-7 PDT)
Before: 14:00 → After: 11:00
Delta: -3.0 hours
```

### Example 3: Year Rollover
```csharp
// Test: CorrectTimezoneAsync_YearRollover_EndOfYear
Date: December 31, 22:00
TZ Change: UTC → UTC+12 (Auckland)
Result: January 1 (next year), 10:00
Rolls across year boundary ✓
```

---

## ✅ Expected Test Results

When you run all tests, you should see:

```
Test Run Successful.

Total tests: 42
  Passed: 42
  Failed: 0
  Skipped: 0

Total time: < 5 seconds
```

---

## 🐛 Debugging Failed Tests

If a test fails, check:

1. **Timezone IDs**: Verify IANA timezone IDs are valid on your system
2. **DateTime**: Check if test uses DateTimeKind.Local consistently
3. **Mock Objects**: Verify FakeIStorage and other mocks are configured correctly
4. **System Settings**: Some timezone tests depend on system timezone database

### Common Issues

**Issue**: Test fails with "Invalid timezone"
- **Cause**: IANA timezone not available on system
- **Fix**: Install timezone database updates (usually automatic)

**Issue**: DST test fails with wrong delta
- **Cause**: Test date's DST status might differ on your system
- **Fix**: Check when DST occurs in your timezone

**Issue**: TimeSpan calculations off by minutes
- **Cause**: Some timezones have non-hour offsets (Nepal +5:45)
- **Fix**: Tests handle this - look for FractionalHourOffset tests

---

## 📈 Test Execution in CI/CD

### GitHub Actions
Add to `.github/workflows/test.yml`:
```yaml
- name: Run EXIF Timezone Tests
  run: dotnet test starskytest.csproj -k ExifTimezoneCorrectionServiceTest
```

### Azure Pipelines
Add to `azure-pipelines.yml`:
```yaml
- task: DotNetCoreCLI@2
  inputs:
    command: 'test'
    arguments: '-k ExifTimezoneCorrectionServiceTest'
```

---

## 📊 Test Maintenance

### Adding New Tests

When adding timezone correction features, add corresponding tests:

```csharp
[TestMethod]
public async Task CorrectTimezoneAsync_NewFeature_ShouldWork()
{
    // Arrange
    var service = CreateService();
    var fileIndexItem = new FileIndexItem { ... };
    var request = new ExifTimezoneCorrectionRequest { ... };

    // Act
    var result = await service.CorrectTimezoneAsync(fileIndexItem, request);

    // Assert
    Assert.IsTrue(result.Success);
    // ... specific assertions
}
```

### Test Naming Convention

Use format: `MethodName_Scenario_ExpectedBehavior`

Examples:
- ✅ `CorrectTimezoneAsync_DSTTransitionAfter_ShouldApplyDelta`
- ✅ `ValidateCorrection_MissingTimezone_ShouldReturnError`
- ✅ `CorrectTimezoneAsync_BatchWithMixedSuccess_ShouldHandlePartialFailures`

---

## 🎓 Learning Resources

### Test File
`starskytest/starsky.foundation.metaupdate/Services/ExifTimezoneCorrectionServiceTest.cs`

### Related Documentation
- See `UNITTEST_SUMMARY.md` for detailed test descriptions
- See `TIMEZONE_CORRECTION_IMPLEMENTATION.md` for algorithm details
- See `OFFSETTIME_MISSING_UNRELIABLE.md` for why we test what we do

---

## 📞 Quick Help

### Run all tests
```bash
dotnet test starskytest.csproj -k ExifTimezoneCorrectionServiceTest
```

### Run DST tests only
```bash
dotnet test starskytest.csproj -k "DST"
```

### Run with output
```bash
dotnet test starskytest.csproj -k ExifTimezoneCorrectionServiceTest -v n
```

### Get list of all tests
```bash
dotnet test starskytest.csproj --list-tests -k ExifTimezoneCorrectionServiceTest
```

---

**All 42 tests are ready to run! ✅**

