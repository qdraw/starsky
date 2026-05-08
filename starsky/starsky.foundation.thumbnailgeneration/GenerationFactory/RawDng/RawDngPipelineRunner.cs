using System;
using System.IO;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.RawDng;

internal static class RawDngPipelineRunner
{
	internal static bool TryRun(Stream input, out RawDngPipelineState? state, out string error,
		Action<RawDngPipelineStep>? onStep = null)
	{
		state = null;
		error = string.Empty;

		onStep?.Invoke(RawDngPipelineStep.ReadTiff);
		if ( !DngSubsetReader.TryLoad(input, out var raw, out error) || raw == null )
		{
			return false;
		}

		state = RawDngPhase3Pipeline.Run(raw,
			onStep != null ? step => onStep(step) : null);
		return true;
	}

	internal static bool TryRunToJpeg(Stream input, Stream output, out string error,
		Action<RawDngPipelineStep>? onStep = null)
	{
		if ( TryRun(input, out var state, out error, onStep) &&
		     state?.DisplayRgb != null )
		{
			return RawDngJpegExporter.TryWriteDisplayRgbAsJpeg(state.DisplayRgb, output, out error);
		}

		error = string.IsNullOrWhiteSpace(error)
			? "No display RGB output available from DNG RAW decode"
			: error;
		return false;
	}
}
