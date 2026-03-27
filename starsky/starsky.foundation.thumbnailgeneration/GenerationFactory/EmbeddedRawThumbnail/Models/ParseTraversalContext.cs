using System.Collections.Generic;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.Models;

internal sealed record ParseTraversalContext
{
	public required List<PreviewCandidate> Previews { get; init; }
	public required HashSet<uint> Visited { get; init; }
	public required string ReferenceInfo { get; init; }
	public required RawFlavor RawFlavor { get; init; }
}
