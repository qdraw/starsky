using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.Models;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.TiffEmbedded;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.
	TiffEmbedded;

[TestClass]
public class TiffEmbeddedPreviewCoverageTests
{
	[TestMethod]
	public void ParseNextIfd_WithIsSubIfdTrue_ReturnsEarly()
	{
		// When isSubIfd=true, ParseNextIfd returns early (line 236-238)
		using var ms = new MemoryStream(new byte[100]);
		var context = new ParseTraversalContext
		{
			Previews = [], Visited = [], ReferenceInfo = "test", RawFlavor = RawFlavor.Unknown
		};

		TiffEmbeddedPreviewExtractor.ParseNextIfd(ms, true, context, 0, true);
		Assert.AreEqual(0, ms.Position);
	}

	[TestMethod]
	public void ParseSubIfdChain_WithEmptyList_DoesNothing()
	{
		// Empty subIfdOffsets should result in no action (line 391)
		using var ms = new MemoryStream(new byte[100]);
		var context = new ParseTraversalContext
		{
			Previews = [], Visited = [], ReferenceInfo = "test", RawFlavor = RawFlavor.Unknown
		};
		var emptyList = new List<uint>();

		TiffEmbeddedPreviewExtractor.ParseSubIfdChain(ms, true, context, 0, emptyList);
		Assert.IsEmpty(context.Previews);
	}

	[TestMethod]
	public void ParseSubIfdChain_WhenPreviewsAtMax_ReturnsFalse()
	{
		// Arrange: create a stream (not actually read because method returns early)
		using var ms = new MemoryStream(new byte[200]);
		// Fill previews to the maximum expected (internal MaxPreviews == 8)
		var previews = new List<PreviewCandidate>();
		for ( var i = 0; i < 8; i++ )
		{
			previews.Add(new PreviewCandidate());
		}

		var context = new ParseTraversalContext
		{
			Previews = previews,
			Visited = [],
			ReferenceInfo = "unit-test",
			RawFlavor = RawFlavor.Unknown
		};

		var subIfdOffsets = new List<uint> { 100 };

		// Act
		var result =
			TiffEmbeddedPreviewExtractor.ParseSubIfdChain(ms, true, context, 0, subIfdOffsets);

		// Assert
		Assert.IsFalse(result,
			"ParseSubIfdChain should return false when Previews count >= MaxPreviews");
	}

	[TestMethod]
	public void ParseMakerNote_WithZeroOffset_ExitsEarly()
	{
		// When makerNoteOffset is 0, ParseMakerNote returns early (line 404-406)
		using var ms = new MemoryStream(new byte[100]);
		var previews = new List<PreviewCandidate>();

		TiffEmbeddedPreviewExtractor.ParseMakerNote(ms, true, RawFlavor.Unknown, 0, 100, previews);
		Assert.IsEmpty(previews);
	}

	[TestMethod]
	public void ParseMakerNote_WithZeroLength_ExitsEarly()
	{
		// When makerNoteLength is 0, ParseMakerNote returns early (line 405-407)
		using var ms = new MemoryStream(new byte[100]);
		var previews = new List<PreviewCandidate>();

		TiffEmbeddedPreviewExtractor.ParseMakerNote(ms, true, RawFlavor.Unknown, 50, 0, previews);
		Assert.IsEmpty(previews);
	}

	[TestMethod]
	public void ParseMakerNote_WithOffsetBeyondStream_BoundedLengthZero()
	{
		// When makerNoteOffset is beyond stream, boundedLength becomes 0 (line 412-419)
		using var ms = new MemoryStream(new byte[50]);
		var previews = new List<PreviewCandidate>();

		TiffEmbeddedPreviewExtractor.ParseMakerNote(ms, true, RawFlavor.SonyArw, 100, 1000,
			previews);
		Assert.IsEmpty(previews);
	}

	[TestMethod]
	public void TryReadIfdEntryHeader_WithBlockLengthTooSmall_ReturnsFalse()
	{
		// When blockLength < 6, TryReadIfdEntryHeader returns false (line 559)
		using var ms = new MemoryStream(new byte[100]);

		var result =
			TiffEmbeddedPreviewExtractor.TryReadIfdEntryHeader(ms, 0, 4, true, out _, out _);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void TryReadIfdEntryHeader_WithZeroEntryCount_ReturnsFalse()
	{
		// When entryCount is 0, TryReadIfdEntryHeader returns false (line 571-573)
		using var ms = new MemoryStream("\0\0"u8.ToArray()); // entryCount = 0

		var result =
			TiffEmbeddedPreviewExtractor.TryReadIfdEntryHeader(ms, 0, 100, true, out _, out _);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void TryReadIfdEntryHeader_WithEntryCountTooLarge_ReturnsFalse()
	{
		// When entryCount > 512, TryReadIfdEntryHeader returns false (line 571-573)
		using var ms = new MemoryStream([244, 1]); // entryCount = 500

		var result =
			TiffEmbeddedPreviewExtractor.TryReadIfdEntryHeader(ms, 0, 100, true, out _, out _);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void TryResolveMakerNoteOffset_WithZeroOffset_ReturnsFalse()
	{
		// When rawOffset is 0, TryResolveMakerNoteOffset returns false (line 639-641)
		using var ms = new MemoryStream(new byte[100]);

		var result = TiffEmbeddedPreviewExtractor.TryResolveMakerNoteOffset(ms, 10, 0, out _);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void TryResolveMakerNoteOffset_WithValidAbsoluteJpeg_ReturnsTrue()
	{
		// When absolute offset points to valid JPEG, returns true (line 644-647)
		using var ms = new MemoryStream(new byte[200]);
		ms.Seek(50, SeekOrigin.Begin);
		ms.Write([0xFF, 0xD8, 0xFF], 0, 3); // JPEG marker
		ms.Seek(0, SeekOrigin.Begin);

		var result = TiffEmbeddedPreviewExtractor.TryResolveMakerNoteOffset(ms, 10, 50, out _);
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void TryResolveMakerNoteOffset_WithValidRelativeJpeg_ReturnsTrue()
	{
		// When relative offset points to valid JPEG, returns true (line 650-654)
		using var ms = new MemoryStream(new byte[200]);
		ms.Seek(60, SeekOrigin.Begin); // 10 (base) + 50 (offset)
		ms.Write([0xFF, 0xD8, 0xFF], 0, 3); // JPEG marker
		ms.Seek(0, SeekOrigin.Begin);

		var result = TiffEmbeddedPreviewExtractor.TryResolveMakerNoteOffset(ms, 10, 50, out _);
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void TryResolveMakerNoteOffset_NoJpegAtOffsets_ReturnsFalse()
	{
		// Arrange: stream length large enough but no JPEG markers at the offsets
		var data = new byte[200];
		for ( var i = 0; i < data.Length; i++ )
		{
			data[i] = 0x00; // ensure no 0xFF 0xD8 0xFF sequences
		}

		using var ms = new MemoryStream(data);

		// Act: rawOffset is within length but not a JPEG, relative (makerNoteBase + rawOffset) also not a JPEG
		var result =
			TiffEmbeddedPreviewExtractor.TryResolveMakerNoteOffset(ms, 10, 50,
				out var resolvedOffset);

		// Assert
		Assert.IsFalse(result);
		Assert.AreEqual(0u, resolvedOffset);
	}

	[TestMethod]
	public void ParseCanonMakerNote_WithNoExplicitCandidate_FallsBackToScan()
	{
		// When no explicit JPEG found in IFD, falls back to scan (line 507-520)
		using var ms = new MemoryStream(new byte[300]);
		// Write a JPEG at position 100
		ms.Seek(100, SeekOrigin.Begin);
		ms.Write([0xFF, 0xD8, 0xFF], 0, 3);
		ms.Seek(0, SeekOrigin.Begin);

		var previews = new List<PreviewCandidate>();

		TiffEmbeddedPreviewExtractor.ParseCanonMakerNote(ms, 50, 200, true, previews);
		Assert.IsNotNull(previews);
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
		var data = "II*\0\t\0\0\0"u8.ToArray();
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
		var context = new ParseTraversalContext
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
	public void ParseNextIfd_WithTooShortRead_ReturnsEarly()
	{
		// Arrange
		var context = new ParseTraversalContext
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
		var context = new ParseTraversalContext
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
		TiffEmbeddedPreviewExtractor.AddSubIfdOffsets(ms, true, subIfdOffsets, 4, 2, 50);

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

		// Act
		// blockLength = 5 (< 6)
		var result =
			TiffEmbeddedPreviewExtractor.TryReadIfdEntryHeader(ms, 8, 5, true, out _, out _);

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void TryReadIfdEntryHeader_WithEntryCountReadFailure_ReturnsFalse()
	{
		// Arrange
		using var ms = new MemoryStream(new byte[2]);
		ms.Seek(1, SeekOrigin.Begin); // Only 1 byte left

		// Act
		var result =
			TiffEmbeddedPreviewExtractor.TryReadIfdEntryHeader(ms, 1, 10, true, out _, out _);

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void ReadIfdTagPair_WhenTryReadFullFails_ReturnsFalse()
	{
		// Arrange: Stream has an entry count of 2 (24 bytes expected) but no entry bytes follow
		using var ms = new MemoryStream([0x02, 0x00]); // entryCount = 2
		var query = new IfdTagPairQuery(0x2010, 0x2011, 0, true);

		// Act
		var result = TiffEmbeddedPreviewExtractor.ReadIfdTagPair(ms, 0, 1000, query);

		// Assert: reading the full entries should fail and method should return false,0,0
		Assert.IsFalse(result.HasPair);
		Assert.AreEqual(0u, result.CandidateOffset);
		Assert.AreEqual(0u, result.CandidateLength);
	}

	[TestMethod]
	public void IsJpegAtOffset_WithReadFailure_ReturnsFalse()
	{
		// Arrange
		using var ms = new MemoryStream(new byte[2]); // Only 2 bytes, needs 3

		// Act
		var result = TiffEmbeddedPreviewExtractor.IsJpegAtOffset(ms, 0);

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void TryResolveMakerNoteOffset_WithAbsoluteOffsetFound_ReturnsTrue()
	{
		// Arrange
		var data = new byte[100];
		data[50] = 0xFF;
		data[51] = 0xD8;
		data[52] = 0xFF; // JPEG signature at 50
		using var ms = new MemoryStream(data);

		// Act
		var result =
			TiffEmbeddedPreviewExtractor.TryResolveMakerNoteOffset(ms, 10, 50,
				out var resolvedOffset);

		// Assert
		Assert.IsTrue(result);
		Assert.AreEqual(50u, resolvedOffset);
	}

	[TestMethod]
	public void TryResolveMakerNoteOffset_WithRelativeOffsetFound_ReturnsTrue()
	{
		// Arrange
		var data = new byte[100];
		data[60] = 0xFF;
		data[61] = 0xD8;
		data[62] = 0xFF; // JPEG signature at 60 (10+50)
		using var ms = new MemoryStream(data);

		// Act
		var result =
			TiffEmbeddedPreviewExtractor.TryResolveMakerNoteOffset(ms, 10, 50,
				out var resolvedOffset);

		// Assert
		Assert.IsTrue(result);
		Assert.AreEqual(60u, resolvedOffset);
	}

	[TestMethod]
	public void ParseMakerNote_WithClampedBoundedLength_ReturnsEarly()
	{
		// Arrange
		using var ms = new MemoryStream(new byte[100]);
		var previews = new List<PreviewCandidate>();

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
		var previews = new List<PreviewCandidate>();

		// Act: makerNoteOffset = 101 -> boundedLength = 0
		TiffEmbeddedPreviewExtractor.ParseMakerNote(ms, true, RawFlavor.SonyArw, 101, 10, previews);

		// Assert
		Assert.IsEmpty(previews);
	}

	[TestMethod]
	public void ParseMakerNote_WithPartialBoundedLength_TooSmallJpeg_ReturnsEmpty()
	{
		// Arrange
		var data = new byte[250];
		// At 100, put a Sony MakerNote (minimal)
		data[100] = 1;
		data[101] = 0; // 1 entry
		data[102] = 0x10;
		data[103] = 0x20; // TagSonyPreviewOffset
		data[104] = 4;
		data[105] = 0; // Type LONG
		data[106] = 1;
		data[107] = 0;
		data[108] = 0;
		data[109] = 0; // n=1
		data[110] = 200;
		data[111] = 0;
		data[112] = 0;
		data[113] = 0; // value=200

		// JPEG at 200
		data[200] = 0xFF;
		data[201] = 0xD8;
		data[202] = 0xFF;

		using var ms = new MemoryStream(data);
		var previews = new List<PreviewCandidate>();

		// Act: makerNoteOffset = 100, makerNoteLength = 1000 (beyond length 250)
		// input.Length - 100 = 150. boundedLength = 150.
		// TryReadIfdEntryHeader: blockLength = 150. entryBytes = 1*12 = 12. 12+6=18 <= 150 (OK)
		TiffEmbeddedPreviewExtractor.ParseMakerNote(ms, true, RawFlavor.SonyArw, 100, 1000,
			previews);

		// Assert: preview not added because detected/declared length is below MinJpegSize
		Assert.IsEmpty(previews);
	}

	[TestMethod]
	public void ParseMakerNote_WithUnknownRawFlavor_ReturnsEarly()
	{
		// Arrange
		using var ms = new MemoryStream(new byte[200]);
		var previews = new List<PreviewCandidate>();

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
		data[8] = 0x01;
		data[9] = 0x02; // 513 entries
		using var ms = new MemoryStream(data);

		// Act
		var result =
			TiffEmbeddedPreviewExtractor.TryReadIfdEntryHeader(ms, 8, 100, true, out _, out _);

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void ExtractTagPairValues_WithNGreaterThanOne_SkipsEntry()
	{
		// Arrange
		// TagSonyPreviewOffset, n=2 (should skip)
		var entries = new byte[12];
		entries[0] = 0x10;
		entries[1] = 0x20; // tag
		entries[2] = 4;
		entries[3] = 0; // type LONG
		entries[4] = 2;
		entries[5] = 0;
		entries[6] = 0;
		entries[7] = 0; // n=2
		entries[8] = 100;
		entries[9] = 0;
		entries[10] = 0;
		entries[11] = 0; // value

		var query = new IfdTagPairQuery(0x2010, 0x2011, 0, true);

		// Act
		var (offset, _) = TiffEmbeddedPreviewExtractor.ExtractTagPairValues(entries, 1, query);

		// Assert
		Assert.AreEqual(0u, offset);
	}

	[TestMethod]
	public void HandleIfdEntry_WithSubIfdsTagAndNOne_AddsDirectly()
	{
		// Arrange
		var entry = new byte[12];
		entry[0] = 0x4A;
		entry[1] = 0x01; // TagSubIfds
		entry[2] = 4;
		entry[3] = 0; // LONG
		entry[4] = 1;
		entry[5] = 0;
		entry[6] = 0;
		entry[7] = 0; // n=1
		entry[8] = 50;
		entry[9] = 0;
		entry[10] = 0;
		entry[11] = 0; // value=50

		var subIfdOffsets = new List<uint>();
		var state = new IfdEntryState();
		using var ms = new MemoryStream();

		// Act
		TiffEmbeddedPreviewExtractor.HandleIfdEntry(ms, entry, true, subIfdOffsets, state);

		// Assert
		Assert.HasCount(1, subIfdOffsets);
		Assert.AreEqual(50u, subIfdOffsets[0]);
	}

	[TestMethod]
	public void HandleIfdEntry_WithMakerNoteTag_SetsState()
	{
		// Arrange
		var entry = new byte[12];
		entry[0] = 0x7C;
		entry[1] = 0x92; // TagMakerNote
		entry[2] = 7;
		entry[3] = 0; // UNDEFINED
		entry[4] = 100;
		entry[5] = 0;
		entry[6] = 0;
		entry[7] = 0; // n=100
		entry[8] = 50;
		entry[9] = 0;
		entry[10] = 0;
		entry[11] = 0; // value=50

		var subIfdOffsets = new List<uint>();
		var state = new IfdEntryState();
		using var ms = new MemoryStream();

		// Act
		TiffEmbeddedPreviewExtractor.HandleIfdEntry(ms, entry, true, subIfdOffsets, state);

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
		entry[0] = 0x7C;
		entry[1] = 0x92;
		entry[4] = 4; // n=4 (too small, needs > 4)
		entry[8] = 50;

		var subIfdOffsets = new List<uint>();
		var state = new IfdEntryState();
		using var ms = new MemoryStream();

		// Act
		TiffEmbeddedPreviewExtractor.HandleIfdEntry(ms, entry, true, subIfdOffsets, state);

		// Assert
		Assert.IsFalse(state.HasMakerNote);
	}

	[TestMethod]
	public void AppendDirectJpegCandidate_WithStripTooSmall_Skips()
	{
		// Arrange
		var previews = new List<PreviewCandidate>();
		var state = new IfdEntryState
		{
			IfdCompression = 6,
			HasStrip = true,
			StripOffset = 100,
			StripLength = 1024 // Too small (< 4096)
		};
		using var ms = new MemoryStream();

		// Act
		TiffEmbeddedPreviewExtractor.AppendDirectJpegCandidate(previews, state, ms,
			RawFlavor.Unknown);

		// Assert
		Assert.IsEmpty(previews);
	}

	[TestMethod]
	public void ReadScalarValue_WithVariousTypes_ReturnsExpected()
	{
		// Type 3 (SHORT), littleEndian = true
		Assert.AreEqual(0x1234u, TiffEmbeddedPreviewExtractor.ReadScalarValue(3, 0x1234, true));
		// Type 3 (SHORT), littleEndian = false
		Assert.AreEqual(0x1234u,
			TiffEmbeddedPreviewExtractor.ReadScalarValue(3, 0x12340000, false));
		// Type 4 (LONG)
		Assert.AreEqual(0x12345678u,
			TiffEmbeddedPreviewExtractor.ReadScalarValue(4, 0x12345678, true));
		// Type 99 (Unknown)
		Assert.AreEqual(0u, TiffEmbeddedPreviewExtractor.ReadScalarValue(99, 0x12345678, true));
	}

	[TestMethod]
	[DataRow(1, false)]
	[DataRow(2, true)]
	[DataRow(3, false)]
	[DataRow(4, true)]
	[DataRow(5, false)]
	[DataRow(6, false)]
	public void IsLosslessJpegAtOffset_WithVariousHeaders_ReturnsExpected(int index,
		bool expected)
	{
		var ms1 = new MemoryStream();
		switch ( index )
		{
			case 1:
				// FF D8 FF C4 ... FF C0 = baseline JPEG starting with DHT -> False
				ms1 = new MemoryStream([
					0xFF, 0xD8,
					0xFF, 0xC4, 0x00, 0x04, 0x00, 0x00,
					0xFF, 0xDB, 0x00, 0x04, 0x00, 0x00,
					0xFF, 0xC0, 0x00, 0x0B, 0x08, 0x00, 0x01, 0x00, 0x01, 0x01, 0x01,
					0x11, 0x00
				]);
				break;
			case 2:
				// FF D8 FF C4 ... FF C3 (lossless SOF3) -> True
				ms1 = new MemoryStream([
					0xFF, 0xD8,
					0xFF, 0xC4, 0x00, 0x04, 0x00, 0x00,
					0xFF, 0xC3, 0x00, 0x0B, 0x08, 0x00, 0x01, 0x00, 0x01, 0x01, 0x01,
					0x11, 0x00
				]);
				break;
			case 3:
				// FF D8 FF C4 ... (DHT alone, no SOF marker) -> False (scan completes without finding SOF)
				ms1 = new MemoryStream([0xFF, 0xD8, 0xFF, 0xC4, 0x00, 0x04]);
				break;
			case 4:
				// FF D8 FF C3 (SOF3 lossless) -> True
				ms1 = new MemoryStream([
					0xFF, 0xD8, 0xFF, 0xC3, 0x00, 0x08, 0x08, 0x00, 0x01, 0x00, 0x01, 0x01
				]);
				break;
			case 5:
				// FF D8 FF E0 (APP0 marker, normal JPEG) -> False
				ms1 = new MemoryStream([
					0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46
				]);
				break;
			case 6:
				// Too short (< 4 bytes) -> False
				ms1 = new MemoryStream([0xFF, 0xD8, 0xFF]);
				break;
		}

		Assert.AreEqual(expected, TiffEmbeddedPreviewExtractor.IsLosslessJpegAtOffset(ms1, 0));
	}

	[TestMethod]
	public void IsJpegCompression_WithVariousValues_ReturnsExpected()
	{
		Assert.IsTrue(TiffEmbeddedPreviewExtractor.IsJpegCompression(6));
		Assert.IsTrue(TiffEmbeddedPreviewExtractor.IsJpegCompression(7));
		Assert.IsFalse(TiffEmbeddedPreviewExtractor.IsJpegCompression(1));
	}

	[TestMethod]
	public void ScanJpegsInRange_WithTooShortRange_ReturnsEmpty()
	{
		using var ms = new MemoryStream([0xFF, 0xD8, 0xFF]);
		var result = TiffEmbeddedPreviewExtractor.ScanJpegsInRange(ms, 0, 3);
		Assert.IsEmpty(new List<PreviewCandidate?>(result));
	}

	[TestMethod]
	public void TryBuildScanCandidate_WithLosslessJpeg_ReturnsFalse()
	{
		using var ms = new MemoryStream([0xFF, 0xD8, 0xFF, 0xC4, 0x00, 0x00, 0x00, 0x00]);
		var result =
			TiffEmbeddedPreviewExtractor.TryBuildScanCandidate(ms, 0, 8, out var candidate);
		Assert.IsFalse(result);
		Assert.IsNull(candidate);
	}

	[TestMethod]
	public void TryExtract_WithNonExistentFile_ReturnsFalse()
	{
		var logger = new FakeIWebLogger();
		var storage = new FakeSelectorStorage();
		var extractor = new TiffEmbeddedPreviewExtractor(logger, storage);

		var result = extractor.TryExtract("none", "out").GetAwaiter().GetResult();
		Assert.IsFalse(result);
	}

	[TestMethod]
	[DataRow(1, false)] // Progressive JPEG (SOF2) - baseline, not lossless
	[DataRow(2, false)] // Arithmetic encoding SOF9 - baseline with arithmetic
	[DataRow(3, false)] // Arithmetic encoding SOF10 - progressive with arithmetic
	[DataRow(4, true)]  // Hierarchical lossless SOF11 - true lossless
	[DataRow(5, false)] // APP1 (EXIF) marker before SOF
	[DataRow(6, false)] // Multiple JPG restart markers in sequence
	[DataRow(7, false)] // DQT quantization table with maximum length (67-byte)
	[DataRow(8, false)] // Dual JPEG concatenation (EOI + new SOI)
	[DataRow(9, false)] // COM comment marker (0xFFFE)
	[DataRow(10, false)] // SOF5 differential baseline
	[DataRow(11, false)] // SOF6 differential progressive
	[DataRow(12, false)] // APP13 (Photoshop) marker (Canon/Nikon quirk)
	[DataRow(13, false)] // Reserved marker 0xFFF0 followed by SOF
	[DataRow(14, true)]  // SOF7 lossless with JFIF (rare but valid)
	[DataRow(15, false)] // DHT zero-length marker edge case
	[DataRow(16, false)] // RSTm marker (D0-D7) before SOF
	[DataRow(17, false)] // JFif (APP0) with unusual length
	[DataRow(18, false)] // Extended sequential DCT (SOF1)
	[DataRow(19, false)] // Incomplete SOF marker (missing fields)
	[DataRow(20, false)] // Multiple APP markers stacked (Canon MakerNote signature)
	public void IsLosslessJpegAtOffset_WithCameraManufacturerQuirks_ReturnsExpected(int index,
		bool expected)
	{
		var ms = new MemoryStream();
		switch ( index )
		{
			case 1:
				// FF D8 FF C2 (Progressive baseline, SOF2) -> False
				// Progressive JPEGs are still lossy, just with multiple passes
				ms = new MemoryStream([
					0xFF, 0xD8,
					0xFF, 0xC2, 0x00, 0x0B, 0x08, 0x00, 0x01, 0x00, 0x01, 0x01, 0x01,
					0x11, 0x00
				]);
				break;
			case 2:
				// FF D8 FF C9 (Arithmetic baseline, SOF9) -> False
				// Arithmetic-encoded baseline from some high-end cameras
				ms = new MemoryStream([
					0xFF, 0xD8,
					0xFF, 0xC9, 0x00, 0x0B, 0x08, 0x00, 0x01, 0x00, 0x01, 0x01, 0x01,
					0x11, 0x00
				]);
				break;
			case 3:
				// FF D8 FF CA (Arithmetic progressive, SOF10) -> False
				// Arithmetic-encoded progressive (rare, used in some scientific cameras)
				ms = new MemoryStream([
					0xFF, 0xD8,
					0xFF, 0xCA, 0x00, 0x0B, 0x08, 0x00, 0x01, 0x00, 0x01, 0x01, 0x01,
					0x11, 0x00
				]);
				break;
			case 4:
				// FF D8 FF CB (Hierarchical/lossless arithmetic, SOF11) -> True
				// Four-component lossless with arithmetic coding (some Olympus/Panasonic)
				ms = new MemoryStream([
					0xFF, 0xD8,
					0xFF, 0xCB, 0x00, 0x0B, 0x08, 0x00, 0x01, 0x00, 0x01, 0x01, 0x01,
					0x11, 0x00
				]);
				break;
			case 5:
				// APP1 (EXIF thumbnail) before SOF -> False
				// Common in TIFF-embedded JPEG with EXIF data
				ms = new MemoryStream([
					0xFF, 0xD8,
					0xFF, 0xE1, 0x00, 0x10, // APP1 with length 16
					0x45, 0x78, 0x69, 0x66, // "Exif"
					0xFF, 0xC0, 0x00, 0x0B, 0x08, 0x00, 0x01, 0x00, 0x01, 0x01, 0x01,
					0x11, 0x00
				]);
				break;
			case 6:
				// Sequence of RSTm (restart) markers D0-D4 before SOF -> False
				// Used by some firmware to align data segments
				ms = new MemoryStream([
					0xFF, 0xD8,
					0xFF, 0xD0, 0xFF, 0xD1, 0xFF, 0xD2,
					0xFF, 0xC0, 0x00, 0x0B, 0x08, 0x00, 0x01, 0x00, 0x01, 0x01, 0x01,
					0x11, 0x00
				]);
				break;
			case 7:
				// DQT (0xFFDB) with maximum 67-byte quantization table -> False
				// Nikon/Canon high-quality JPEG headers
				ms = new MemoryStream([
					0xFF, 0xD8,
					0xFF, 0xDB, 0x00, 0x43, // DQT with 67 bytes (0-3 are padding/precision/table)
					0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09,
					0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x11, 0x12, 0x13,
					0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D,
					0x1E, 0x1F, 0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27,
					0x28, 0x29, 0x2A, 0x2B, 0x2C, 0x2D, 0x2E, 0x2F, 0x30, 0x31,
					0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3A, 0x3B,
					0x3C, 0x3D, 0x3E, 0x3F,
					0xFF, 0xC0, 0x00, 0x0B, 0x08, 0x00, 0x01, 0x00, 0x01, 0x01, 0x01,
					0x11, 0x00
				]);
				break;
			case 8:
				// EOI + new SOI (concatenated JPEG, thumbnail case) -> False
				// Some Sony/Fuji cameras store multiple previews concatenated
				ms = new MemoryStream([
					0xFF, 0xD8,
					0xFF, 0xC0, 0x00, 0x0B, 0x08, 0x00, 0x01, 0x00, 0x01, 0x01, 0x01,
					0x11, 0x00,
					0xFF, 0xD9, // EOI marker
					0xFF, 0xD8, // New SOI
					0xFF, 0xC3, 0x00, 0x0B, 0x08, 0x00, 0x01, 0x00, 0x01, 0x01, 0x01,
					0x11, 0x00
				]);
				break;
			case 9:
				// COM (comment) marker 0xFFFE followed by baseline SOF -> False
				// Metadata comment added by some Canon firmware
				ms = new MemoryStream([
					0xFF, 0xD8,
					0xFF, 0xFE, 0x00, 0x08, 0x54, 0x65, 0x73, 0x74, // COM + "Test"
					0xFF, 0xC0, 0x00, 0x0B, 0x08, 0x00, 0x01, 0x00, 0x01, 0x01, 0x01,
					0x11, 0x00
				]);
				break;
			case 10:
				// SOF5 (differential sequential DCT) -> False
				// Non-standard variant used in some scientific imaging
				ms = new MemoryStream([
					0xFF, 0xD8,
					0xFF, 0xC5, 0x00, 0x0B, 0x08, 0x00, 0x01, 0x00, 0x01, 0x01, 0x01,
					0x11, 0x00
				]);
				break;
			case 11:
				// SOF6 (differential progressive DCT) -> False
				// Another non-standard variant
				ms = new MemoryStream([
					0xFF, 0xD8,
					0xFF, 0xC6, 0x00, 0x0B, 0x08, 0x00, 0x01, 0x00, 0x01, 0x01, 0x01,
					0x11, 0x00
				]);
				break;
			case 12:
				// APP13 (0xFFED) Photoshop/IPTCnews marker (Nikon quirk) -> False
				// Nikon embeds MakerNote info in APP13
				ms = new MemoryStream([
					0xFF, 0xD8,
					0xFF, 0xED, 0x00, 0x08, 0x50, 0x68, 0x6F, 0x74, // APP13 + "Phot"
					0xFF, 0xC0, 0x00, 0x0B, 0x08, 0x00, 0x01, 0x00, 0x01, 0x01, 0x01,
					0x11, 0x00
				]);
				break;
			case 13:
				// Reserved marker 0xFFF0 (undefined) followed by SOF0 -> False
				// Some firmware writes reserved markers by mistake
				ms = new MemoryStream([
					0xFF, 0xD8,
					0xFF, 0xF0, 0x00, 0x04, 0x00, 0x00, // Reserved marker with content
					0xFF, 0xC0, 0x00, 0x0B, 0x08, 0x00, 0x01, 0x00, 0x01, 0x01, 0x01,
					0x11, 0x00
				]);
				break;
			case 14:
				// SOF7 (lossless with arithmetic, rare) -> True
				// Lossless encoding with arithmetic coder (supported in JPEG baseline spec)
				ms = new MemoryStream([
					0xFF, 0xD8,
					0xFF, 0xC7, 0x00, 0x0B, 0x08, 0x00, 0x01, 0x00, 0x01, 0x01, 0x01,
					0x11, 0x00
				]);
				break;
			case 15:
				// DHT with zero length edge case -> False
				// Some old firmware incorrectly writes zero-length DHT
				ms = new MemoryStream([
					0xFF, 0xD8,
					0xFF, 0xC4, 0x00, 0x00, // DHT zero-length (malformed)
					0xFF, 0xC0, 0x00, 0x0B, 0x08, 0x00, 0x01, 0x00, 0x01, 0x01, 0x01,
					0x11, 0x00
				]);
				break;
			case 16:
				// RSTm markers before baseline SOF -> False
				// Improper data alignment from buggy encoders
				ms = new MemoryStream([
					0xFF, 0xD8,
					0xFF, 0xD7, 0xFF, 0xD6, 0xFF, 0xD5, // Multiple restart markers
					0xFF, 0xC0, 0x00, 0x0B, 0x08, 0x00, 0x01, 0x00, 0x01, 0x01, 0x01,
					0x11, 0x00
				]);
				break;
			case 17:
				// APP0 (JFIF) with unusual length of 20 -> False
				// Canon EOS cameras sometimes use extended APP0
				ms = new MemoryStream([
					0xFF, 0xD8,
					0xFF, 0xE0, 0x00, 0x14, 0x4A, 0x46, 0x49, 0x46, 0x00,
					0x01, 0x01, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00,
					0xFF, 0xC0, 0x00, 0x0B, 0x08, 0x00, 0x01, 0x00, 0x01, 0x01, 0x01,
					0x11, 0x00
				]);
				break;
			case 18:
				// SOF1 (extended sequential DCT) -> False
				// Some HPx cameras use extended mode
				ms = new MemoryStream([
					0xFF, 0xD8,
					0xFF, 0xC1, 0x00, 0x0B, 0x08, 0x00, 0x01, 0x00, 0x01, 0x01, 0x01,
					0x11, 0x00
				]);
				break;
			case 19:
				// Incomplete SOF marker (missing precision/dimensions) -> False
				// Corrupted header from camera crash during JPEG encoding
				ms = new MemoryStream([
					0xFF, 0xD8,
					0xFF, 0xC0, 0x00, 0x04 // Incomplete SOF
				]);
				break;
			case 20:
				// Multiple consecutive APP markers (Canon EOS pattern) -> False
				// Canon embeds multiple marker types for maker data
				ms = new MemoryStream([
					0xFF, 0xD8,
					0xFF, 0xE0, 0x00, 0x04, 0x00, 0x00, // APP0
					0xFF, 0xE1, 0x00, 0x04, 0x00, 0x00, // APP1 (EXIF)
					0xFF, 0xED, 0x00, 0x04, 0x00, 0x00, // APP13 (Photoshop)
					0xFF, 0xC0, 0x00, 0x0B, 0x08, 0x00, 0x01, 0x00, 0x01, 0x01, 0x01,
					0x11, 0x00
				]);
				break;
		}

		Assert.AreEqual(expected, TiffEmbeddedPreviewExtractor.IsLosslessJpegAtOffset(ms, 0),
			$"Case {index} failed: expected {expected}");
	}
}
