using System;
using System.IO;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.RawDng;

internal static class RawDngPipelineRunner
{
	internal static bool TryRun(Stream input, out RawDngPipelineState? state, out string error,
		Action<RawDngPipelineStep>? onStep = null)
	{
		return RawDngPipelineExecutor.TryRun(input, out state, out error, onStep);
	}

	internal static bool TryRunToJpeg(Stream input, Stream output, out string error,
		Action<RawDngPipelineStep>? onStep = null)
	{
		return RawDngPipelineExecutor.TryRunToJpeg(input, output, out error, onStep);
	}
}
