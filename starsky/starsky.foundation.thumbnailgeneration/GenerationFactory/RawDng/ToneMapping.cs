using System;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.RawDng;

internal enum ToneCurve
{
	None,
	Hable,
	Aces
}

internal static class ToneMapping
{
	internal static void ApplyInPlace(float[,,] rgb, float gamma, ToneCurve curve)
	{
		var invGamma = gamma > 0f ? 1f / gamma : 1f / 2.2f;
		var h = rgb.GetLength(0);
		var w = rgb.GetLength(1);
		for ( var y = 0; y < h; y++ )
		{
			for ( var x = 0; x < w; x++ )
			{
				for ( var c = 0; c < 3; c++ )
				{
					rgb[y, x, c] = MapValue(rgb[y, x, c], invGamma, curve);
				}
			}
		}
	}

	internal static float MapValue(float linear, float invGamma, ToneCurve curve)
	{
		if ( linear <= 0f )
		{
			return 0f;
		}

		// Step 1: apply tone curve on linear light values
		float mapped;
		switch ( curve )
		{
			case ToneCurve.Hable:
				mapped = HableNormalized(linear);
				break;
			case ToneCurve.Aces:
				mapped = Aces(linear);
				break;
			default:
				mapped = linear;
				break;
		}

		// Step 2: gamma-encode the tone-mapped result
		var value = ( float ) Math.Pow(Math.Max(mapped, 0f), invGamma);
		return Math.Clamp(value, 0f, 1f);
	}

	/// <summary>
	/// Uncharted 2 / Hable filmic curve applied to linear input and normalized
	/// by the curve's white point so that a value of 1 maps to 1.
	/// </summary>
	private static float HableNormalized(float x)
	{
		// White point: scene-linear value that should map to display white.
		// 4.0 works well for typical RAW input after +1.5 EV compensation.
		const float w = 4.0f;
		var tw = HableF(w);
		if ( tw <= 0f )
		{
			return 0f;
		}

		return HableF(x) / tw;
	}

	/// <summary>Raw Hable curve operator (Uncharted 2 filmic).</summary>
	private static float HableF(float x)
	{
		const float a = 0.15f;
		const float b = 0.50f;
		const float c = 0.10f;
		const float d = 0.20f;
		const float e = 0.02f;
		const float f = 0.30f;

		return ( x * ( a * x + c * b ) + d * e ) / ( x * ( a * x + b ) + d * f ) - e / f;
	}

	private static float Aces(float x)
	{
		const float a = 2.51f;
		const float b = 0.03f;
		const float c = 2.43f;
		const float d = 0.59f;
		const float e = 0.14f;
		return ( x * ( a * x + b ) ) / ( x * ( c * x + d ) + e );
	}
}

