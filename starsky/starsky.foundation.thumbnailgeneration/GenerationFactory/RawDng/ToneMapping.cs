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
		var value = linear <= 0f ? 0f : ( float ) Math.Pow(linear, invGamma);
		value = curve switch
		{
			ToneCurve.Hable => Hable(value),
			ToneCurve.Aces => Aces(value),
			_ => value
		};
		return Math.Clamp(value, 0f, 1f);
	}

	private static float Hable(float x)
	{
		const float a = 0.15f;
		const float b = 0.50f;
		const float c = 0.10f;
		const float d = 0.20f;
		const float e = 0.02f;
		const float f = 0.30f;
		const float w = 11.2f;

		var t = ( x * ( a * x + c * b ) + d * e ) / ( x * ( a * x + b ) + d * f ) - e / f;
		var tw = ( w * ( a * w + c * b ) + d * e ) / ( w * ( a * w + b ) + d * f ) - e / f;
		if ( tw <= 0f )
		{
			return 0f;
		}

		return t / tw;
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

