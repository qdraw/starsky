namespace starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.Models;

/// <summary>
/// Holds parsed state information for a single Image File Directory (IFD) entry
/// when extracting embedded thumbnails (and maker notes/strips) from RAW files.
///
/// This is a lightweight DTO populated while scanning IFD entries to record
/// offsets and lengths of embedded JPEG data (or strip-based preview), the
/// compression/type info and the image dimensions. Consumers use this to
/// locate and extract thumbnail payloads without reparsing the IFD again.
/// </summary>
internal sealed class IfdEntryState
{
	/// <summary>
	/// Offset (in bytes) from the start of the file to the embedded JPEG data.
	/// Only valid when <see cref="HasJpeg"/> is true. The offset is typically
	/// read from TIFF/IFD tag values and represents where the JPEG payload starts.
	/// </summary>
	public uint JpegOffset { get; set; }

	/// <summary>
	/// Length (in bytes) of the embedded JPEG payload. Together with
	/// <see cref="JpegOffset"/>, it defines the exact byte range to extract.
	/// </summary>
	public uint JpegLength { get; set; }

	/// <summary>
	/// The compression tag value read from the IFD for this entry. This helps
	/// determine how the preview data is stored (e.g. JPEG v.s. uncompressed).
	/// </summary>
	public uint IfdCompression { get; set; }

	/// <summary>
	/// Width (in pixels) of the preview/thumbnail referenced by this IFD entry.
	/// Useful for quick validation and to select the best candidate preview.
	/// </summary>
	public uint IfdWidth { get; set; }

	/// <summary>
	/// Height (in pixels) of the preview/thumbnail referenced by this IFD entry.
	/// </summary>
	public uint IfdHeight { get; set; }

	/// <summary>
	/// Offset to the MakerNote block (if present). MakerNote often contains
	/// vendor-specific metadata; some vendors embed a preview inside the maker
	/// note region — store offset/length to allow extraction.
	/// </summary>
	public uint MakerNoteOffset { get; set; }

	/// <summary>
	/// Length (in bytes) of the maker note block starting at <see cref="MakerNoteOffset"/>.
	/// </summary>
	public uint MakerNoteLength { get; set; }

	/// <summary>
	/// True when a JPEG preview is present for this IFD entry and JpegOffset/JpegLength
	/// contain a valid byte range to extract.
	/// </summary>
	public bool HasJpeg { get; set; }

	/// <summary>
	/// True when a maker note block is present (and MakerNoteOffset/MakerNoteLength are set).
	/// </summary>
	public bool HasMakerNote { get; set; }

	// Strip-based preview: Canon CR2 IFD0 stores the large JPEG at 0x0111/0x0117 (count=1)
	/// <summary>
	/// Offset to a strip-based preview (when previews are stored as TIFF strips
	/// rather than a contiguous JPEG blob). Used by some formats (Canon CR2)
	/// where preview data is referenced via strip/strip byte counts.
	/// </summary>
	public uint StripOffset { get; set; }

	/// <summary>
	/// Length (in bytes) of the strip-based preview data referenced by <see cref="StripOffset"/>.
	/// </summary>
	public uint StripLength { get; set; }

	/// <summary>
	/// True when a strip-based preview exists for this IFD entry and StripOffset/StripLength
	/// define the range.
	/// </summary>
	public bool HasStrip { get; set; }
}
