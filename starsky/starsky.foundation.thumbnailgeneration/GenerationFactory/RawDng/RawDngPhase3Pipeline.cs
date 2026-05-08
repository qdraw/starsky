using System;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.RawDng;

[Obsolete("Use RawDngPipelineExecutor instead")]
internal static class RawDngPhase3Pipeline
{
	internal static RawDngPipelineState Run(DngRawImage raw)
	{
		return RawDngPipelineExecutor.Run(raw);
	}

	internal static RawDngPipelineState Run(DngRawImage raw,
		Action<RawDngPipelineStep>? onStep)
	{
		return RawDngPipelineExecutor.Run(raw, onStep);
	}
}

