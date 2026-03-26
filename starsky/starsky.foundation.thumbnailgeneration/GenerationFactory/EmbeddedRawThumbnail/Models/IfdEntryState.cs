namespace starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.Models;

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
