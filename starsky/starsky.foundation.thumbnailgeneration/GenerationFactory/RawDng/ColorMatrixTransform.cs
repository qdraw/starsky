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

		return Multiply3X3(XyzToSrgb, cameraToXyz);
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

