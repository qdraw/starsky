using System;
using System.IO;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.RawDng;

internal static class RawDngPipelineRunner
{
	internal static bool TryRun(Stream input, out RawDngPipelineState? state, out string error,
		Action<RawDngPipelineStep>? onStep = null, Action<byte[,]>? onRawDebug = null)
	{
		state = null;
		error = string.Empty;

		onStep?.Invoke(RawDngPipelineStep.ReadTiff);
		if ( !DngSubsetReader.TryLoad(input, out var raw, out error) || raw == null )
		{
			return false;
		}

		state = RawDngPhase3Pipeline.Run(raw,
			onStep != null ? step => onStep(step) : null,
			onRawDebug != null ? debug => onRawDebug(debug) : null);
		return true;
	}

	internal static bool TryRunToJpeg(Stream input, Stream output, out string error,
		Action<RawDngPipelineStep>? onStep = null, Action<byte[,]>? onRawDebug = null)
	{
		error = string.Empty;
		if ( !TryRun(input, out var state, out error, onStep, onRawDebug) ||
		     state?.DisplayRgb == null )
		{
			if ( string.IsNullOrEmpty(error) )
			{
				error = "No display RGB output available";
			}

			return false;
		}

		return RawDngJpegExporter.TryWriteDisplayRgbAsJpeg(state.DisplayRgb, output, out error);
	}
}


