using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using starsky.foundation.thumbnailgeneration.GenerationFactory.RawDng;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.RawDng;

/// <summary>
///     Diagnostic utility to analyze Leica DNG metadata and identify rendering issues.
/// </summary>
internal static class LeicaDngDiagnostics
{
	public static string AnalyzeDngFile(string filePath)
	{
		var sb = new StringBuilder();
		sb.AppendLine($"=== Analyzing: {Path.GetFileName(filePath)} ===");

		try
		{
			using var input = File.OpenRead(filePath);

			// Try to load with our reader
			var loadSuccess = DngSubsetReader.TryLoad(input, out var raw, out var error);

			if ( !loadSuccess || raw == null )
			{
				sb.AppendLine($"❌ LOAD FAILED: {error}");
				return sb.ToString();
			}

			sb.AppendLine("✓ Loaded successfully");
			sb.AppendLine();

			// Dump metadata
			sb.AppendLine("📐 DIMENSIONS:");
			sb.AppendLine($"  Width: {raw.Width}");
			sb.AppendLine($"  Height: {raw.Height}");
			sb.AppendLine($"  BitsPerSample: {raw.BitsPerSample}");
			sb.AppendLine();

			sb.AppendLine("🎨 COLOR INFO:");
			sb.AppendLine($"  CFA Pattern: [{string.Join(", ", raw.CfaPattern)}]");
			sb.AppendLine();

			sb.AppendLine("⚫ BLACK LEVEL:");
			sb.AppendLine(
				$"  Values ({raw.BlackLevel.Length}): [{string.Join(", ", raw.BlackLevel.Select(x => x.ToString("F1")))}]");
			var blackMin = raw.BlackLevel.Min();
			var blackMax = raw.BlackLevel.Max();
			if ( blackMax > 0 && blackMin < 0 )
			{
				sb.AppendLine("  ⚠️  MIXED SIGNS (negative blacklevel unusual!)");
			}

			if ( blackMax - blackMin > 100 )
			{
				sb.AppendLine($"  ⚠️  LARGE VARIANCE ({blackMax - blackMin:F1})");
			}

			sb.AppendLine();

			sb.AppendLine("⚪ WHITE LEVEL:");
			sb.AppendLine(
				$"  Values ({raw.WhiteLevel.Length}): [{string.Join(", ", raw.WhiteLevel.Select(x => x.ToString("F1")))}]");
			var whiteMin = raw.WhiteLevel.Min();
			var whiteMax = raw.WhiteLevel.Max();
			if ( whiteMin <= blackMax )
			{
				sb.AppendLine($"  ❌ ERROR: WhiteLevel ({whiteMin}) <= BlackLevel ({blackMax})!");
			}

			if ( whiteMax - whiteMin > 100 )
			{
				sb.AppendLine($"  ⚠️  LARGE VARIANCE ({whiteMax - whiteMin:F1})");
			}

			sb.AppendLine();

			sb.AppendLine("💡 ILLUMINANT:");
			sb.AppendLine(
				$"  CalibrationIlluminant1: {raw.CalibrationIlluminant1} ({IlluminantName(raw.CalibrationIlluminant1)})");
			if ( raw.CalibrationIlluminant2.HasValue )
			{
				sb.AppendLine(
					$"  CalibrationIlluminant2: {raw.CalibrationIlluminant2} ({IlluminantName(raw.CalibrationIlluminant2.Value)})");
			}

			if ( raw.CalibrationIlluminant1 == 0 )
			{
				sb.AppendLine("  ⚠️  UNKNOWN - Now defaults to D65 after fix");
			}
			else if ( raw.CalibrationIlluminant1 == 23 )
			{
				sb.AppendLine("  ✓ D50 detected - should get D50→D65 adaptation");
			}
			else
			{
				sb.AppendLine("  ℹ️  Non-standard illuminant code");
			}

			sb.AppendLine();

			sb.AppendLine("🤍 WHITE BALANCE (AsShotNeutral):");
			sb.AppendLine(
				$"  Values: [{string.Join(", ", raw.AsShotNeutral.Select(x => x.ToString("F3")))}]");
			if ( raw.AsShotNeutral.Any(x => x <= 0) )
			{
				sb.AppendLine("  ⚠️  ZERO/NEGATIVE value (unusual!)");
			}

			if ( raw.AsShotNeutral[1] == 0 ) // Green = 0
			{
				sb.AppendLine("  ❌ ERROR: Green neutral is 0 (division by zero risk)!");
			}

			sb.AppendLine();

			sb.AppendLine("🎯 COLOR MATRICES:");
			sb.AppendLine($"  ColorMatrix1 present: {( raw.ColorMatrix1 != null ? "✓" : "✗" )}");
			sb.AppendLine($"    Value: [{DumpMatrix(raw.ColorMatrix1)}]");
			if ( raw.ColorMatrix2 != null )
			{
				sb.AppendLine("  ColorMatrix2 present: ✓");
			}

			sb.AppendLine(
				$"  ForwardMatrix1 present: {( raw.ForwardMatrix1 != null ? "✓" : "✗" )}");
			if ( raw.ForwardMatrix2 != null )
			{
				sb.AppendLine("  ForwardMatrix2 present: ✓");
			}

			sb.AppendLine();

			sb.AppendLine("🔧 CALIBRATION:");
			sb.AppendLine(
				$"  CameraCalibration1: {( IsIdentity(raw.CameraCalibration1) ? "Identity" : "Custom" )}");
			sb.AppendLine(
				$"  CameraCalibration2: {( IsIdentity(raw.CameraCalibration2) ? "Identity" : "Custom" )}");
			sb.AppendLine();

			// Diagnostic analysis
			sb.AppendLine("🔍 ANALYSIS:");
			var issues = new List<string>();

			if ( raw.BlackLevel.Any(x => x < 0) )
			{
				issues.Add("Negative black level");
			}

			if ( raw.BlackLevel.Max() >= raw.WhiteLevel.Min() )
			{
				issues.Add("Black level >= white level");
			}

			if ( Math.Abs(raw.AsShotNeutral[1] - 1f) > 0.5f )
			{
				issues.Add("Unusual green white balance");
			}

			if ( raw.CalibrationIlluminant1 == 23 && raw.ColorMatrix1 == null )
			{
				issues.Add("D50 illuminant but no color matrix");
			}

			if ( issues.Count == 0 )
			{
				sb.AppendLine("  ✓ No obvious issues detected");
			}
			else
			{
				sb.AppendLine($"  ⚠️  Found {issues.Count} potential issues:");
				foreach ( var issue in issues )
				{
					sb.AppendLine($"     - {issue}");
				}
			}

			sb.AppendLine();

			// Recommendations
			sb.AppendLine("💡 RECOMMENDATIONS:");
			if ( raw.CalibrationIlluminant1 == 23 )
			{
				sb.AppendLine("  • D50 camera - ensure D50→D65 adaptation is applied");
			}

			if ( raw.BlackLevel.Length == 4 && raw.BlackLevel.Distinct().Count() > 1 )
			{
				sb.AppendLine(
					"  • Different black levels per channel - verify RawNormalization logic");
			}

			if ( Math.Abs(raw.AsShotNeutral.Sum() - 3f) > 0.5f )
			{
				sb.AppendLine(
					"  • Unusual white balance - may indicate metadata issue or special lighting");
			}

			if ( raw.WhiteLevel.Max() < 4096 )
			{
				sb.AppendLine("  • White level < 4096 unusual - verify encoding");
			}

			sb.AppendLine();
		}
		catch ( Exception ex )
		{
			sb.AppendLine($"❌ ERROR: {ex.Message}");
		}

		return sb.ToString();
	}

	private static string IlluminantName(ushort illuminant)
	{
		return illuminant switch
		{
			0 => "Unknown",
			1 => "Daylight",
			17 => "D55",
			20 => "D65",
			21 => "D65",
			23 => "D50",
			_ => $"Other({illuminant})"
		};
	}

	private static string DumpMatrix(float[,]? matrix)
	{
		if ( matrix == null )
		{
			return "null";
		}

		var values = new float[9];
		for ( var i = 0; i < 3; i++ )
		{
			for ( var j = 0; j < 3; j++ )
			{
				values[i * 3 + j] = matrix[i, j];
			}
		}

		return $"[{string.Join(", ", values.Select(x => x.ToString("F3")))}]";
	}

	private static bool IsIdentity(float[,] matrix)
	{
		return matrix[0, 0] == 1 && matrix[0, 1] == 0 && matrix[0, 2] == 0 &&
		       matrix[1, 0] == 0 && matrix[1, 1] == 1 && matrix[1, 2] == 0 &&
		       matrix[2, 0] == 0 && matrix[2, 1] == 0 && matrix[2, 2] == 1;
	}
}
