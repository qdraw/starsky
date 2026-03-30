namespace starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.Models;

internal sealed record PreviewCandidate
{
	public uint Offset { get; set; }
	public uint Length { get; set; }
	public uint Width { get; set; }
	public uint Height { get; set; }
}
