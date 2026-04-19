using System;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.RawDng;

internal static class ColorMatrixTransform
{
	// Linear XYZ (D65) to linear sRGB
	private static readonly float[,] XyzToSrgb =
	{
		{ 3.2404542f, -1.5371385f, -0.4985314f },
		{ -0.9692660f, 1.8760108f, 0.0415560f },
		{ 0.0556434f, -0.2040259f, 1.0572252f }
	};

	internal static float[,] BuildCameraToSrgb(float[,] colorMatrix1)
	{
		if ( !TryInvert3X3(colorMatrix1, out var cameraToXyz) )
		{
			return Identity3X3();
		}

		var combined = Multiply3X3(XyzToSrgb, cameraToXyz);

		// Normalize each row so that a neutral camera input [1,1,1] maps to [1,1,1]
		// if the matrix structure permits (IsNormalizationSafe check).
		if ( IsNormalizationSafe(combined) )
		{
			NormalizeRowsForNeutral(combined);
		}

		// Clamp any negative values that might have resulted from matrix math
		ClampNegative(combined);

		return combined;
	}

	/// <summary>
	/// Check if normalizing the matrix rows would preserve the matrix structure
	/// (avoid inverting signs or creating huge amplifications).
	/// </summary>
	private static bool IsNormalizationSafe(float[,] m)
	{
		for ( var row = 0; row < 3; row++ )
		{
			var rowSum = m[row, 0] + m[row, 1] + m[row, 2];
			if ( Math.Abs(rowSum) < 1e-6f )
			{
				return false;
			}

			var inv = 1f / rowSum;
			// Check if normalization would invert signs or create values > 2
			for ( var col = 0; col < 3; col++ )
			{
				var normalized = m[row, col] * inv;
				var original = m[row, col];
				// If sign flips or amplification is extreme, skip normalization
				if ( Math.Sign(original) != Math.Sign(normalized) || Math.Abs(normalized) > 2f )
				{
					return false;
				}
			}
		}

		return true;
	}

	/// <summary>
	/// Clamps negative values to 0 to avoid color artifacts from matrix ringing.
	/// </summary>
	private static void ClampNegative(float[,] m)
	{
		for ( var row = 0; row < 3; row++ )
		{
			for ( var col = 0; col < 3; col++ )
			{
				if ( m[row, col] < 0f )
				{
					m[row, col] = 0f;
				}
			}
		}
	}

	/// <summary>
	/// Scales each row of <paramref name="m"/> so that
	/// <c>m * [1,1,1]^T == [1,1,1]^T</c>, i.e. a neutral (equal-channel)
	/// camera input produces a neutral sRGB output.
	/// </summary>
	private static void NormalizeRowsForNeutral(float[,] m)
	{
		for ( var row = 0; row < 3; row++ )
		{
			var rowSum = m[row, 0] + m[row, 1] + m[row, 2];
			if ( Math.Abs(rowSum) < 1e-6f )
			{
				continue;
			}

			var inv = 1f / rowSum;
			m[row, 0] *= inv;
			m[row, 1] *= inv;
			m[row, 2] *= inv;
		}
	}

	internal static void ApplyInPlace(float[,,] linearRgb, float[,] cameraToSrgb)
	{
		var height = linearRgb.GetLength(0);
		var width = linearRgb.GetLength(1);
		for ( var y = 0; y < height; y++ )
		{
			for ( var x = 0; x < width; x++ )
			{
				var r = linearRgb[y, x, 0];
				var g = linearRgb[y, x, 1];
				var b = linearRgb[y, x, 2];

				linearRgb[y, x, 0] = cameraToSrgb[0, 0] * r + cameraToSrgb[0, 1] * g +
				                    cameraToSrgb[0, 2] * b;
				linearRgb[y, x, 1] = cameraToSrgb[1, 0] * r + cameraToSrgb[1, 1] * g +
				                    cameraToSrgb[1, 2] * b;
				linearRgb[y, x, 2] = cameraToSrgb[2, 0] * r + cameraToSrgb[2, 1] * g +
				                    cameraToSrgb[2, 2] * b;
			}
		}
	}

	internal static bool TryInvert3X3(float[,] matrix, out float[,] inverse)
	{
		inverse = Identity3X3();
		if ( matrix.GetLength(0) != 3 || matrix.GetLength(1) != 3 )
		{
			return false;
		}

		var a = matrix[0, 0];
		var b = matrix[0, 1];
		var c = matrix[0, 2];
		var d = matrix[1, 0];
		var e = matrix[1, 1];
		var f = matrix[1, 2];
		var g = matrix[2, 0];
		var h = matrix[2, 1];
		var i = matrix[2, 2];

		var det = a * ( e * i - f * h ) - b * ( d * i - f * g ) + c * ( d * h - e * g );
		if ( Math.Abs(det) < 1e-12f )
		{
			return false;
		}

		var invDet = 1f / det;
		inverse = new[,]
		{
			{ ( e * i - f * h ) * invDet, ( c * h - b * i ) * invDet, ( b * f - c * e ) * invDet },
			{ ( f * g - d * i ) * invDet, ( a * i - c * g ) * invDet, ( c * d - a * f ) * invDet },
			{ ( d * h - e * g ) * invDet, ( b * g - a * h ) * invDet, ( a * e - b * d ) * invDet }
		};
		return true;
	}

	private static float[,] Multiply3X3(float[,] left, float[,] right)
	{
		var result = new float[3, 3];
		for ( var row = 0; row < 3; row++ )
		{
			for ( var col = 0; col < 3; col++ )
			{
				result[row, col] = left[row, 0] * right[0, col] +
				                   left[row, 1] * right[1, col] +
				                   left[row, 2] * right[2, col];
			}
		}

		return result;
	}

	private static float[,] Identity3X3()
	{
		return new[,]
		{
			{ 1f, 0f, 0f },
			{ 0f, 1f, 0f },
			{ 0f, 0f, 1f }
		};
	}
}

