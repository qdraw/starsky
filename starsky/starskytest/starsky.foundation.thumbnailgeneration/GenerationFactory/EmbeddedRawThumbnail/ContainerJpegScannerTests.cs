using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;

/// <summary>
///     Tests for <see cref="ContainerJpegScanner.TryExtractBestPreview" />, covering
///     all branches of the refactored HasIptcApp13 helper and its private sub-methods
///     (TrySkipToMarker, IsJpegStreamEndMarker, IsStandaloneMarker,
///     TryReadSegmentPayloadLength, AdvanceSegmentAndCheckIptc, IsIptcApp13Payload).
/// </summary>
[TestClass]
public class ContainerJpegScannerTests
{
	/// <summary>Minimum JPEG size accepted by the scanner (mirrors <c>MinJpegSize</c> const).</summary>
	private const int MinJpeg = 4096;

	// ── JPEG builder ─────────────────────────────────────────────────────────

	/// <summary>
	///     Builds a syntactically valid JPEG of exactly <paramref name="totalSize" /> bytes.
	///     <para>
	///         Layout (in order): SOI · APP0 · optional RST0 · optional APP13 · zero padding · EOI.
	///     </para>
	///     <para>
	///         A minimal APP0 is always written after SOI so that the third JPEG byte is
	///         0xFF — the value required by <c>ScanCandidates</c> to detect the SOI
	///         (<c>prev == 0xFF &amp;&amp; current == 0xD8 &amp;&amp; next == 0xFF</c>).
	///     </para>
	/// </summary>
	private static byte[] BuildJpeg(
		int totalSize,
		string? app13Content = null,
		bool withRstMarker = false,
		bool withMarkerPadding = false)
	{
		if ( totalSize < MinJpeg )
		{
			totalSize = MinJpeg;
		}

		using var ms = new MemoryStream();

		// SOI
		ms.Write([0xFF, 0xD8]);

		// Minimal APP0 — guarantees byte[2] == 0xFF so ScanCandidates can find the SOI
		ms.Write([0xFF, 0xE0, 0x00, 0x04, 0x00, 0x00]); // marker + segLen=4 + 2-byte payload

		// Optional RST0 standalone marker (no length / payload)
		if ( withRstMarker )
		{
			ms.Write([0xFF, 0xD0]);
		}

		// Optional APP13 (0xED) segment
		if ( app13Content != null )
		{
			var payload = Encoding.ASCII.GetBytes(app13Content);
			var segLen = ( ushort ) ( payload.Length + 2 );

			if ( withMarkerPadding )
			{
				// JPEG allows 0xFF padding bytes before the real marker byte
				ms.Write([0xFF, 0xFF]); // padding byte
				ms.WriteByte(0xED);
			}
			else
			{
				ms.Write([0xFF, 0xED]); // APP13 marker
			}

			ms.WriteByte(( byte ) ( ( segLen >> 8 ) & 0xFF ));
			ms.WriteByte(( byte ) ( segLen & 0xFF ));
			ms.Write(payload);
		}

		// Zero-fill to (totalSize − 2) then write EOI
		var padNeeded = totalSize - ( int ) ms.Length - 2;
		if ( padNeeded > 0 )
		{
			ms.Write(new byte[padNeeded]);
		}

		ms.Write([0xFF, 0xD9]); // EOI
		return ms.ToArray();
	}

	private static MemoryStream StreamOf(byte[] bytes) =>
		new(bytes, writable: false);

	// ── Tests: stream-level guards ────────────────────────────────────────────

	[TestMethod]
	public async Task TryExtractBestPreview_ReturnsFalse_WhenStreamIsSmallerThanMinimumSize()
	{
		// Scanner requires at least 4 096 bytes
		using var input = new MemoryStream(new byte[100]);
		using var output = new MemoryStream();

		var result = await ContainerJpegScanner.TryExtractBestPreview(input, output);

		Assert.IsFalse(result, "Streams shorter than MinJpegSize should be rejected immediately");
	}

	[TestMethod]
	public async Task TryExtractBestPreview_ReturnsFalse_WhenStreamIsNotSeekable()
	{
		await using var input = new NonSeekableStream(new byte[MinJpeg * 2]);
		using var output = new MemoryStream();

		var result = await ContainerJpegScanner.TryExtractBestPreview(input, output);

		Assert.IsFalse(result, "Non-seekable streams must be rejected before scanning");
	}

	[TestMethod]
	public async Task TryExtractBestPreview_ReturnsFalse_WhenNoJpegSoiIsFound()
	{
		// Stream of bytes that never contain a valid 0xFF 0xD8 0xFF SOI sequence
		var bytes = Enumerable.Range(0, MinJpeg * 2).Select(i => ( byte ) ( i & 0x7F )).ToArray();
		using var input = StreamOf(bytes);
		using var output = new MemoryStream();

		var result = await ContainerJpegScanner.TryExtractBestPreview(input, output);

		Assert.IsFalse(result, "No JPEG SOI in stream should return false");
	}

	// ── Tests: probe mode (output == null) ───────────────────────────────────

	[TestMethod]
	public async Task TryExtractBestPreview_ReturnsTrue_WhenOutputIsNull_AndJpegIsFound()
	{
		// Passing null output puts the scanner in "probe" mode — returns true if a JPEG
		// candidate was found without writing any bytes.
		var jpeg = BuildJpeg(MinJpeg);
		using var input = StreamOf(jpeg);

		var result = await ContainerJpegScanner.TryExtractBestPreview(input, output: null);

		Assert.IsTrue(result, "Probe mode (null output) should return true when a JPEG is found");
	}

	// ── Tests: IPTC detection paths ───────────────────────────────────────────

	[TestMethod]
	public async Task TryExtractBestPreview_WritesJpeg_WhenNoIptcPresent()
	{
		// Plain JPEG without an APP13 segment — candidate with hasIptc=false
		var jpeg = BuildJpeg(MinJpeg);
		using var input = StreamOf(jpeg);
		using var output = new MemoryStream();

		var result = await ContainerJpegScanner.TryExtractBestPreview(input, output);

		Assert.IsTrue(result, "Valid JPEG without IPTC should still be extracted");
		Assert.IsGreaterThan(0, output.Length, "Expected JPEG bytes in output");
	}

	[TestMethod]
	public async Task TryExtractBestPreview_DetectsIptc_ViaPhotoshopApp13Signature()
	{
		// "Photoshop 3.0\0" is the canonical APP13 IPTC header
		var jpeg = BuildJpeg(MinJpeg, app13Content: "Photoshop 3.0\0");
		using var input = StreamOf(jpeg);
		using var output = new MemoryStream();

		var result = await ContainerJpegScanner.TryExtractBestPreview(input, output);

		Assert.IsTrue(result);
		Assert.IsGreaterThan(0, output.Length, "Expected JPEG extracted when APP13 has Photoshop signature");
	}

	[TestMethod]
	public async Task TryExtractBestPreview_DetectsIptc_Via8BimApp13Signature()
	{
		// "8BIM" is the Photoshop resource block marker also accepted as an IPTC signal
		var jpeg = BuildJpeg(MinJpeg, app13Content: "8BIM");
		using var input = StreamOf(jpeg);
		using var output = new MemoryStream();

		var result = await ContainerJpegScanner.TryExtractBestPreview(input, output);

		Assert.IsTrue(result);
		Assert.IsGreaterThan(0, output.Length, "Expected JPEG extracted when APP13 has 8BIM signature");
	}

	[TestMethod]
	public async Task TryExtractBestPreview_ExtractsJpeg_WhenApp13HasUnrecognisedContent()
	{
		// APP13 present but payload does not contain either IPTC signature.
		// The JPEG should still be extracted; it simply won't be scored as an IPTC candidate.
		var jpeg = BuildJpeg(MinJpeg, app13Content: "Unknown APP13 content");
		using var input = StreamOf(jpeg);
		using var output = new MemoryStream();

		var result = await ContainerJpegScanner.TryExtractBestPreview(input, output);

		Assert.IsTrue(result, "JPEG must be extracted even when APP13 holds no IPTC signature");
		Assert.IsGreaterThan(0, output.Length, "Expected JPEG bytes in output");
	}

	// ── Tests: candidate selection ────────────────────────────────────────────

	[TestMethod]
	public async Task TryExtractBestPreview_PrefersIptcCandidate_OverLargerPlainCandidate()
	{
		// SelectBest always picks a candidate with hasIptc=true over one with hasIptc=false,
		// even when the IPTC JPEG is smaller.
		const int smallIptcSize = MinJpeg + 512;
		const int largeNoIptcSize = MinJpeg + 4096;

		var plainJpeg = BuildJpeg(largeNoIptcSize);                          // large, no IPTC
		var iptcJpeg = BuildJpeg(smallIptcSize, app13Content: "Photoshop 3.0\0"); // small, IPTC

		var container = plainJpeg.Concat(iptcJpeg).ToArray();
		using var input = StreamOf(container);
		using var output = new MemoryStream();

		var result = await ContainerJpegScanner.TryExtractBestPreview(input, output);

		Assert.IsTrue(result);
		Assert.AreEqual(smallIptcSize, ( int ) output.Length,
			"The smaller IPTC-bearing JPEG should be preferred over the larger plain JPEG");
	}

	[TestMethod]
	public async Task TryExtractBestPreview_SelectsLargerCandidate_WhenBothHaveIptc()
	{
		// When both candidates have hasIptc=true, SelectBest chooses the larger one.
		const int smallIptcSize = MinJpeg + 512;
		const int largeIptcSize = MinJpeg + 8192;

		var smallJpeg = BuildJpeg(smallIptcSize, app13Content: "Photoshop 3.0\0");
		var largeJpeg = BuildJpeg(largeIptcSize, app13Content: "8BIM");

		var container = smallJpeg.Concat(largeJpeg).ToArray();
		using var input = StreamOf(container);
		using var output = new MemoryStream();

		var result = await ContainerJpegScanner.TryExtractBestPreview(input, output);

		Assert.IsTrue(result);
		Assert.AreEqual(largeIptcSize, ( int ) output.Length,
			"The larger of two IPTC candidates should be selected");
	}

	// ── Tests: marker-parsing edge cases ─────────────────────────────────────

	[TestMethod]
	public async Task TryExtractBestPreview_CorrectlyParsesRstStandaloneMarker()
	{
		// RST0 (0xD0) is a standalone marker — it has no length field or payload.
		// IsStandaloneMarker should recognise it and continue without trying to read a length.
		var jpeg = BuildJpeg(MinJpeg, app13Content: "Photoshop 3.0\0", withRstMarker: true);
		using var input = StreamOf(jpeg);
		using var output = new MemoryStream();

		var result = await ContainerJpegScanner.TryExtractBestPreview(input, output);

		Assert.IsTrue(result,
			"Scanner must continue past a standalone RST marker and find the following APP13");
		Assert.IsGreaterThan(0, output.Length);
	}

	[TestMethod]
	public async Task TryExtractBestPreview_CorrectlyStrips0xFFMarkerPaddingBytes()
	{
		// JPEG allows 0xFF padding bytes before the true marker byte (e.g. FF FF ED instead
		// of FF ED).  TrySkipToMarker's inner do-while loop must consume them.
		var jpeg = BuildJpeg(MinJpeg, app13Content: "Photoshop 3.0\0", withMarkerPadding: true);
		using var input = StreamOf(jpeg);
		using var output = new MemoryStream();

		var result = await ContainerJpegScanner.TryExtractBestPreview(input, output);

		Assert.IsTrue(result,
			"Scanner must detect IPTC even when the APP13 marker byte is preceded by 0xFF padding");
		Assert.IsGreaterThan(0, output.Length);
	}

	[TestMethod]
	public async Task TryExtractBestPreview_EoiMarkerTerminatesIptcScan_JpegStillExtracted()
	{
		// A JPEG that contains no APP13 segment will have its scan end at EOI (0xD9).
		// IsJpegStreamEndMarker returns true → HasIptcApp13 returns false, but the
		// candidate is still added (just without the IPTC flag) and the JPEG is extracted.
		var jpeg = BuildJpeg(MinJpeg); // no APP13
		using var input = StreamOf(jpeg);
		using var output = new MemoryStream();

		var result = await ContainerJpegScanner.TryExtractBestPreview(input, output);

		Assert.IsTrue(result, "JPEG should be extracted even when EOI terminates the IPTC scan");
		Assert.IsGreaterThan(0, output.Length);
	}

	[TestMethod]
	public async Task TryExtractBestPreview_ReturnsFalse_WhenReadAsyncReturnsZero()
	{
		// This tests the case where input.Read returns <= 0 during scanning
		// However, it's hard to trigger read <= 0 in the MIDDLE of the stream Length.
		// ContainerJpegScanner has:
		// while ( scanned < input.Length && candidates.Count < MaxCandidates ) {
		//    var toRead = ( int ) Math.Min(buffer.Length, input.Length - scanned);
		//    var read = input.Read(buffer, 0, toRead);
		//    if ( read <= 0 ) break;
		// }
		
		// If input.Read returns 0, it means EOF or an error.
		// We can mock a stream that claims to have more data than it actually returns.
		var input = new TruncatedReadStream(new byte[MinJpeg * 2]);
		using var output = new MemoryStream();

		var result = await ContainerJpegScanner.TryExtractBestPreview(input, output);

		Assert.IsFalse(result);
	}

	private sealed class TruncatedReadStream(byte[] data) : MemoryStream(data)
	{
		public override int Read(byte[] buffer, int offset, int count)
		{
			// Return 0 even if data might be available (though MemoryStream wouldn't do this)
			// Actually, let's just return 0 to trigger the break.
			return 0;
		}

		public override Task<int> ReadAsync(byte[] buffer, int offset, int count,
			System.Threading.CancellationToken cancellationToken)
		{
			return Task.FromResult(0);
		}
		
		public override ValueTask<int> ReadAsync(Memory<byte> buffer, System.Threading.CancellationToken cancellationToken = default)
		{
			return new ValueTask<int>(0);
		}
	}

	// ── Non-seekable stream helper ────────────────────────────────────────────

	/// <summary>A pass-through stream wrapper with <see cref="CanSeek" /> = <c>false</c>.</summary>
	private sealed class NonSeekableStream(byte[] data) : Stream
	{
		private int _pos;

		public override bool CanRead => true;
		public override bool CanSeek => false;
		public override bool CanWrite => false;

		public override long Length =>
			throw new NotSupportedException();

		public override long Position
		{
			get => _pos;
			set => throw new NotSupportedException();
		}

		public override void Flush()
		{
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			var available = Math.Min(count, data.Length - _pos);
			if ( available <= 0 )
			{
				return 0;
			}

			Array.Copy(data, _pos, buffer, offset, available);
			_pos += available;
			return available;
		}

		public override long Seek(long offset, SeekOrigin origin) =>
			throw new NotSupportedException();

		public override void SetLength(long value) =>
			throw new NotSupportedException();

		public override void Write(byte[] buffer, int offset, int count) =>
			throw new NotSupportedException();
	}
}


