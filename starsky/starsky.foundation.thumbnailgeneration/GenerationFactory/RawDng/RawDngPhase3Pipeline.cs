using System;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.RawDng;

internal enum RawDngPipelineStep
{
	ReadTiff,
	DumpRawGrayscaleImage,
	Normalize,
	BilinearDemosaic,
	WhiteBalance,
	ColorMatrix,
	ToneCurve
}

internal sealed class RawDngPipelineState
{
	public required DngRawImage Raw { get; init; }
	public float[,]? NormalizedBayer { get; init; }
	public float[,,]? LinearRgb { get; init; }
	public float[,,]? DisplayRgb { get; init; }
	public float[]? WhiteBalanceGains { get; init; }
	public float[,]? CameraToSrgbMatrix { get; init; }
}

internal static class RawDngPhase3Pipeline
{
	internal static RawDngPipelineState Run(DngRawImage raw)
	{
		return Run(raw, null, null);
	}

	internal static RawDngPipelineState Run(DngRawImage raw,
		Action<RawDngPipelineStep>? onStep,
		Action<byte[,]>? onRawDebug)
	{
		onStep?.Invoke(RawDngPipelineStep.DumpRawGrayscaleImage);
		onRawDebug?.Invoke(RawDebugView.CreateRawGrayscale(raw));

		var initial = new RawDngPipelineState { Raw = raw };

		var pipeline = new Pipeline<RawDngPipelineState>()
			.Add(state => ExecuteStep(state, Normalize, onStep, RawDngPipelineStep.Normalize))
			.Add(state =>
				ExecuteStep(state, Demosaic, onStep, RawDngPipelineStep.BilinearDemosaic))
			.Add(state =>
				ExecuteStep(state, ApplyWhiteBalance, onStep, RawDngPipelineStep.WhiteBalance))
			.Add(state =>
				ExecuteStep(state, ApplyColorMatrix, onStep, RawDngPipelineStep.ColorMatrix))
			.Add(state =>
				ExecuteStep(state, ApplyToneMapping, onStep, RawDngPipelineStep.ToneCurve));

		return pipeline.Run(initial);
	}

	private static RawDngPipelineState ExecuteStep(RawDngPipelineState state,
		Func<RawDngPipelineState, RawDngPipelineState> step,
		Action<RawDngPipelineStep>? onStep,
		RawDngPipelineStep stage)
	{
		onStep?.Invoke(stage);
		return step(state);
	}

	private static RawDngPipelineState Normalize(RawDngPipelineState state)
	{
		var normalized = RawNormalization.NormalizeBayerToLinear(state.Raw.Bayer,
			state.Raw.BlackLevel, state.Raw.WhiteLevel);
		return new RawDngPipelineState
		{
			Raw = state.Raw,
			NormalizedBayer = normalized,
			LinearRgb = state.LinearRgb,
			DisplayRgb = state.DisplayRgb,
			WhiteBalanceGains = state.WhiteBalanceGains,
			CameraToSrgbMatrix = state.CameraToSrgbMatrix
		};
	}

	private static RawDngPipelineState Demosaic(RawDngPipelineState state)
	{
		if ( state.NormalizedBayer == null )
		{
			return state;
		}

		var rgb = BilinearDemosaic.Demosaic(state.NormalizedBayer, state.Raw.CfaPattern);
		return new RawDngPipelineState
		{
			Raw = state.Raw,
			NormalizedBayer = state.NormalizedBayer,
			LinearRgb = rgb,
			DisplayRgb = state.DisplayRgb,
			WhiteBalanceGains = state.WhiteBalanceGains,
			CameraToSrgbMatrix = state.CameraToSrgbMatrix
		};
	}

	private static RawDngPipelineState ApplyWhiteBalance(RawDngPipelineState state)
	{
		if ( state.LinearRgb == null )
		{
			return state;
		}

		var gains = WhiteBalance.GainsFromAsShotNeutral(state.Raw.AsShotNeutral);
		WhiteBalance.ApplyInPlace(state.LinearRgb, gains);

		return new RawDngPipelineState
		{
			Raw = state.Raw,
			NormalizedBayer = state.NormalizedBayer,
			LinearRgb = state.LinearRgb,
			DisplayRgb = state.DisplayRgb,
			WhiteBalanceGains = gains,
			CameraToSrgbMatrix = state.CameraToSrgbMatrix
		};
	}

	private static RawDngPipelineState ApplyColorMatrix(RawDngPipelineState state)
	{
		if ( state.LinearRgb == null )
		{
			return state;
		}

		var cameraToSrgb = ColorMatrixTransform.BuildCameraToSrgb(state.Raw.ColorMatrix1);
		ColorMatrixTransform.ApplyInPlace(state.LinearRgb, cameraToSrgb);

		return new RawDngPipelineState
		{
			Raw = state.Raw,
			NormalizedBayer = state.NormalizedBayer,
			LinearRgb = state.LinearRgb,
			DisplayRgb = state.DisplayRgb,
			WhiteBalanceGains = state.WhiteBalanceGains,
			CameraToSrgbMatrix = cameraToSrgb
		};
	}

	private static RawDngPipelineState ApplyToneMapping(RawDngPipelineState state)
	{
		if ( state.LinearRgb == null )
		{
			return state;
		}

		var display = CopyRgb(state.LinearRgb);
		ToneMapping.ApplyInPlace(display, 2.2f, ToneCurve.Hable);

		return new RawDngPipelineState
		{
			Raw = state.Raw,
			NormalizedBayer = state.NormalizedBayer,
			LinearRgb = state.LinearRgb,
			DisplayRgb = display,
			WhiteBalanceGains = state.WhiteBalanceGains,
			CameraToSrgbMatrix = state.CameraToSrgbMatrix
		};
	}

	private static float[,,] CopyRgb(float[,,] source)
	{
		var h = source.GetLength(0);
		var w = source.GetLength(1);
		var copy = new float[h, w, 3];
		for ( var y = 0; y < h; y++ )
		{
			for ( var x = 0; x < w; x++ )
			{
				copy[y, x, 0] = source[y, x, 0];
				copy[y, x, 1] = source[y, x, 1];
				copy[y, x, 2] = source[y, x, 2];
			}
		}

		return copy;
	}
}




