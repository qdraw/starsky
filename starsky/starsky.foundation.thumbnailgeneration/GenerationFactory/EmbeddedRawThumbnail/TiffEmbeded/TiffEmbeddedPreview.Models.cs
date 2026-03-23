using System.Collections.Generic;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.TiffEmbeded;

public partial class TiffEmbeddedPreviewExtractor
{
	internal sealed record ParseTraversalContext
	{
		public required List<PreviewCandidate> Previews { get; init; }
		public required HashSet<uint> Visited { get; init; }
		public required string ReferenceInfo { get; init; }
		public required RawFlavor RawFlavor { get; init; }
	}

	internal sealed class IfdEntryState
	{
		public uint JpegOffset { get; set; }
		public uint JpegLength { get; set; }
		public uint IfdCompression { get; set; }
		public uint IfdWidth { get; set; }
		public uint IfdHeight { get; set; }
		public uint MakerNoteOffset { get; set; }
		public uint MakerNoteLength { get; set; }
		public bool HasJpeg { get; set; }

		public bool HasMakerNote { get; set; }

		// Strip-based preview: Canon CR2 IFD0 stores the large JPEG at 0x0111/0x0117 (count=1)
		public uint StripOffset { get; set; }
		public uint StripLength { get; set; }
		public bool HasStrip { get; set; }
	}

	internal sealed record PreviewCandidate
	{
		public uint Offset { get; set; }
		public uint Length { get; set; }
		public uint Width { get; set; }
		public uint Height { get; set; }
	}

	internal readonly record struct IfdTagPairQuery(
		ushort PrimaryOffsetTag,
		ushort PrimaryLengthTag,
		ushort AltTag,
		bool LittleEndian);
}
