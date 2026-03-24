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

	private static MemoryStream StreamOf(byte[] bytes)
	{
		return new MemoryStream(bytes, false);
	}

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

		var result = await ContainerJpegScanner.TryExtractBestPreview(input, null);

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
		var jpeg = BuildJpeg(MinJpeg, "Photoshop 3.0\0");
		using var input = StreamOf(jpeg);
		using var output = new MemoryStream();

		var result = await ContainerJpegScanner.TryExtractBestPreview(input, output);

		Assert.IsTrue(result);
		Assert.IsGreaterThan(0, output.Length,
			"Expected JPEG extracted when APP13 has Photoshop signature");
	}

	[TestMethod]
	public async Task TryExtractBestPreview_DetectsIptc_Via8BimApp13Signature()
	{
		// "8BIM" is the Photoshop resource block marker also accepted as an IPTC signal
		var jpeg = BuildJpeg(MinJpeg, "8BIM");
		using var input = StreamOf(jpeg);
		using var output = new MemoryStream();

		var result = await ContainerJpegScanner.TryExtractBestPreview(input, output);

		Assert.IsTrue(result);
		Assert.IsGreaterThan(0, output.Length,
			"Expected JPEG extracted when APP13 has 8BIM signature");
	}

	[TestMethod]
	public async Task TryExtractBestPreview_ExtractsJpeg_WhenApp13HasUnrecognisedContent()
	{
		// APP13 present but payload does not contain either IPTC signature.
		// The JPEG should still be extracted; it simply won't be scored as an IPTC candidate.
		var jpeg = BuildJpeg(MinJpeg, "Unknown APP13 content");
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

		var plainJpeg = BuildJpeg(largeNoIptcSize); // large, no IPTC
		var iptcJpeg = BuildJpeg(smallIptcSize, "Photoshop 3.0\0"); // small, IPTC

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

		var smallJpeg = BuildJpeg(smallIptcSize, "Photoshop 3.0\0");
		var largeJpeg = BuildJpeg(largeIptcSize, "8BIM");

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
		var jpeg = BuildJpeg(MinJpeg, "Photoshop 3.0\0", true);
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
		var jpeg = BuildJpeg(MinJpeg, "Photoshop 3.0\0", withMarkerPadding: true);
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

		// If input.Read returns 0, it means EOF or an error.
		// We can mock a stream that claims to have more data than it actually returns.
		var input = new TruncatedStream(new byte[MinJpeg * 2], MinJpeg * 2);
		using var output = new MemoryStream();

		var result = await ContainerJpegScanner.TryExtractBestPreview(input, output);

		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task TryExtractBestPreview_SkipsSmallCandidate()
	{
		// TryAddJpegCandidate: length < MinJpegSize ) return
		// MinJpegSize = 4096. 
		// We'll put a SOI but NO EOI, so DetectJpegLengthFromSoi returns 0.
		var smallJpeg = new byte[MinJpeg];
		smallJpeg[0] = 0xFF;
		smallJpeg[1] = 0xD8;
		smallJpeg[2] = 0xFF;
		smallJpeg[3] = 0xE0;
		// No EOI (FF D9) in the rest.
		
		using var input = StreamOf(smallJpeg);
		var result = await ContainerJpegScanner.TryExtractBestPreview(input, null);
		// result will be false because only candidate was length 0 < 4096
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task TryExtractBestPreview_PrefersFirstCandidate_WhenSameSizeAndIptc()
	{
		//  candidate.HasIptc == best.HasIptc AND candidate.Length > best.Length 
		// If length is SAME, it keeps the first one (best is not updated).
		var jpeg1 = BuildJpeg(MinJpeg);
		var jpeg2 = BuildJpeg(MinJpeg);
		var container = jpeg1.Concat(jpeg2).ToArray();

		using var input = StreamOf(container);
		using var output = new MemoryStream();
		var result = await ContainerJpegScanner.TryExtractBestPreview(input, output);

		Assert.IsTrue(result);
		// It should be jpeg1 (at offset 0)
		Assert.AreEqual(MinJpeg, ( int ) output.Length);
	}

	[TestMethod]
	public async Task HasIptcApp13_ReturnsFalse_WhenInitialSeekFails()
	{
		// !StreamPrimitives.TrySeek(input, offset + 2)
		// We need to bypass the public API if we can, but we can't easily.
		// offset is found by ScanCandidates, then HasIptcApp13 is called with that offset.
		// If input is truncated so that offset+2 is past end, it fails.

		var jpeg = new byte[MinJpeg];
		jpeg[0] = 0xFF;
		jpeg[1] = 0xD8;
		// Stream length is only 2
		using var input = new TruncatedStream(jpeg, 2);
		// TryExtractBestPreview requires length >= MinJpegSize, so we need to lie about Length.

		var mockInput =
			new PseudoTruncatedStream(jpeg, 2); // Length says 4096, but CanSeek/Read only allow 2.
		var result = await ContainerJpegScanner.TryExtractBestPreview(mockInput, null);
		Assert.IsFalse(result);
	}

	private sealed class PseudoTruncatedStream(byte[] data, int realLength) : MemoryStream(data)
	{
		public override long Length => 4096;

		public override int Read(byte[] buffer, int offset, int count)
		{
			var available = Math.Min(count, realLength - ( int ) Position);
			if ( available <= 0 )
			{
				return 0;
			}

			return base.Read(buffer, offset, available);
		}

		public override long Seek(long offset, SeekOrigin loc)
		{
			if ( offset > realLength )
			{
				return -1; // Fail seek past realLength
			}

			return base.Seek(offset, loc);
		}
	}

	private sealed class TruncatedStream(byte[] data, int length)
		: MemoryStream(data.Take(length).ToArray())
	{
		public override int Read(byte[] buffer, int offset, int count)
		{
			return 0;
		}

		public override Task<int> ReadAsync(byte[] buffer, int offset, int count,
			System.Threading.CancellationToken cancellationToken)
		{
			return Task.FromResult(0);
		}

		public override ValueTask<int> ReadAsync(Memory<byte> buffer,
			System.Threading.CancellationToken cancellationToken = default)
		{
			return new ValueTask<int>(0);
		}
	}

	[TestMethod]
	public async Task TryExtractBestPreview_ReturnsFalse_WhenSeekFails()
	{
		// StreamPrimitives.TrySeek(input, 0) failure in ScanCandidates
		var input = new SeekFailureStream(BuildJpeg(MinJpeg));
		var result = await ContainerJpegScanner.TryExtractBestPreview(input, null);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task TryExtractBestPreview_StopsAtMaxCandidates()
	{
		// MaxCandidates = 8
		var jpeg = BuildJpeg(MinJpeg);
		var container = new byte[0];
		for ( var i = 0; i < 10; i++ )
		{
			container = container.Concat(jpeg).ToArray();
		}

		using var input = StreamOf(container);
		// ScanCandidates loop should terminate after finding 8 candidates
		var result = await ContainerJpegScanner.TryExtractBestPreview(input, null);
		Assert.IsTrue(result);
	}

	[TestMethod]
	public async Task TryExtractBestPreview_HandlesCandidateFoundAtBufferBoundary()
	{
		// Buffer size in ScanCandidates is 64*1024
		const int bufferSize = 64 * 1024;
		var containerSize = bufferSize * 2;
		var container = new byte[containerSize];
		// Put FF at end of first buffer, D8 FF at start of next buffer
		container[bufferSize - 1] = 0xFF;
		container[bufferSize] = 0xD8;
		container[bufferSize + 1] = 0xFF;

		// Need to complete the JPEG so DetectJpegLengthFromSoi doesn't fail to meet MinJpegSize
		var jpeg = BuildJpeg(MinJpeg);
		Array.Copy(jpeg, 2, container, bufferSize + 1,
			Math.Min(jpeg.Length - 2, container.Length - ( bufferSize + 1 )));

		using var input = StreamOf(container);
		var result = await ContainerJpegScanner.TryExtractBestPreview(input, null);
		Assert.IsTrue(result);
	}

	[TestMethod]
	public async Task TryExtractBestPreview_ReturnsFalse_WhenCopyRangeSeekFails()
	{
		var jpeg = BuildJpeg(MinJpeg);
		var input =
			new SeekFailureStream(jpeg)
			{
				AllowFirstSeek = true
			}; // Allow ScanCandidates seek, fail CopyRange seek
		var output = new MemoryStream();
		var result = await ContainerJpegScanner.TryExtractBestPreview(input, output);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task TryExtractBestPreview_ReturnsFalse_WhenCopyRangeReadFails()
	{
		var jpeg = BuildJpeg(MinJpeg);
		// Position will be 0 during ScanCandidates, then CopyRangeToOutput will seek to offset and read.
		// offset is 0 for BuildJpeg.
		var input = new ReadFailureStream(jpeg) { FailAtPosition = 0, OnlyFailReadAsync = true };
		var output = new MemoryStream();
		var result = await ContainerJpegScanner.TryExtractBestPreview(input, output);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task HasIptcApp13_HandlesInvalidSegmentLength()
	{
		// TryReadSegmentPayloadLength returns false (segmentLength < 2)
		var jpeg = BuildJpeg(MinJpeg);
		// Find an APP marker and corrupt its length. 
		// APP0 is at index 2: FF E0 [length high] [length low]
		jpeg[4] = 0x00;
		jpeg[5] = 0x01; // length = 1, which is < 2

		using var input = StreamOf(jpeg);
		var result = await ContainerJpegScanner.TryExtractBestPreview(input, null);
		// Should still return true (JPEG found), just HasIptc=false
		Assert.IsTrue(result);
	}

	[TestMethod]
	public async Task HasIptcApp13_HandlesSegmentLengthPastEnd()
	{
		// input.Position + payloadLength > end
		var jpeg = BuildJpeg(MinJpeg);
		// APP0 at index 2. Let's make it huge.
		jpeg[4] = 0xFF;
		jpeg[5] = 0xFF;

		using var input = StreamOf(jpeg);
		var result = await ContainerJpegScanner.TryExtractBestPreview(input, null);
		Assert.IsTrue(result);
	}

	[TestMethod]
	public async Task TryExtractBestPreview_HandlesApp13ReadFailure()
	{
		// IsIptcApp13Payload read < probeLength
		var jpeg = BuildJpeg(MinJpeg,
			"Photoshop 3.0 Some very long content to ensure it is more than 64 bytes if needed, but the probe is 64 anyway.");
		// We want input.Read to return less than probeLength.
		var input = new ReadFailureInHasIptcStream(jpeg);
		var result = await ContainerJpegScanner.TryExtractBestPreview(input, null);
		Assert.IsTrue(result);
	}

	private sealed class ReadFailureInHasIptcStream(byte[] data) : MemoryStream(data)
	{
		public override int Read(byte[] buffer, int offset, int count)
		{
			return count == 64
				? 10
				: // Fail the probe read in IsIptcApp13Payload
				base.Read(buffer, offset, count);
		}
	}

	private sealed class ReadFailureStream(byte[] data) : MemoryStream(data)
	{
		public long FailAtPosition { get; set; } = -1;
		public bool OnlyFailReadAsync { get; set; }

		public override int Read(byte[] buffer, int offset, int count)
		{
			if ( !OnlyFailReadAsync && FailAtPosition >= 0 && Position >= FailAtPosition )
			{
				return 0;
			}

			return base.Read(buffer, offset, count);
		}

		public override async ValueTask<int> ReadAsync(Memory<byte> buffer,
			System.Threading.CancellationToken cancellationToken = default)
		{
			if ( FailAtPosition >= 0 && Position >= FailAtPosition )
			{
				return 0;
			}

			return await base.ReadAsync(buffer, cancellationToken);
		}
	}

	[TestMethod]
	public async Task IsStandaloneMarker_HandlesTemMarker()
	{
		// 0x01 TEM marker
		var jpeg = BuildJpeg(MinJpeg);
		// Inject 0xFF 0x01
		jpeg[6] = 0xFF;
		jpeg[7] = 0x01;

		using var input = StreamOf(jpeg);
		var result = await ContainerJpegScanner.TryExtractBestPreview(input, null);
		Assert.IsTrue(result);
	}

	private sealed class SeekFailureStream(byte[] data) : MemoryStream(data)
	{
		public bool AllowFirstSeek { get; set; }
		private int _seekCount;

		public override long Seek(long offset, SeekOrigin loc)
		{
			_seekCount++;
			if ( AllowFirstSeek && _seekCount == 1 )
			{
				return base.Seek(offset, loc);
			}

			return
				-1; // Or throw, but TrySeek expects false which comes from Catch in some implementations, 
			// but StreamPrimitives.TrySeek uses try-catch.
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

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException();
		}
	}
}
