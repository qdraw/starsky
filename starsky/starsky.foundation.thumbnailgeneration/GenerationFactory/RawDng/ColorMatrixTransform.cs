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

	// Bradford chromatic adaptation matrices used to normalize the profile's
	// white-balanced camera neutral to the D65 white point expected by sRGB.
	private static readonly float[,] Bradford =
	{
		{ 0.8951f, 0.2664f, -0.1614f },
		{ -0.7502f, 1.7135f, 0.0367f },
		{ 0.0389f, -0.0685f, 1.0296f }
	};

	private static readonly float[,] BradfordInverse =
	{
		{ 0.9869929f, -0.1470543f, 0.1599627f },
		{ 0.4323053f, 0.5183603f, 0.0492912f },
		{ -0.0085287f, 0.0400428f, 0.9684867f }
	};

	private static readonly float[] D65White = [0.95047f, 1f, 1.08883f];

	internal static float[,] BuildCameraToSrgb(float[,] colorMatrix1,
		float[,] forwardMatrix1, ushort calibrationIlluminant1,
		float[,]? cameraCalibration1 = null,
		float[,]? colorMatrix2 = null,
		float[,]? forwardMatrix2 = null,
		ushort? calibrationIlluminant2 = null,
		float[,]? cameraCalibration2 = null,
		float[]? asShotNeutral = null)
	{
		if ( !TryBuildCameraToXyz(colorMatrix1, forwardMatrix1,
			cameraCalibration1, out var cameraToXyz1) )
		{
			return Identity3X3();
		}

		var cameraToXyz = cameraToXyz1;
		if ( colorMatrix2 != null && calibrationIlluminant2.HasValue &&
		    TryBuildCameraToXyz(colorMatrix2,
			    forwardMatrix2 ?? Identity3X3(),
			    cameraCalibration2,
			    out var cameraToXyz2) &&
		    TryEstimateCctFromDualProfiles(asShotNeutral, cameraToXyz1, cameraToXyz2,
			    out var asShotCct) )
		{
			var cct1 = IlluminantToCct(calibrationIlluminant1);
			var cct2 = IlluminantToCct(calibrationIlluminant2.Value);
			var w = ComputeReciprocalTemperatureWeight(asShotCct, cct1, cct2);
			cameraToXyz = Lerp3X3(cameraToXyz1, cameraToXyz2, w);
		}

		cameraToXyz = NormalizeWhiteBalancedNeutralToD65(cameraToXyz);

		// Final transform from the profile connection space (XYZ) to sRGB.
		return Multiply3X3(XyzToSrgb, cameraToXyz);
	}

	private static bool TryBuildCameraToXyz(float[,] colorMatrix, float[,] forwardMatrix,
		float[,]? cameraCalibration, out float[,] cameraToXyz)
	{
		cameraToXyz = Identity3X3();
		var cameraCalibrationInv = Identity3X3();
		if ( cameraCalibration != null && !IsIdentity3X3(cameraCalibration) &&
		     TryInvert3X3(cameraCalibration, out var invCalibration) )
		{
			cameraCalibrationInv = invCalibration;
		}

		if ( !IsIdentity3X3(forwardMatrix) )
		{
			cameraToXyz = Multiply3X3(forwardMatrix, cameraCalibrationInv);
			return true;
		}

		if ( !TryInvert3X3(colorMatrix, out var invertedCameraToXyz) )
		{
			return false;
		}

		cameraToXyz = Multiply3X3(invertedCameraToXyz, cameraCalibrationInv);

		return true;
	}

	private static float[,] NormalizeWhiteBalancedNeutralToD65(float[,] cameraToXyz)
	{
		var sourceWhite = Multiply3X3Vector(cameraToXyz, 1f, 1f, 1f);
		if ( !TryBuildBradfordAdaptation(sourceWhite, D65White, out var adaptation) )
		{
			return cameraToXyz;
		}

		return Multiply3X3(adaptation, cameraToXyz);
	}

	private static bool TryBuildBradfordAdaptation(float[] sourceWhite, float[] targetWhite,
		out float[,] adaptation)
	{
		adaptation = Identity3X3();
		if ( sourceWhite.Length < 3 || targetWhite.Length < 3 )
		{
			return false;
		}

		var sourceCone = Multiply3X3Vector(Bradford, sourceWhite[0], sourceWhite[1], sourceWhite[2]);
		var targetCone = Multiply3X3Vector(Bradford, targetWhite[0], targetWhite[1], targetWhite[2]);
		for ( var i = 0; i < 3; i++ )
		{
			if ( !float.IsFinite(sourceCone[i]) || !float.IsFinite(targetCone[i]) ||
			     sourceCone[i] <= 1e-6f || targetCone[i] <= 1e-6f )
			{
				return false;
			}
		}

		var coneScale = new[,]
		{
			{ targetCone[0] / sourceCone[0], 0f, 0f },
			{ 0f, targetCone[1] / sourceCone[1], 0f },
			{ 0f, 0f, targetCone[2] / sourceCone[2] }
		};

		adaptation = Multiply3X3(BradfordInverse, Multiply3X3(coneScale, Bradford));
		return true;
	}

	private static bool TryEstimateCctFromDualProfiles(float[]? asShotNeutral,
		float[,] cameraToXyz1, float[,] cameraToXyz2, out float cct)
	{
		cct = 0f;
		var ok1 = TryEstimateCctFromAsShotNeutral(asShotNeutral, cameraToXyz1, out var cct1);
		var ok2 = TryEstimateCctFromAsShotNeutral(asShotNeutral, cameraToXyz2, out var cct2);

		if ( ok1 && ok2 )
		{
			// Blend in reciprocal-temperature domain for better stability.
			var r1 = 1f / Math.Max(1000f, cct1);
			var r2 = 1f / Math.Max(1000f, cct2);
			var r = ( r1 + r2 ) * 0.5f;
			cct = 1f / r;
			return true;
		}

		if ( ok1 )
		{
			cct = cct1;
			return true;
		}

		if ( ok2 )
		{
			cct = cct2;
			return true;
		}

		return false;
	}

	private static bool TryEstimateCctFromAsShotNeutral(float[]? asShotNeutral,
		float[,] cameraToXyz, out float cct)
	{
		cct = 0f;
		if ( asShotNeutral == null || asShotNeutral.Length < 3 )
		{
			return false;
		}

		var r = asShotNeutral[0];
		var g = asShotNeutral[1];
		var b = asShotNeutral[2];
		if ( r <= 0f || g <= 0f || b <= 0f )
		{
			return false;
		}

		var x = cameraToXyz[0, 0] * r + cameraToXyz[0, 1] * g + cameraToXyz[0, 2] * b;
		var y = cameraToXyz[1, 0] * r + cameraToXyz[1, 1] * g + cameraToXyz[1, 2] * b;
		var z = cameraToXyz[2, 0] * r + cameraToXyz[2, 1] * g + cameraToXyz[2, 2] * b;
		var sum = x + y + z;
		if ( sum <= 1e-8f )
		{
			return false;
		}

		var chromaX = x / sum;
		var chromaY = y / sum;
		if ( chromaY <= 1e-6f )
		{
			return false;
		}

		// McCamy approximation
		var n = ( chromaX - 0.3320f ) / ( 0.1858f - chromaY );
		cct = 449f * n * n * n + 3525f * n * n + 6823.3f * n + 5520.33f;
		return cct is > 1000f and < 25000f && !float.IsNaN(cct) && !float.IsInfinity(cct);
	}

	private static float IlluminantToCct(ushort illuminant)
	{
		return illuminant switch
		{
			17 => 2856f, // Standard Light A
			20 => 5503f, // D55
			21 => 6504f, // D65
			22 => 7504f, // D75
			23 => 5003f, // D50
			_ => 5500f
		};
	}

	private static float ComputeReciprocalTemperatureWeight(float asShotCct, float cct1,
		float cct2)
	{
		var r = 1f / Math.Max(1000f, asShotCct);
		var r1 = 1f / Math.Max(1000f, cct1);
		var r2 = 1f / Math.Max(1000f, cct2);
		var denom = r1 - r2;
		if ( Math.Abs(denom) < 1e-8f )
		{
			return 0.5f;
		}

		var w = ( r - r2 ) / denom;
		return Math.Clamp(w, 0f, 1f);
	}

	private static float[,] Lerp3X3(float[,] a, float[,] b, float t)
	{
		var result = new float[3, 3];
		for ( var r = 0; r < 3; r++ )
		{
			for ( var c = 0; c < 3; c++ )
			{
				result[r, c] = a[r, c] * t + b[r, c] * ( 1f - t );
			}
		}

		return result;
	}

	private static bool IsIdentity3X3(float[,] m)
	{
		for ( var row = 0; row < 3; row++ )
		{
			for ( var col = 0; col < 3; col++ )
			{
				var expected = row == col ? 1f : 0f;
				if ( Math.Abs(m[row, col] - expected) > 1e-6f )
				{
					return false;
				}
			}
		}

		return true;
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

				// Apply matrix. Clamp output to >= 0 to handle out-of-gamut pixels.
				// DO NOT clamp the matrix itself — negative coefficients are essential
				// for correct hue discrimination.
				linearRgb[y, x, 0] = Math.Max(0f, cameraToSrgb[0, 0] * r + cameraToSrgb[0, 1] * g + cameraToSrgb[0, 2] * b);
				linearRgb[y, x, 1] = Math.Max(0f, cameraToSrgb[1, 0] * r + cameraToSrgb[1, 1] * g + cameraToSrgb[1, 2] * b);
				linearRgb[y, x, 2] = Math.Max(0f, cameraToSrgb[2, 0] * r + cameraToSrgb[2, 1] * g + cameraToSrgb[2, 2] * b);
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

	private static float[] Multiply3X3Vector(float[,] matrix, float x, float y, float z)
	{
		return
		[
			matrix[0, 0] * x + matrix[0, 1] * y + matrix[0, 2] * z,
			matrix[1, 0] * x + matrix[1, 1] * y + matrix[1, 2] * z,
			matrix[2, 0] * x + matrix[2, 1] * y + matrix[2, 2] * z
		];
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

