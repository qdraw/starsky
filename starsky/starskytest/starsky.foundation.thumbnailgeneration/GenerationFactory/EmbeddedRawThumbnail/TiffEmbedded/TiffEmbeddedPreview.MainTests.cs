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
	[DataRow(3, true)]
	[DataRow(4, true)]
	[DataRow(5, true)]
	[DataRow(6, true)]
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
				ms1 = new MemoryStream([
					0xFF, 0xD8,
					0xFF, 0xC4, 0x00, 0x04, 0x00, 0x00,
					0xFF, 0xC3, 0x00, 0x0B, 0x08, 0x00, 0x01, 0x00, 0x01, 0x01, 0x01,
					0x11, 0x00
				]);
				break;
			case 3:
				// FF D8 FF C4 (DHT) -> True
				ms1 = new MemoryStream([0xFF, 0xD8, 0xFF, 0xC4]);
				break;
			case 4:
				// FF D8 FF C3 (SOF3) -> True
				ms1 = new MemoryStream([0xFF, 0xD8, 0xFF, 0xC3]);
				break;
			case 5:
				// FF D8 FF E0 (Normal JPEG) -> False
				ms1 = new MemoryStream([0xFF, 0xD8, 0xFF, 0xC3]);
				break;
			case 6:
				// Too short -> False
				ms1 = new MemoryStream([0xFF, 0xD8, 0xFF, 0xC3]);
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
}
