using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.Models;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.TiffEmbedded;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.
	TiffEmbedded;

[TestClass]
public class TiffEmbeddedPreviewLengthTests
{
	[TestMethod]
	public void ParseCanonMakerNote_ResolvedLengthLessThanMinJpegSize_CallsDetectJpegLengthByEoi()
	{
		// TagCanonPreviewOffset = 0x0001
		// TagCanonPreviewLength = 0x0004
		// MinJpegSize = 4096

		using var ms = new MemoryStream(new byte[10000]);
		const bool littleEndian = true;
		const uint makerNoteOffset = 100u;
		const uint makerNoteLength = 500u;

		// Write IFD at makerNoteOffset
		ms.Position = makerNoteOffset;
		ms.Write([0x02, 0x00], 0, 2); // 2 entries

		// Entry 1: Offset tag 0x0001
		ms.Write([0x01, 0x00], 0, 2); // Tag 0x0001
		ms.Write([0x04, 0x00], 0, 2); // Type LONG (4)
		ms.Write([0x01, 0x00, 0x00, 0x00], 0, 4); // Count 1
		ms.Write([0x00, 0x03, 0x00, 0x00], 0, 4); // Value 0x300 = 768

		// Entry 2: Length tag 0x0004
		ms.Write([0x04, 0x00], 0, 2); // Tag 0x0004
		ms.Write([0x04, 0x00], 0, 2); // Type LONG (4)
		ms.Write([0x01, 0x00, 0x00, 0x00], 0, 4); // Count 1
		ms.Write([0x00, 0x01, 0x00, 0x00], 0, 4); // Value 0x100 = 256 (< 4096)

		// Write JPEG at 0x300 (768)
		ms.Position = 0x300;
		ms.Write([0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46], 0, 10);

		// Write EOI at 0x300 + 5000
		ms.Position = 0x300 + 5000;
		ms.Write([0xFF, 0xD9], 0, 2);

		var previews = new List<PreviewCandidate>();
		TiffEmbeddedPreviewExtractor.ParseCanonMakerNote(ms, makerNoteOffset, makerNoteLength,
			littleEndian, previews);

		Assert.HasCount(1, previews);
		Assert.AreEqual(0x300u, previews[0].Offset);
		// 5000 + 2 (EOI) = 5002
		Assert.AreEqual(5002u, previews[0].Length);
	}

	[TestMethod]
	public void ParseCanonMakerNote_ResolvedLengthStillTooSmallAfterDetect_Continues()
	{
		using var ms = new MemoryStream(new byte[10000]);
		var littleEndian = true;
		var makerNoteOffset = 100u;
		var makerNoteLength = 500u;

		// Write IFD at makerNoteOffset
		ms.Position = makerNoteOffset;
		ms.Write([0x02, 0x00], 0, 2); // 2 entries

		// Entry 1: Offset tag 0x0001
		ms.Write([0x01, 0x00], 0, 2);
		ms.Write([0x04, 0x00], 0, 2);
		ms.Write([0x01, 0x00, 0x00, 0x00], 0, 4);
		ms.Write([0x00, 0x03, 0x00, 0x00], 0, 4); // Value 0x300 = 768

		// Entry 2: Length tag 0x0004
		ms.Write([0x04, 0x00], 0, 2);
		ms.Write([0x04, 0x00], 0, 2);
		ms.Write([0x01, 0x00, 0x00, 0x00], 0, 4);
		ms.Write([0x00, 0x01, 0x00, 0x00], 0, 4); // Value 256 (< 4096)

		// Write JPEG at 0x300
		ms.Position = 0x300;
		ms.Write([0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46], 0, 10);

		// Write EOI at 0x300 + 1000 (total 1002 < 4096)
		ms.Position = 0x300 + 1000;
		ms.Write([0xFF, 0xD9], 0, 2);

		var previews = new List<PreviewCandidate>();
		// Should continue and NOT add to previews
		TiffEmbeddedPreviewExtractor.ParseCanonMakerNote(ms, makerNoteOffset, makerNoteLength,
			littleEndian, previews);

		// It will fall back to ScanJpegsInRange, which will find it!
		// But we want to test that the explicit candidate was skipped.
		// ScanJpegsInRange starts from makerNoteOffset (100) and scans boundedCanonScan (Min(500, 2MB) = 500)
		// Our JPEG is at 768, which is OUTSIDE 100+500=600.
		// So it should NOT find it via fallback either.
		Assert.IsEmpty(previews);
	}

	[TestMethod]
	public void ParseCanonMakerNote_MaxPreviewsReached_ReturnsEarly()
	{
		using var ms = new MemoryStream(new byte[10000]);
		var littleEndian = true;
		var makerNoteOffset = 100u;
		var makerNoteLength = 500u;

		// Fill previews to 8 (MaxPreviews)
		var previews = new List<PreviewCandidate>();
		for ( var i = 0; i < 8; i++ )
		{
			previews.Add(new PreviewCandidate());
		}

		// Write IFD at makerNoteOffset
		ms.Position = makerNoteOffset;
		ms.Write([0x01, 0x00], 0, 2); // 1 entry (we only need one pair to trigger the loop once)

		// Entry 1: Offset tag 0x0001
		ms.Write([0x01, 0x00], 0, 2);
		ms.Write([0x04, 0x00], 0, 2);
		ms.Write([0x01, 0x00, 0x00, 0x00], 0, 4);
		ms.Write([0x00, 0x03, 0x00, 0x00], 0, 4);

		// TagCanonPreviewLength (0x0004) is NOT in this IFD.
		// ReadIfdTagPair will be called for the first query (TagCanonPreviewOffset, TagCanonPreviewLength).
		// ExtractTagPairValues will find TagCanonPreviewOffset=0x300 and TagCanonPreviewLength=0.
		// ReadIfdTagPair returns (true, 0x300, 0).

		// Write JPEG at 0x300
		ms.Position = 0x300;
		ms.Write([0xFF, 0xD8, 0xFF], 0, 3);

		// We need it to find it as valid JPEG. so TryResolveMakerNoteOffset succeeds.
		// It will then see rawLength = 0 < MinJpegSize (4096).
		// It calls DetectJpegLengthByEoi.
		// If we don't write EOI, it returns 0.
		// 0 < MinJpegSize, so it continues.

		// Wait, I want it to ADD to previews and THEN check if count >= MaxPreviews.
		// So I need resolvedLength >= MinJpegSize.

		ms.Position = 0x300 + 5000;
		ms.Write([0xFF, 0xD9], 0, 2);

		TiffEmbeddedPreviewExtractor.ParseCanonMakerNote(ms, makerNoteOffset, makerNoteLength,
			littleEndian, previews);

		Assert.HasCount(9, previews);
	}

	[TestMethod]
	public void ParseSonyMakerNote_ResolvedLengthLessThanMinJpegSize_CallsDetectJpegLengthByEoi()
	{
		// TagSonyPreviewOffset = 0x2010
		// TagSonyPreviewLength = 0x2011

		using var ms = new MemoryStream(new byte[10000]);
		var littleEndian = true;
		var makerNoteOffset = 100u;
		var makerNoteLength = 500u;

		// Write IFD at makerNoteOffset
		ms.Position = makerNoteOffset;
		ms.Write([0x02, 0x00], 0, 2);

		// Entry 1: Offset tag 0x2010
		ms.Write([0x10, 0x20], 0, 2);
		ms.Write([0x04, 0x00], 0, 2);
		ms.Write([0x01, 0x00, 0x00, 0x00], 0, 4);
		ms.Write([0x00, 0x03, 0x00, 0x00], 0, 4);

		// Entry 2: Length tag 0x2011
		ms.Write([0x11, 0x20], 0, 2);
		ms.Write([0x04, 0x00], 0, 2);
		ms.Write([0x01, 0x00, 0x00, 0x00], 0, 4);
		ms.Write([0x00, 0x01, 0x00, 0x00], 0, 4);

		// Write JPEG at 0x300
		ms.Position = 0x300;
		ms.Write([0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46], 0, 10);

		// Write EOI at 0x300 + 5000
		ms.Position = 0x300 + 5000;
		ms.Write([0xFF, 0xD9], 0, 2);

		var previews = new List<PreviewCandidate>();
		TiffEmbeddedPreviewExtractor.ParseSonyMakerNote(ms, makerNoteOffset, makerNoteLength,
			littleEndian, previews);

		Assert.HasCount(1, previews);
		Assert.AreEqual(5002u, previews[0].Length);
	}
}
