using System;
using System.IO;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.RawDng;

internal enum RawDngPipelineStep
{
	ReadTiff,
	DumpRawGrayscaleImage,
	Normalize,
	BilinearDemosaic,
	WhiteBalance,
	ColorMatrix,
	ExposureCompensation,
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

internal static class RawDngPipelineExecutor
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

		state = Run(raw, onStep);
		return true;
	}

	internal static bool TryRunToJpeg(Stream input, Stream output, out string error,
		Action<RawDngPipelineStep>? onStep = null)
	{
		if ( TryRun(input, out var state, out error, onStep) && state?.DisplayRgb != null )
		{
			return RawDngJpegExporter.TryWriteDisplayRgbAsJpeg(state.DisplayRgb, output, out error);
		}

		error = string.IsNullOrWhiteSpace(error)
			? "No display RGB output available from DNG RAW decode"
			: error;
		return false;
	}

	internal static RawDngPipelineState Run(DngRawImage raw,
		Action<RawDngPipelineStep>? onStep = null)
	{
		onStep?.Invoke(RawDngPipelineStep.DumpRawGrayscaleImage);

		var initial = new RawDngPipelineState { Raw = raw };
		var pipeline = new Pipeline<RawDngPipelineState>()
			.Add(state => ExecuteStep(state, Normalize, onStep, RawDngPipelineStep.Normalize))
			.Add(state => ExecuteStep(state, Demosaic, onStep, RawDngPipelineStep.BilinearDemosaic))
			.Add(state => ExecuteStep(state, ApplyWhiteBalance, onStep, RawDngPipelineStep.WhiteBalance))
			.Add(state => ExecuteStep(state, ApplyColorMatrix, onStep, RawDngPipelineStep.ColorMatrix))
			.Add(state =>
				ExecuteStep(state, ApplyExposureCompensation, onStep,
					RawDngPipelineStep.ExposureCompensation))
			.Add(state => ExecuteStep(state, ApplyToneMapping, onStep, RawDngPipelineStep.ToneCurve));

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
			state.Raw.BlackLevel, state.Raw.WhiteLevel, state.Raw.CfaPattern);
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

		var cameraToSrgb = ColorMatrixTransform.BuildCameraToSrgb(state.Raw.ColorMatrix1,
			state.Raw.ForwardMatrix1, state.Raw.CalibrationIlluminant1,
			state.Raw.CameraCalibration1,
			state.Raw.ColorMatrix2,
			state.Raw.ForwardMatrix2,
			state.Raw.CalibrationIlluminant2,
			state.Raw.CameraCalibration2,
			state.Raw.AsShotNeutral);
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

	private static RawDngPipelineState ApplyExposureCompensation(RawDngPipelineState state)
	{
		if ( state.LinearRgb == null )
		{
			return state;
		}

		// Auto-brighten from sampled max luminance to keep output usable across camera profiles.
		var gain = ComputeAutoExposureGain(state.LinearRgb);
		var h = state.LinearRgb.GetLength(0);
		var w = state.LinearRgb.GetLength(1);
		for ( var y = 0; y < h; y++ )
		{
			for ( var x = 0; x < w; x++ )
			{
				state.LinearRgb[y, x, 0] *= gain;
				state.LinearRgb[y, x, 1] *= gain;
				state.LinearRgb[y, x, 2] *= gain;
			}
		}

		return state;
	}

	private static float ComputeAutoExposureGain(float[,,] linearRgb)
	{
		var h = linearRgb.GetLength(0);
		var w = linearRgb.GetLength(1);

		var y1 = h / 10;
		var y2 = h * 9 / 10;
		var x1 = w / 10;
		var x2 = w * 9 / 10;

		float maxLum = 1e-6f;
		var sampleCount = 0;
		for ( var y = y1; y < y2; y += 4 )
		{
			for ( var x = x1; x < x2; x += 4 )
			{
				var r = linearRgb[y, x, 0];
				var g = linearRgb[y, x, 1];
				var b = linearRgb[y, x, 2];
				var lum = 0.2126f * r + 0.7152f * g + 0.0722f * b;
				if ( lum > maxLum )
				{
					maxLum = lum;
				}

				sampleCount++;
			}
		}

		if ( sampleCount == 0 )
		{
			return 1f;
		}

		const float target = 0.9f;
		var gain = target / maxLum;
		return Math.Clamp(gain, 0.5f, 3.0f);
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

