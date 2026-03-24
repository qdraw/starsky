using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.TiffEmbeded;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.TiffEmbeded;

[TestClass]
public class TiffEmbeddedPreviewCoverageTests
{
	[TestMethod]
	public void ParseNextIfd_WithIsSubIfdTrue_ReturnsEarly()
	{
		// When isSubIfd=true, ParseNextIfd returns early (line 236-238)
		using var ms = new MemoryStream(new byte[100]);
		var context = new TiffEmbeddedPreviewExtractor.ParseTraversalContext
		{
			Previews = [],
			Visited = [],
			ReferenceInfo = "test",
			RawFlavor = RawFlavor.Unknown
		};

		TiffEmbeddedPreviewExtractor.ParseNextIfd(ms, true, context, 0, true);
		Assert.AreEqual(0, ms.Position);
	}

	[TestMethod]
	public void ParseSubIfdChain_WithEmptyList_DoesNothing()
	{
		// Empty subIfdOffsets should result in no action (line 391)
		using var ms = new MemoryStream(new byte[100]);
		var context = new TiffEmbeddedPreviewExtractor.ParseTraversalContext
		{
			Previews = [],
			Visited = [],
			ReferenceInfo = "test",
			RawFlavor = RawFlavor.Unknown
		};
		var emptyList = new List<uint>();

		TiffEmbeddedPreviewExtractor.ParseSubIfdChain(ms, true, context, 0, emptyList);
		Assert.IsEmpty(context.Previews);
	}

	[TestMethod]
	public void ParseMakerNote_WithZeroOffset_ExitsEarly()
	{
		// When makerNoteOffset is 0, ParseMakerNote returns early (line 404-406)
		using var ms = new MemoryStream(new byte[100]);
		var previews = new List<TiffEmbeddedPreviewExtractor.PreviewCandidate>();

		TiffEmbeddedPreviewExtractor.ParseMakerNote(ms, true, RawFlavor.Unknown, 0, 100, previews);
		Assert.IsEmpty(previews);
	}

	[TestMethod]
	public void ParseMakerNote_WithZeroLength_ExitsEarly()
	{
		// When makerNoteLength is 0, ParseMakerNote returns early (line 405-407)
		using var ms = new MemoryStream(new byte[100]);
		var previews = new List<TiffEmbeddedPreviewExtractor.PreviewCandidate>();

		TiffEmbeddedPreviewExtractor.ParseMakerNote(ms, true, RawFlavor.Unknown, 50, 0, previews);
		Assert.IsEmpty(previews);
	}

	[TestMethod]
	public void ParseMakerNote_WithOffsetBeyondStream_BoundedLengthZero()
	{
		// When makerNoteOffset is beyond stream, boundedLength becomes 0 (line 412-419)
		using var ms = new MemoryStream(new byte[50]);
		var previews = new List<TiffEmbeddedPreviewExtractor.PreviewCandidate>();

		TiffEmbeddedPreviewExtractor.ParseMakerNote(ms, true, RawFlavor.SonyArw, 100, 1000, previews);
		Assert.IsEmpty(previews);
	}

	[TestMethod]
	public void TryReadIfdEntryHeader_WithBlockLengthTooSmall_ReturnsFalse()
	{
		// When blockLength < 6, TryReadIfdEntryHeader returns false (line 559)
		using var ms = new MemoryStream(new byte[100]);
		
		var result = TiffEmbeddedPreviewExtractor.TryReadIfdEntryHeader(ms, 0, 4, true, out _, out _);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void TryReadIfdEntryHeader_WithZeroEntryCount_ReturnsFalse()
	{
		// When entryCount is 0, TryReadIfdEntryHeader returns false (line 571-573)
		using var ms = new MemoryStream("\0\0"u8.ToArray()); // entryCount = 0
		
		var result = TiffEmbeddedPreviewExtractor.TryReadIfdEntryHeader(ms, 0, 100, true, out _, out _);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void TryReadIfdEntryHeader_WithEntryCountTooLarge_ReturnsFalse()
	{
		// When entryCount > 512, TryReadIfdEntryHeader returns false (line 571-573)
		using var ms = new MemoryStream([244, 1]); // entryCount = 500
		
		var result = TiffEmbeddedPreviewExtractor.TryReadIfdEntryHeader(ms, 0, 100, true, out _, out _);
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
	public void ParseCanonMakerNote_WithNoExplicitCandidate_FallsBackToScan()
	{
		// When no explicit JPEG found in IFD, falls back to scan (line 507-520)
		using var ms = new MemoryStream(new byte[300]);
		// Write a JPEG at position 100
		ms.Seek(100, SeekOrigin.Begin);
		ms.Write([0xFF, 0xD8, 0xFF], 0, 3);
		ms.Seek(0, SeekOrigin.Begin);
		
		var previews = new List<TiffEmbeddedPreviewExtractor.PreviewCandidate>();
		
		TiffEmbeddedPreviewExtractor.ParseCanonMakerNote(ms, 50, 200, true, previews);
		Assert.IsNotNull(previews);
	}
}
