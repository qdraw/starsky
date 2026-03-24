using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.TiffEmbeded;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;

[TestClass]
public class TiffEmbeddedPreviewExtractorCoverageTests
{
    private const string InputDngSubPath = "/raw/test.dng";
    private const string OutputSubPath = "/tmp/output.jpg";

    private static FakeSelectorStorageByType CreateSelectorStorage(byte[]? inputBytes,
        string inputSubPath,
        out FakeIStorage subPathStorage,
        out FakeIStorage tempStorage)
    {
        subPathStorage = inputBytes != null
            ? new FakeIStorage(
                ["/raw"],
                [inputSubPath],
                [inputBytes])
            : new FakeIStorage(["/raw"]);

        tempStorage = new FakeIStorage(["/tmp"]);
        var thumbnailStorage = new FakeIStorage();
        var hostStorage = new FakeIStorage();

        return new FakeSelectorStorageByType(subPathStorage, thumbnailStorage, hostStorage,
            tempStorage);
    }

    [TestMethod]
    public void TryParseTiffHeader_WithIfdOffsetEqualToLength_ReturnsTrue()
    {
        // Arrange: Valid header but IFD offset is exactly at stream length (boundary)
        var data = new byte[] { 0x49, 0x49, 0x2A, 0x00, 0x08, 0x00, 0x00, 0x00 };
        using var ms = new MemoryStream(data);

        // Act
        var result = TiffEmbeddedPreviewExtractor.TryParseTiffHeader(ms, out _, out var offset);

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(8u, offset);
    }

    [TestMethod]
    public void TryParseTiffHeader_WithIfdOffsetBeyondLength_ReturnsFalse()
    {
        // Arrange: Valid header but IFD offset is beyond stream length
        var data = new byte[] { 0x49, 0x49, 0x2A, 0x00, 0x09, 0x00, 0x00, 0x00 };
        using var ms = new MemoryStream(data);

        // Act
        var result = TiffEmbeddedPreviewExtractor.TryParseTiffHeader(ms, out _, out _);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void ParseIfdRecursive_WithEntryCountExceedingMax_ReturnsEarly()
    {
        // Arrange
        var context = new TiffEmbeddedPreviewExtractor.ParseTraversalContext
        {
            RawFlavor = RawFlavor.Unknown, Previews = [], Visited = [], ReferenceInfo = "test"
        };
        var data = new byte[20];
        data[8] = 0x11; // 10001 entries (> 10000)
        data[9] = 0x27;
        using var ms = new MemoryStream(data);

        // Act
        TiffEmbeddedPreviewExtractor.ParseIfdRecursive(ms, 8, true, context, 0, false);

        // Assert
        Assert.HasCount(1, context.Visited);
    }

    [TestMethod]
    public void ParseNextIfd_WithIsSubIfdTrue_ReturnsEarly()
    {
        // Arrange
        var context = new TiffEmbeddedPreviewExtractor.ParseTraversalContext
        {
            RawFlavor = RawFlavor.Unknown, Previews = [], Visited = [], ReferenceInfo = "test"
        };
        using var ms = new MemoryStream(new byte[100]);

        // Act
        TiffEmbeddedPreviewExtractor.ParseNextIfd(ms, true, context, 0, true);

        // Assert
        Assert.AreEqual(0L, ms.Position); // Should not have read anything
    }

    [TestMethod]
    public void ParseNextIfd_WithTooShortRead_ReturnsEarly()
    {
        // Arrange
        var context = new TiffEmbeddedPreviewExtractor.ParseTraversalContext
        {
            RawFlavor = RawFlavor.Unknown, Previews = [], Visited = [], ReferenceInfo = "test"
        };
        using var ms = new MemoryStream(new byte[2]); // Too short for 4-byte offset

        // Act
        TiffEmbeddedPreviewExtractor.ParseNextIfd(ms, true, context, 0, false);

        // Assert
        Assert.AreEqual(2L, ms.Position);
    }

    [TestMethod]
    public void ParseNextIfd_WithMaxRootIfdChainReached_ReturnsEarly()
    {
        // Arrange
        var context = new TiffEmbeddedPreviewExtractor.ParseTraversalContext
        {
            RawFlavor = RawFlavor.Unknown, Previews = [], Visited = [], ReferenceInfo = "test"
        };
        var data = new byte[] { 0x08, 0x00, 0x00, 0x00 };
        using var ms = new MemoryStream(data);

        // Act (MaxRootIfdChain is 6)
        TiffEmbeddedPreviewExtractor.ParseNextIfd(ms, true, context, 6, false);

        // Assert
        Assert.AreEqual(4L, ms.Position);
        Assert.IsEmpty(context.Visited);
    }

    [TestMethod]
    public void AddSubIfdOffsets_WithNGreaterThanOne_ReadsIndirect()
    {
        // Arrange
        var subIfdOffsets = new List<uint>();
        var data = new byte[100];
        // At offset 50, put two subIFD offsets
        data[50] = 0x64; // 100
        data[54] = 0xC8; // 200
        using var ms = new MemoryStream(data);

        // Act
        // type 4 (LONG), n=2, value=50
        // Use reflection or make internal to call directly if possible
        var method = typeof(TiffEmbeddedPreviewExtractor).GetMethod("AddSubIfdOffsets", BindingFlags.NonPublic | BindingFlags.Static);
        method.Invoke(null, [ms, true, subIfdOffsets, (ushort)4, (uint)2, (uint)50]);

        // Assert
        Assert.HasCount(2, subIfdOffsets);
        Assert.AreEqual(100u, subIfdOffsets[0]);
        Assert.AreEqual(200u, subIfdOffsets[1]);
    }

    [TestMethod]
    public void TryReadIfdEntryHeader_WithTooShortBlock_ReturnsFalse()
    {
        // Arrange
        using var ms = new MemoryStream(new byte[100]);
        var method = typeof(TiffEmbeddedPreviewExtractor).GetMethod("TryReadIfdEntryHeader", BindingFlags.NonPublic | BindingFlags.Static);

        // Act
        // blockLength = 5 (< 6)
        var result = (bool)method.Invoke(null, [ms, (uint)8, (uint)5, true, null, null]);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void TryReadIfdEntryHeader_WithEntryCountReadFailure_ReturnsFalse()
    {
        // Arrange
        using var ms = new MemoryStream(new byte[2]);
        ms.Seek(1, SeekOrigin.Begin); // Only 1 byte left
        var method = typeof(TiffEmbeddedPreviewExtractor).GetMethod("TryReadIfdEntryHeader", BindingFlags.NonPublic | BindingFlags.Static);

        // Act
        var result = (bool)method.Invoke(null, [ms, (uint)1, (uint)10, true, null, null]);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsJpegAtOffset_WithReadFailure_ReturnsFalse()
    {
        // Arrange
        using var ms = new MemoryStream(new byte[2]); // Only 2 bytes, needs 3
        var method = typeof(TiffEmbeddedPreviewExtractor).GetMethod("IsJpegAtOffset", BindingFlags.NonPublic | BindingFlags.Static);

        // Act
        var result = (bool)method.Invoke(null, [ms, (uint)0]);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void TryResolveMakerNoteOffset_WithAbsoluteOffsetFound_ReturnsTrue()
    {
        // Arrange
        var data = new byte[100];
        data[50] = 0xFF; data[51] = 0xD8; data[52] = 0xFF; // JPEG signature at 50
        using var ms = new MemoryStream(data);
        var method = typeof(TiffEmbeddedPreviewExtractor).GetMethod("TryResolveMakerNoteOffset", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public);

        // Act
        uint resolvedOffset = 0;
        var args = new object[] { ms, (uint)10, (uint)50, resolvedOffset };
        var result = (bool)method.Invoke(null, args)!;
        resolvedOffset = (uint)args[3]!;

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(50u, resolvedOffset);
    }

    [TestMethod]
    public void TryResolveMakerNoteOffset_WithRelativeOffsetFound_ReturnsTrue()
    {
        // Arrange
        var data = new byte[100];
        data[60] = 0xFF; data[61] = 0xD8; data[62] = 0xFF; // JPEG signature at 60 (10+50)
        using var ms = new MemoryStream(data);
        var method = typeof(TiffEmbeddedPreviewExtractor).GetMethod("TryResolveMakerNoteOffset", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public);

        // Act
        uint resolvedOffset = 0;
        var args = new object[] { ms, (uint)10, (uint)50, resolvedOffset };
        var result = (bool)method.Invoke(null, args)!;
        resolvedOffset = (uint)args[3]!;

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(60u, resolvedOffset);
    }

    [TestMethod]
    public void ParseMakerNote_WithClampedBoundedLength_ReturnsEarly()
    {
        // Arrange
        using var ms = new MemoryStream(new byte[100]);
        var previews = new List<TiffEmbeddedPreviewExtractor.PreviewCandidate>();

        // Act: makerNoteOffset = 100 (exactly at length), n = 10 -> boundedLength = 0
        TiffEmbeddedPreviewExtractor.ParseMakerNote(ms, true, RawFlavor.SonyArw, 100, 10, previews);

        // Assert
        Assert.IsEmpty(previews);
    }

    [TestMethod]
    public void ParseMakerNote_WithOffsetBeyondLength_ReturnsEarly()
    {
        // Arrange
        using var ms = new MemoryStream(new byte[100]);
        var previews = new List<TiffEmbeddedPreviewExtractor.PreviewCandidate>();

        // Act: makerNoteOffset = 101 -> boundedLength = 0
        TiffEmbeddedPreviewExtractor.ParseMakerNote(ms, true, RawFlavor.SonyArw, 101, 10, previews);

        // Assert
        Assert.IsEmpty(previews);
    }

    [TestMethod]
    public void ParseMakerNote_WithPartialBoundedLength_Succeeds()
    {
        // Arrange
        var data = new byte[250];
        // At 100, put a Sony MakerNote (minimal)
        data[100] = 1; data[101] = 0; // 1 entry
        data[102] = 0x10; data[103] = 0x20; // TagSonyPreviewOffset
        data[104] = 4; data[105] = 0; // Type LONG
        data[106] = 1; data[107] = 0; data[108] = 0; data[109] = 0; // n=1
        data[110] = 200; data[111] = 0; data[112] = 0; data[113] = 0; // value=200

        // JPEG at 200
        data[200] = 0xFF; data[201] = 0xD8; data[202] = 0xFF;

        using var ms = new MemoryStream(data);
        var previews = new List<TiffEmbeddedPreviewExtractor.PreviewCandidate>();

        // Act: makerNoteOffset = 100, makerNoteLength = 1000 (beyond length 250)
        // input.Length - 100 = 150. boundedLength = 150.
        // TryReadIfdEntryHeader: blockLength = 150. entryBytes = 1*12 = 12. 12+6=18 <= 150 (OK)
        TiffEmbeddedPreviewExtractor.ParseMakerNote(ms, true, RawFlavor.SonyArw, 100, 1000, previews);

        // Assert
        Assert.IsNotEmpty(previews);
        Assert.AreEqual(200u, previews[0].Offset);
    }

    [TestMethod]
    public void ParseMakerNote_WithUnknownRawFlavor_ReturnsEarly()
    {
        // Arrange
        using var ms = new MemoryStream(new byte[200]);
        var previews = new List<TiffEmbeddedPreviewExtractor.PreviewCandidate>();

        // Act
        TiffEmbeddedPreviewExtractor.ParseMakerNote(ms, true, RawFlavor.Unknown, 10, 100, previews);

        // Assert
        Assert.IsEmpty(previews);
    }

    [TestMethod]
    public void TryReadIfdEntryHeader_WithLargeEntryCount_ReturnsFalse()
    {
        // Arrange
        var data = new byte[20];
        data[8] = 0x01; data[9] = 0x02; // 513 entries
        using var ms = new MemoryStream(data);
        var method = typeof(TiffEmbeddedPreviewExtractor).GetMethod("TryReadIfdEntryHeader", BindingFlags.NonPublic | BindingFlags.Static);

        // Act
        var result = (bool)method.Invoke(null, [ms, (uint)8, (uint)100, true, null, null])!;

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void ExtractTagPairValues_WithNGreaterThanOne_SkipsEntry()
    {
        // Arrange
        // TagSonyPreviewOffset, n=2 (should skip)
        var entries = new byte[12];
        entries[0] = 0x10; entries[1] = 0x20; // tag
        entries[2] = 4; entries[3] = 0; // type LONG
        entries[4] = 2; entries[5] = 0; entries[6] = 0; entries[7] = 0; // n=2
        entries[8] = 100; entries[9] = 0; entries[10] = 0; entries[11] = 0; // value

        var query = new TiffEmbeddedPreviewExtractor.IfdTagPairQuery(0x2010, 0x2011, 0, true);
        var method = typeof(TiffEmbeddedPreviewExtractor).GetMethod("ExtractTagPairValues", BindingFlags.NonPublic | BindingFlags.Static);

        // Act
        var result = method.Invoke(null, [ (ReadOnlySpan<byte>)entries, 1, query ])!;
        var (offset, length) = ((uint Offset, uint Length))result;

        // Assert
        Assert.AreEqual(0u, offset);
    }

    [TestMethod]
    public void HandleIfdEntry_WithSubIfdsTagAndNOne_AddsDirectly()
    {
        // Arrange
        var entry = new byte[12];
        entry[0] = 0x4A; entry[1] = 0x01; // TagSubIfds
        entry[2] = 4; entry[3] = 0; // LONG
        entry[4] = 1; entry[5] = 0; entry[6] = 0; entry[7] = 0; // n=1
        entry[8] = 50; entry[9] = 0; entry[10] = 0; entry[11] = 0; // value=50

        var subIfdOffsets = new List<uint>();
        var state = new TiffEmbeddedPreviewExtractor.IfdEntryState();
        using var ms = new MemoryStream();

        // Act
        var method = typeof(TiffEmbeddedPreviewExtractor).GetMethod("HandleIfdEntry", BindingFlags.NonPublic | BindingFlags.Static);
        method.Invoke(null, [ms, (ReadOnlySpan<byte>)entry, true, subIfdOffsets, state]);

        // Assert
        Assert.AreEqual(1, subIfdOffsets.Count);
        Assert.AreEqual(50u, subIfdOffsets[0]);
    }

    [TestMethod]
    public void HandleIfdEntry_WithMakerNoteTag_SetsState()
    {
        // Arrange
        var entry = new byte[12];
        entry[0] = 0x7C; entry[1] = 0x92; // TagMakerNote
        entry[2] = 7; entry[3] = 0; // UNDEFINED
        entry[4] = 100; entry[5] = 0; entry[6] = 0; entry[7] = 0; // n=100
        entry[8] = 50; entry[9] = 0; entry[10] = 0; entry[11] = 0; // value=50

        var subIfdOffsets = new List<uint>();
        var state = new TiffEmbeddedPreviewExtractor.IfdEntryState();
        using var ms = new MemoryStream();

        // Act
        var method = typeof(TiffEmbeddedPreviewExtractor).GetMethod("HandleIfdEntry", BindingFlags.NonPublic | BindingFlags.Static);
        method.Invoke(null, [ms, (ReadOnlySpan<byte>)entry, true, subIfdOffsets, state]);

        // Assert
        Assert.IsTrue(state.HasMakerNote);
        Assert.AreEqual(50u, state.MakerNoteOffset);
        Assert.AreEqual(100u, state.MakerNoteLength);
    }

    [TestMethod]
    public void HandleIfdEntry_WithMakerNoteTagTooSmall_DoesNotSetState()
    {
        // Arrange
        var entry = new byte[12];
        entry[0] = 0x7C; entry[1] = 0x92;
        entry[4] = 4; // n=4 (too small, needs > 4)
        entry[8] = 50;

        var subIfdOffsets = new List<uint>();
        var state = new TiffEmbeddedPreviewExtractor.IfdEntryState();
        using var ms = new MemoryStream();

        // Act
        var method = typeof(TiffEmbeddedPreviewExtractor).GetMethod("HandleIfdEntry", BindingFlags.NonPublic | BindingFlags.Static);
        method.Invoke(null, [ms, (ReadOnlySpan<byte>)entry, true, subIfdOffsets, state]);

        // Assert
        Assert.IsFalse(state.HasMakerNote);
    }

    [TestMethod]
    public void AppendDirectJpegCandidate_WithStripTooSmall_Skips()
    {
        // Arrange
        var previews = new List<TiffEmbeddedPreviewExtractor.PreviewCandidate>();
        var state = new TiffEmbeddedPreviewExtractor.IfdEntryState
        {
            IfdCompression = 6,
            HasStrip = true,
            StripOffset = 100,
            StripLength = 1024 // Too small (< 4096)
        };
        using var ms = new MemoryStream();

        // Act
        var method = typeof(TiffEmbeddedPreviewExtractor).GetMethod("AppendDirectJpegCandidate", BindingFlags.NonPublic | BindingFlags.Static);
        method.Invoke(null, [previews, state, ms, RawFlavor.Unknown]);

        // Assert
        Assert.IsEmpty(previews);
    }

    [TestMethod]
    public void ReadScalarValue_WithVariousTypes_ReturnsExpected()
    {
        // Type 3 (SHORT), littleEndian = true
        Assert.AreEqual(0x1234u, TiffEmbeddedPreviewExtractor.ReadScalarValue(3, 0x1234, true));
        // Type 3 (SHORT), littleEndian = false
        Assert.AreEqual(0x1234u, TiffEmbeddedPreviewExtractor.ReadScalarValue(3, 0x12340000, false));
        // Type 4 (LONG)
        Assert.AreEqual(0x12345678u, TiffEmbeddedPreviewExtractor.ReadScalarValue(4, 0x12345678, true));
        // Type 99 (Unknown)
        Assert.AreEqual(0u, TiffEmbeddedPreviewExtractor.ReadScalarValue(99, 0x12345678, true));
    }

    [TestMethod]
    public void IsLosslessJpegAtOffset_WithVariousHeaders_ReturnsExpected()
    {
        // FF D8 FF C4 (DHT) -> True
        using var ms1 = new MemoryStream([0xFF, 0xD8, 0xFF, 0xC4]);
        Assert.IsTrue(TiffEmbeddedPreviewExtractor.IsLosslessJpegAtOffset(ms1, 0));

        // FF D8 FF C3 (SOF3) -> True
        using var ms2 = new MemoryStream([0xFF, 0xD8, 0xFF, 0xC3]);
        Assert.IsTrue(TiffEmbeddedPreviewExtractor.IsLosslessJpegAtOffset(ms2, 0));

        // FF D8 FF E0 (Normal JPEG) -> False
        using var ms3 = new MemoryStream([0xFF, 0xD8, 0xFF, 0xE0]);
        Assert.IsFalse(TiffEmbeddedPreviewExtractor.IsLosslessJpegAtOffset(ms3, 0));

        // Too short -> False
        using var ms4 = new MemoryStream([0xFF, 0xD8, 0xFF]);
        Assert.IsFalse(TiffEmbeddedPreviewExtractor.IsLosslessJpegAtOffset(ms4, 0));
    }

    [TestMethod]
    public void IsJpegCompression_WithVariousValues_ReturnsExpected()
    {
        var method = typeof(TiffEmbeddedPreviewExtractor).GetMethod("IsJpegCompression", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsTrue((bool)method.Invoke(null, [(uint)6])!);
        Assert.IsTrue((bool)method.Invoke(null, [(uint)7])!);
        Assert.IsFalse((bool)method.Invoke(null, [(uint)1])!);
    }

    [TestMethod]
    public void ScanJpegsInRange_WithTooShortRange_ReturnsEmpty()
    {
        using var ms = new MemoryStream([0xFF, 0xD8, 0xFF]);
        var result = TiffEmbeddedPreviewExtractor.ScanJpegsInRange(ms, 0, 3);
        Assert.AreEqual(0, new List<TiffEmbeddedPreviewExtractor.PreviewCandidate?>(result).Count);
    }

    [TestMethod]
    public void TryBuildScanCandidate_WithLosslessJpeg_ReturnsFalse()
    {
        using var ms = new MemoryStream([0xFF, 0xD8, 0xFF, 0xC4, 0x00, 0x00, 0x00, 0x00]);
        var result = TiffEmbeddedPreviewExtractor.TryBuildScanCandidate(ms, 0, 8, out var candidate);
        Assert.IsFalse(result);
        Assert.IsNull(candidate);
    }

    [TestMethod]
    public void TryExtract_WithNonExistentFile_ReturnsFalse()
    {
        var logger = new FakeIWebLogger();
        var storage = CreateSelectorStorage(null, "none", out _, out _);
        var extractor = new TiffEmbeddedPreviewExtractor(logger, storage);

        var result = extractor.TryExtract("none", "out").GetAwaiter().GetResult();
        Assert.IsFalse(result);
    }
}
