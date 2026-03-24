using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
			CancellationToken cancellationToken)
		{
			return Task.FromResult(0);
		}

		public override ValueTask<int> ReadAsync(Memory<byte> buffer,
			CancellationToken cancellationToken = default)
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
		var container = Array.Empty<byte>();
		for ( var i = 0; i < 10; i++ )
		{
			container = [.. container, .. jpeg];
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
			CancellationToken cancellationToken = default)
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
		private int _seekCount;
		public bool AllowFirstSeek { get; set; }

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

	/// <summary>
	///     Additional coverage tests for edge cases and internal helper methods.
	/// </summary>
	[TestClass]
	public class ContainerJpegScannerCoverageTests
	{
		[TestMethod]
		public void IsJpegStreamEndMarker_WithEoiMarker_ReturnsTrue()
		{
			// EOI marker is 0xD9
			var result = ContainerJpegScanner.IsJpegStreamEndMarker(0xD9);
			Assert.IsTrue(result);
		}

		[TestMethod]
		public void IsJpegStreamEndMarker_WithSosMarker_ReturnsTrue()
		{
			// SOS marker is 0xDA
			var result = ContainerJpegScanner.IsJpegStreamEndMarker(0xDA);
			Assert.IsTrue(result);
		}

		[TestMethod]
		public void IsJpegStreamEndMarker_WithOtherMarker_ReturnsFalse()
		{
			// APP0 marker is 0xE0
			var result = ContainerJpegScanner.IsJpegStreamEndMarker(0xE0);
			Assert.IsFalse(result);
		}

		[TestMethod]
		public void IsStandaloneMarker_WithRst0Marker_ReturnsTrue()
		{
			// RST0 is 0xD0
			var result = ContainerJpegScanner.IsStandaloneMarker(0xD0);
			Assert.IsTrue(result);
		}

		[TestMethod]
		public void IsStandaloneMarker_WithRst7Marker_ReturnsTrue()
		{
			// RST7 is 0xD7
			var result = ContainerJpegScanner.IsStandaloneMarker(0xD7);
			Assert.IsTrue(result);
		}

		[TestMethod]
		public void IsStandaloneMarker_WithTemMarker_ReturnsTrue()
		{
			// TEM is 0x01
			var result = ContainerJpegScanner.IsStandaloneMarker(0x01);
			Assert.IsTrue(result);
		}

		[TestMethod]
		public void IsStandaloneMarker_WithNonStandaloneMarker_ReturnsFalse()
		{
			// APP0 is 0xE0, not standalone
			var result = ContainerJpegScanner.IsStandaloneMarker(0xE0);
			Assert.IsFalse(result);
		}

		[TestMethod]
		public void TryReadSegmentPayloadLength_WithValidLength_ReturnsTrue()
		{
			// Write a 2-byte length field: 0x00 0x10 = 16 bytes total, so payload = 14
			using var ms = new MemoryStream([0x00, 0x10]);

			var result =
				ContainerJpegScanner.TryReadSegmentPayloadLength(ms, out var payloadLength);

			Assert.IsTrue(result);
			Assert.AreEqual(14, payloadLength);
		}

		[TestMethod]
		public void TryReadSegmentPayloadLength_WithLengthTooSmall_ReturnsFalse()
		{
			// Write a 2-byte length field: 0x00 0x01 = 1 byte total, which is invalid (< 2)
			using var ms = new MemoryStream([0x00, 0x01]);

			var result = ContainerJpegScanner.TryReadSegmentPayloadLength(ms, out _);

			Assert.IsFalse(result);
		}

		[TestMethod]
		public void TryReadSegmentPayloadLength_WithInsufficientBytes_ReturnsFalse()
		{
			// Only 1 byte available
			using var ms = new MemoryStream([0x00]);

			var result = ContainerJpegScanner.TryReadSegmentPayloadLength(ms, out _);

			Assert.IsFalse(result);
		}

		[TestMethod]
		public void AdvanceSegmentAndCheckIptc_WithNonApp13Marker_ReturnsFalse()
		{
			// APP0 marker (0xE0) is not APP13 (0xED)
			using var ms = new MemoryStream(new byte[100]);

			var result = ContainerJpegScanner.AdvanceSegmentAndCheckIptc(ms, 0xE0, 50);

			Assert.IsFalse(result);
			// Stream should have advanced
			Assert.AreEqual(50, ms.Position);
		}

		[TestMethod]
		public void AdvanceSegmentAndCheckIptc_WithApp13Marker_CallsIsIptcApp13Payload()
		{
			// APP13 marker (0xED) should delegate to IsIptcApp13Payload
			using var ms =
				new MemoryStream(Encoding.ASCII.GetBytes("Photoshop 3.0" + new string(' ', 100)));

			var result = ContainerJpegScanner.AdvanceSegmentAndCheckIptc(ms, 0xED, 114);

			Assert.IsTrue(result);
		}

		[TestMethod]
		public void SelectBest_WithEmptyList_ReturnsNull()
		{
			var candidates = new List<ContainerJpegScanner.PreviewCandidate>();

			var result = ContainerJpegScanner.SelectBest(candidates);

			Assert.IsNull(result);
		}

		[TestMethod]
		public void SelectBest_WithSingleCandidate_ReturnsIt()
		{
			var candidates = new List<ContainerJpegScanner.PreviewCandidate>
			{
				new(100, 5000, false)
			};

			var result = ContainerJpegScanner.SelectBest(candidates);

			Assert.IsNotNull(result);
			Assert.AreEqual(100u, result.Offset);
		}

		[TestMethod]
		public void SelectBest_PrefersIptcCandidate_OverNonIptc()
		{
			var candidates = new List<ContainerJpegScanner.PreviewCandidate>
			{
				new(100, 5000, false), new(200, 4000, true)
			};

			var result = ContainerJpegScanner.SelectBest(candidates);

			Assert.IsNotNull(result);
			Assert.AreEqual(200u, result.Offset, "Should prefer IPTC candidate");
		}

		[TestMethod]
		public void SelectBest_PrefersLongerLengthWhenSameIptcStatus()
		{
			var candidates = new List<ContainerJpegScanner.PreviewCandidate>
			{
				new(100, 4000, true), new(200, 5000, true)
			};

			var result = ContainerJpegScanner.SelectBest(candidates);

			Assert.IsNotNull(result);
			Assert.AreEqual(200u, result.Offset, "Should prefer longer length");
		}

		[TestMethod]
		public async Task CopyRangeToOutput_WithValidRange_CopiesBytesSuccessfully()
		{
			var inputData = new byte[300];
			for ( var i = 0; i < inputData.Length; i++ )
			{
				inputData[i] = ( byte ) ( i % 256 );
			}

			using var input = new MemoryStream(inputData);
			using var output = new MemoryStream();

			var result = await ContainerJpegScanner.CopyRangeToOutput(input, output, 50, 100);

			Assert.IsTrue(result);
			Assert.AreEqual(100, output.Length);
		}

		[TestMethod]
		public async Task CopyRangeToOutput_WithRangeExceedingStreamLength_ReturnsFalse()
		{
			using var input = new MemoryStream(new byte[100]);
			using var output = new MemoryStream();

			var result = await ContainerJpegScanner.CopyRangeToOutput(input, output, 50, 100);

			Assert.IsFalse(result);
		}

		[TestMethod]
		public async Task CopyRangeToOutput_WithUnseekableStream_ReturnsFalse()
		{
			var inner = new MemoryStream(new byte[300]);
			var unseekable = new UnseekableStream(inner);
			using var output = new MemoryStream();

			var result = await ContainerJpegScanner.CopyRangeToOutput(unseekable, output, 50, 100);

			Assert.IsFalse(result);
		}

		[TestMethod]
		public void TrySkipToMarker_WithValidMarkerAtStart_ReturnsTrue()
		{
			// 0xFF followed by non-0xFF byte (0xE0)
			using var ms = new MemoryStream([0xFF, 0xE0, 0x00, 0x00]);

			var result = ContainerJpegScanner.TrySkipToMarker(ms, 4, out var marker);

			Assert.IsTrue(result);
			Assert.AreEqual(0xE0, marker);
		}

		[TestMethod]
		public void TrySkipToMarker_WithPaddingBytes_SkipsAndFindsMarker()
		{
			// Some padding (0xFF 0xFF) then marker 0xE0
			using var ms = new MemoryStream([0x00, 0xFF, 0xFF, 0xE0, 0x00]);

			var result = ContainerJpegScanner.TrySkipToMarker(ms, 5, out var marker);

			Assert.IsTrue(result);
			Assert.AreEqual(0xE0, marker);
		}

		[TestMethod]
		public void TrySkipToMarker_WithNoValidMarker_ReturnsFalse()
		{
			// No 0xFF byte at all
			using var ms = new MemoryStream([0x00, 0x01, 0x02]);

			var result = ContainerJpegScanner.TrySkipToMarker(ms, 3, out _);

			Assert.IsFalse(result);
		}

		[TestMethod]
		public void TrySkipToMarker_AtEndOfRange_ReturnsFalse()
		{
			using var ms = new MemoryStream([0x00, 0x01]);

			var result = ContainerJpegScanner.TrySkipToMarker(ms, 2, out _);

			Assert.IsFalse(result);
		}

		[TestMethod]
		public void IsIptcApp13Payload_WithPhotoshopSignature_ReturnsTrue()
		{
			var payload = Encoding.ASCII.GetBytes("Photoshop 3.0");
			using var ms = new MemoryStream(payload);

			var result = ContainerJpegScanner.IsIptcApp13Payload(ms, payload.Length);

			Assert.IsTrue(result);
		}

		[TestMethod]
		public void IsIptcApp13Payload_With8BimSignature_ReturnsTrue()
		{
			var payload = Encoding.ASCII.GetBytes("8BIM");
			using var ms = new MemoryStream(payload);

			var result = ContainerJpegScanner.IsIptcApp13Payload(ms, payload.Length);

			Assert.IsTrue(result);
		}

		[TestMethod]
		public void IsIptcApp13Payload_WithoutIptcSignature_ReturnsFalse()
		{
			var payload = Encoding.ASCII.GetBytes("Some random data here");
			using var ms = new MemoryStream(payload);

			var result = ContainerJpegScanner.IsIptcApp13Payload(ms, payload.Length);

			Assert.IsFalse(result);
		}

		[TestMethod]
		public void IsIptcApp13Payload_WithInsufficientBytes_ReturnsFalse()
		{
			using var ms = new MemoryStream([]);

			var result = ContainerJpegScanner.IsIptcApp13Payload(ms, 0);

			Assert.IsFalse(result);
		}

		[TestMethod]
		public void ScanCandidates_WithValidJpegData_FindsCandidates()
		{
			// Create stream with JPEG SOI marker
			var data = new byte[10000];
			data[0] = 0xFF;
			data[1] = 0xD8;
			data[2] = 0xFF;
			// Fill with some data to reach minimum size
			for ( var i = 3; i < data.Length - 2; i++ )
			{
				data[i] = 0x00;
			}

			data[^2] = 0xFF;
			data[^1] = 0xD9; // EOI

			using var ms = new MemoryStream(data);

			var candidates = ContainerJpegScanner.ScanCandidates(ms);

			Assert.IsNotEmpty(candidates);
		}

		private sealed class UnseekableStream : Stream
		{
			private readonly Stream _inner;

			public UnseekableStream(Stream inner)
			{
				_inner = inner;
			}

			public override bool CanRead => _inner.CanRead;
			public override bool CanSeek => false;
			public override bool CanWrite => _inner.CanWrite;
			public override long Length => _inner.Length;

			public override long Position
			{
				get => _inner.Position;
				set => throw new NotSupportedException();
			}

			public override void Flush()
			{
				_inner.Flush();
			}

			public override int Read(byte[] buffer, int offset, int count)
			{
				return _inner.Read(buffer, offset, count);
			}

			public override long Seek(long offset, SeekOrigin origin)
			{
				throw new NotSupportedException();
			}

			public override void SetLength(long value)
			{
				_inner.SetLength(value);
			}

			public override void Write(byte[] buffer, int offset, int count)
			{
				_inner.Write(buffer, offset, count);
			}

			protected override void Dispose(bool disposing)
			{
				if ( disposing )
				{
					_inner.Dispose();
				}

				base.Dispose(disposing);
			}
		}
	}
}
