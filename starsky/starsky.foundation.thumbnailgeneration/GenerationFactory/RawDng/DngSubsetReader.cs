using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("starskytest")]

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.RawDng;

internal sealed class DngRawImage
{
	public required ushort[,] Bayer { get; init; }
	public required int Width { get; init; }
	public required int Height { get; init; }
	public required int BitsPerSample { get; init; }
	public required float[] BlackLevel { get; init; } // Per-channel black levels (up to 4 values for CFA channels)
	public required float[] WhiteLevel { get; init; } // Per-channel white levels (up to 4 values for CFA channels)
	public required float[] AsShotNeutral { get; init; }
	public required float[,] ColorMatrix1 { get; init; }
	public float[,]? ColorMatrix2 { get; init; }
	public required byte[] CfaPattern { get; init; }
	public required float[,] ForwardMatrix1 { get; init; } // Optional: used if ColorMatrix1 absent
	public float[,]? ForwardMatrix2 { get; init; }
	public required float[,] CameraCalibration1 { get; init; } // Optional: applies per-channel color correction
	public required float[,] CameraCalibration2 { get; init; } // Optional: second calibration
	public required ushort CalibrationIlluminant1 { get; init; } // Illuminant for ColorMatrix1
	public ushort? CalibrationIlluminant2 { get; init; }
}

internal sealed class DngRawCapture
{
	public required int Width { get; init; }
	public required int Height { get; init; }
	public required int BitsPerSample { get; init; }
	public required ushort Compression { get; init; }
	public required bool IsTiled { get; init; }
	public required int RowsPerStrip { get; init; }
	public required int TileWidth { get; init; }
	public required int TileLength { get; init; }
	public required byte[] CfaPattern { get; init; }
	public required byte[][] EncodedChunks { get; init; }
}

internal static class DngSubsetReader
{
	private const ushort TagSubIfds = 0x014A;
	private const ushort TagImageWidth = 0x0100;
	private const ushort TagImageLength = 0x0101;
	private const ushort TagBitsPerSample = 0x0102;
	private const ushort TagCompression = 0x0103;
	private const ushort TagPhotometricInterpretation = 0x0106;
	private const ushort TagStripOffsets = 0x0111;
	private const ushort TagSamplesPerPixel = 0x0115;
	private const ushort TagRowsPerStrip = 0x0116;
	private const ushort TagStripByteCounts = 0x0117;
	private const ushort TagPredictor = 0x013D;
	private const ushort TagTileWidth = 0x0142;
	private const ushort TagTileLength = 0x0143;
	private const ushort TagTileOffsets = 0x0144;
	private const ushort TagTileByteCounts = 0x0145;
	private const ushort TagCfaRepeatPatternDim = 0x828D;
	private const ushort TagCfaPattern = 0x828E;
	private const ushort TagBlackLevel = 0xC61A;
	private const ushort TagWhiteLevel = 0xC61D;
	private const ushort TagAsShotNeutral = 0xC628;
	private const ushort TagAsShotWhiteXy = 0xC629;
	private const ushort TagAnalogBalance = 0xC627;
	private const ushort TagExifIfd = 0x8769;
	private const ushort TagColorMatrix1 = 0xC621;
	private const ushort TagColorMatrix2 = 0xC622;
	private const ushort TagForwardMatrix1 = 0xC714;
	private const ushort TagForwardMatrix2 = 0xC715;
	private const ushort TagCameraCalibration1 = 0xC723;
	private const ushort TagCameraCalibration2 = 0xC724;
	private const ushort TagCalibrationIlluminant1 = 0xC65A;
	private const ushort TagCalibrationIlluminant2 = 0xC65B;

	private const ushort CompressionUncompressed = 1;
	private const ushort CompressionJpegOldStyle = 6;
	private const ushort CompressionJpeg = 7;
	private const ushort CompressionDeflate = 8;
	private const ushort CompressionAdobeDeflate = 32946;
	private const ushort PhotometricCfa = 32803;

	public static bool TryLoad(Stream input, out DngRawImage? image, out string error)
	{
		image = null;
		error = string.Empty;

		if ( !TryParseHeader(input, out var littleEndian, out var firstIfdOffset) )
		{
			error = "Invalid TIFF/DNG header";
			return false;
		}

		var ifd0 = ReadIfd(input, firstIfdOffset, littleEndian);
		if ( ifd0 == null )
		{
			error = "Failed to read IFD0";
			return false;
		}

		var rawIfd = ResolveRawIfd(input, littleEndian, ifd0) ?? ifd0;
		if ( !TryBuildRawImage(input, littleEndian, rawIfd, ifd0, out image, out error) )
		{
			return false;
		}

		return true;
	}

	internal static bool TryExtractRawCapture(Stream input, out DngRawCapture? capture,
		out string error)
	{
		capture = null;
		error = string.Empty;

		if ( !TryParseHeader(input, out var littleEndian, out var firstIfdOffset) )
		{
			error = "Invalid TIFF/DNG header";
			return false;
		}

		var ifd0 = ReadIfd(input, firstIfdOffset, littleEndian);
		if ( ifd0 == null )
		{
			error = "Failed to read IFD0";
			return false;
		}

		var rawIfd = ResolveRawIfd(input, littleEndian, ifd0) ?? ifd0;
		if ( !TryGetUnsigned(input, littleEndian, rawIfd, TagImageWidth, out var widthU) ||
		     !TryGetUnsigned(input, littleEndian, rawIfd, TagImageLength, out var heightU) ||
		     !TryGetUnsigned(input, littleEndian, rawIfd, TagBitsPerSample, out var bitsU) ||
		     !TryGetUnsigned(input, littleEndian, rawIfd, TagCompression, out var compU) )
		{
			error = "Missing RAW IFD metadata";
			return false;
		}

		var hasTiles = TryGetUnsignedArray(input, littleEndian, rawIfd, TagTileOffsets,
			out var tileOffsets) && tileOffsets.Length > 0;
		var hasStrips = TryGetUnsignedArray(input, littleEndian, rawIfd, TagStripOffsets,
			out var stripOffsets) && stripOffsets.Length > 0;

		if ( !hasTiles && !hasStrips )
		{
			error = "No strip/tile payload found in RAW IFD";
			return false;
		}

		uint[] offsets;
		uint[] counts;
		var rowsPerStrip = TryGetUnsigned(input, littleEndian, rawIfd, TagRowsPerStrip, out var rowsU)
			? ( int ) rowsU
			: 0;
		var tileWidth = TryGetUnsigned(input, littleEndian, rawIfd, TagTileWidth, out var tw)
			? ( int ) tw
			: 0;
		var tileLength = TryGetUnsigned(input, littleEndian, rawIfd, TagTileLength, out var tl)
			? ( int ) tl
			: 0;

		if ( hasTiles )
		{
			if ( !TryGetUnsignedArray(input, littleEndian, rawIfd, TagTileByteCounts,
				    out var tileCounts) || tileCounts.Length != tileOffsets.Length )
			{
				error = "Invalid tile offsets/counts metadata";
				return false;
			}

			offsets = tileOffsets;
			counts = tileCounts;
		}
		else
		{
			if ( !TryGetUnsignedArray(input, littleEndian, rawIfd, TagStripByteCounts,
				    out var stripCounts) || stripCounts.Length != stripOffsets.Length )
			{
				error = "Invalid strip offsets/counts metadata";
				return false;
			}

			offsets = stripOffsets;
			counts = stripCounts;
		}

		var chunks = new byte[offsets.Length][];
		for ( var i = 0; i < offsets.Length; i++ )
		{
			var chunk = new byte[counts[i]];
			if ( !TrySeekAndRead(input, offsets[i], chunk, 0, chunk.Length) )
			{
				error = "Failed to read RAW payload chunk";
				return false;
			}

			chunks[i] = chunk;
		}

		var cfaPattern = TryGetByteArray(input, littleEndian, rawIfd, TagCfaPattern, out var cfa) &&
		                 cfa.Length >= 4
			? cfa.Take(4).ToArray()
			: [0, 1, 1, 2];

		capture = new DngRawCapture
		{
			Width = ( int ) widthU,
			Height = ( int ) heightU,
			BitsPerSample = ( int ) bitsU,
			Compression = ( ushort ) compU,
			IsTiled = hasTiles,
			RowsPerStrip = rowsPerStrip,
			TileWidth = tileWidth,
			TileLength = tileLength,
			CfaPattern = cfaPattern,
			EncodedChunks = chunks
		};

		return true;
	}

	/// <summary>
	/// Scans all SubIFDs in the DNG for a JPEG-compressed, non-raw (non-CFA) preview image
	/// and writes the largest found (by pixel area) to <paramref name="output"/>.
	/// </summary>
	public static bool TryExtractJpegPreview(Stream input, Stream output, out string error)
	{
		error = string.Empty;

		if ( !TryParseHeader(input, out var littleEndian, out var firstIfdOffset) )
		{
			error = "Invalid TIFF/DNG header";
			return false;
		}

		var ifd0 = ReadIfd(input, firstIfdOffset, littleEndian);
		if ( ifd0 == null )
		{
			error = "Failed to read IFD0";
			return false;
		}

		// Scan all SubIFDs; collect (jpegBytes, pixelArea) pairs for JPEG-compressed, non-CFA IFDs
		var candidates = new List<(byte[] Data, long Area)>();

		if ( TryGetUnsignedArray(input, littleEndian, ifd0, TagSubIfds, out var subIfdOffsets) )
		{
			foreach ( var subOffset in subIfdOffsets )
			{
				var sub = ReadIfd(input, subOffset, littleEndian);
				if ( sub == null )
				{
					continue;
				}

				if ( !TryGetUnsigned(input, littleEndian, sub, TagCompression, out var comp) )
				{
					continue;
				}

				// Only standard JPEG (old or new style), not raw lossless JPEG variants
				if ( comp is not ( CompressionJpegOldStyle or CompressionJpeg ) )
				{
					continue;
				}

				// Skip CFA (raw sensor) IFDs — we only want rendered color previews
				if ( TryGetUnsigned(input, littleEndian, sub, TagPhotometricInterpretation,
					    out var photo) && photo == PhotometricCfa )
				{
					continue;
				}

				TryGetUnsigned(input, littleEndian, sub, TagImageWidth, out var w);
				TryGetUnsigned(input, littleEndian, sub, TagImageLength, out var h);

				if ( !TryGetUnsignedArray(input, littleEndian, sub, TagStripOffsets,
					    out var stripOffsets) ||
				     !TryGetUnsignedArray(input, littleEndian, sub, TagStripByteCounts,
					    out var stripCounts) ||
				     stripOffsets.Length == 0 || stripOffsets.Length != stripCounts.Length )
				{
					continue;
				}

				var totalBytes = checked(( int ) stripCounts.Sum(c => ( long ) c));
				var jpegBytes = new byte[totalBytes];
				var cursor = 0;
				var readOk = true;
				for ( var i = 0; i < stripOffsets.Length; i++ )
				{
					if ( !TrySeekAndRead(input, stripOffsets[i], jpegBytes, cursor,
						    ( int ) stripCounts[i]) )
					{
						readOk = false;
						break;
					}

					cursor += ( int ) stripCounts[i];
				}

				if ( !readOk || jpegBytes.Length < 4 )
				{
					continue;
				}

				// Verify JPEG SOI signature
				if ( jpegBytes[0] != 0xFF || jpegBytes[1] != 0xD8 )
				{
					continue;
				}

				candidates.Add(( jpegBytes, ( long ) w * h ));
			}
		}

		if ( candidates.Count == 0 )
		{
			error = "No JPEG preview SubIFD found in DNG";
			return false;
		}

		// Write the highest-resolution candidate
		var best = candidates.OrderByDescending(c => c.Area).First().Data;
		output.Write(best, 0, best.Length);
		return true;
	}

	private static IfdDirectory? ResolveRawIfd(Stream input, bool littleEndian, IfdDirectory ifd0)
	{
		if ( TryGetUnsignedArray(input, littleEndian, ifd0, TagSubIfds, out var subIfdOffsets) )
		{
			foreach ( var offset in subIfdOffsets )
			{
				var sub = ReadIfd(input, offset, littleEndian);
				if ( sub != null && IsRawIfd(input, littleEndian, sub) )
				{
					return sub;
				}
			}
		}

		return IsRawIfd(input, littleEndian, ifd0) ? ifd0 : null;
	}

	private static bool IsRawIfd(Stream input, bool littleEndian, IfdDirectory ifd)
	{
		if ( !TryGetUnsigned(input, littleEndian, ifd, TagPhotometricInterpretation,
			    out var photometric) )
		{
			return false;
		}

		return photometric == PhotometricCfa;
	}

	private static bool TryBuildRawImage(Stream input, bool littleEndian, IfdDirectory ifd,
		IfdDirectory ifd0,
		out DngRawImage? image, out string error)
	{
		image = null;
		error = string.Empty;

		if ( !TryGetUnsigned(input, littleEndian, ifd, TagImageWidth, out var widthU) ||
		     !TryGetUnsigned(input, littleEndian, ifd, TagImageLength, out var heightU) ||
		     !TryGetUnsigned(input, littleEndian, ifd, TagBitsPerSample, out var bitsPerSampleU) )
		{
			error = "Missing width/height/bits metadata";
			return false;
		}

		if ( !TryGetUnsigned(input, littleEndian, ifd, TagCompression, out var compression) )
		{
			error = "Only uncompressed DNG is supported in the subset reader";
			return false;
		}

		if ( compression is not ( CompressionUncompressed or CompressionDeflate or CompressionAdobeDeflate ) )
		{
			error = "Only uncompressed DNG is supported in the subset reader";
			return false;
		}

		// Check for tiled or strip-based image
		var hasTiles = TryGetUnsignedArray(input, littleEndian, ifd, TagTileOffsets,
			out var tileOffsets) && tileOffsets.Length > 0;
		var hasStrips = TryGetUnsignedArray(input, littleEndian, ifd, TagStripOffsets,
			out var stripOffsets) && stripOffsets.Length > 0;

		if ( !hasTiles && !hasStrips )
		{
			error = "Missing tile or strip data pointers";
			return false;
		}

		uint[] offsets, counts;
		int tileWidth = 0, tileLength = 0;

		if ( hasTiles )
		{
			// Tiled image
			if ( !TryGetUnsignedArray(input, littleEndian, ifd, TagTileByteCounts,
				    out var tileByteCounts) || tileByteCounts.Length == 0 )
			{
				error = "Missing tile byte counts";
				return false;
			}

			if ( tileOffsets.Length != tileByteCounts.Length )
			{
				error = "Tile offsets/counts mismatch";
				return false;
			}

			if ( !TryGetUnsigned(input, littleEndian, ifd, TagTileWidth, out var tw) ||
			     !TryGetUnsigned(input, littleEndian, ifd, TagTileLength, out var tl) )
			{
				error = "Missing tile dimensions";
				return false;
			}

			tileWidth = ( int ) tw;
			tileLength = ( int ) tl;
			offsets = tileOffsets;
			counts = tileByteCounts;
		}
		else
		{
			// Strip-based image
			if ( !TryGetUnsignedArray(input, littleEndian, ifd, TagStripByteCounts,
				    out var stripByteCounts) || stripByteCounts.Length == 0 )
			{
				error = "Missing strip byte counts";
				return false;
			}

			if ( stripOffsets.Length != stripByteCounts.Length )
			{
				error = "Strip offsets/counts mismatch";
				return false;
			}

			offsets = stripOffsets;
			counts = stripByteCounts;
		}

		var width = ( int ) widthU;
		var height = ( int ) heightU;
		var bitsPerSample = ( int ) bitsPerSampleU;
		var rowsPerStrip = TryGetUnsigned(input, littleEndian, ifd, TagRowsPerStrip, out var rowsU)
			? ( int ) rowsU
			: height;
		var predictor = TryGetUnsigned(input, littleEndian, ifd, TagPredictor, out var predictorU)
			? ( int ) predictorU
			: 1;

		if ( bitsPerSample is not ( 8 or 10 or 12 or 14 or 16 ) )
		{
			error = $"Unsupported bits per sample: {bitsPerSample}";
			return false;
		}

		if ( predictor != 1 )
		{
			error = $"Unsupported predictor: {predictor}";
			return false;
		}

		var bayer = new ushort[height, width];
		if ( hasTiles )
		{
			if ( !TryReadPixelsTiled(input, littleEndian, offsets, counts, bitsPerSample,
				    width, height, tileWidth, tileLength, ( ushort ) compression, bayer) )
			{
				error = "Failed to decode tile payload";
				return false;
			}
		}
		else
		{
			if ( !TryReadPixels(input, littleEndian, offsets, counts, bitsPerSample,
				    width, height, rowsPerStrip, ( ushort ) compression, bayer) )
			{
				error = "Failed to decode strip payload";
				return false;
			}
		}

		// Per-channel black levels (DNG spec allows array for CFA channels)
		var blackLevels = TryGetFloatArray(input, littleEndian, ifd, TagBlackLevel, out var blacks)
			&& blacks.Length > 0
			? blacks
			: [0f, 0f, 0f, 0f];
		
		// Per-channel white levels (DNG spec allows array for CFA channels)
		var defaultWhiteLevel = ( float ) ( ( 1 << Math.Min(16, bitsPerSample) ) - 1 );
		var whiteLevels = TryGetFloatArray(input, littleEndian, ifd, TagWhiteLevel, out var whites)
			&& whites.Length > 0
			? whites
			: [defaultWhiteLevel, defaultWhiteLevel, defaultWhiteLevel, defaultWhiteLevel];
		
 		var colorMatrix = TryGetFloatArray(input, littleEndian, ifd, TagColorMatrix1,
 			out var matrixValuesRaw) && matrixValuesRaw.Length >= 9
 			? To3x3(matrixValuesRaw)
 			: TryGetFloatArray(input, littleEndian, ifd0, TagColorMatrix1, out var matrixValues0) &&
 			  matrixValues0.Length >= 9
				? To3x3(matrixValues0)
				: Identity3x3();
		var colorMatrix2 = TryGetFloatArray(input, littleEndian, ifd, TagColorMatrix2,
			out var matrix2Raw) && matrix2Raw.Length >= 9
			? To3x3(matrix2Raw)
			: TryGetFloatArray(input, littleEndian, ifd0, TagColorMatrix2, out var matrix20) &&
			  matrix20.Length >= 9
				? To3x3(matrix20)
				: null;

		var asShotNeutral = TryGetAsShotNeutral(input, littleEndian, ifd, ifd0, out var neutral)
			? neutral
			: TryGetAsShotWhiteXy(input, littleEndian, ifd, ifd0, out var whiteXy) &&
			  TryComputeNeutralFromWhiteXy(whiteXy, colorMatrix, out var neutralFromXy)
				? neutralFromXy
				: [1f, 1f, 1f];

		// Read AnalogBalance metadata to stay aligned with DNG WB metadata coverage.
		// Not applied yet in this simplified pipeline, but useful for diagnostics/future tuning.
		_ = TryGetAnalogBalance(input, littleEndian, ifd, ifd0, out _);
 		var cfaPattern = TryGetByteArray(input, littleEndian, ifd, TagCfaPattern, out var cfa) &&
 		                 cfa.Length >= 4
 			? cfa.Take(4).ToArray()
 			: new byte[] { 0, 1, 1, 2 }; // fallback RGGB

		var forwardMatrix = TryGetFloatArray(input, littleEndian, ifd, TagForwardMatrix1,
			out var fwdRaw) && fwdRaw.Length >= 9
			? To3x3(fwdRaw)
			: TryGetFloatArray(input, littleEndian, ifd0, TagForwardMatrix1, out var fwd0) &&
			  fwd0.Length >= 9
				? To3x3(fwd0)
				: Identity3x3();
		var forwardMatrix2 = TryGetFloatArray(input, littleEndian, ifd, TagForwardMatrix2,
			out var fwd2Raw) && fwd2Raw.Length >= 9
			? To3x3(fwd2Raw)
			: TryGetFloatArray(input, littleEndian, ifd0, TagForwardMatrix2, out var fwd20) &&
			  fwd20.Length >= 9
				? To3x3(fwd20)
				: null;
		var cameraCalibration1 = TryGetFloatArray(input, littleEndian, ifd, TagCameraCalibration1,
			out var cal1Raw) && cal1Raw.Length >= 9
			? To3x3(cal1Raw)
			: TryGetFloatArray(input, littleEndian, ifd0, TagCameraCalibration1, out var cal10) &&
			  cal10.Length >= 9
				? To3x3(cal10)
				: Identity3x3();
		var cameraCalibration2 = TryGetFloatArray(input, littleEndian, ifd, TagCameraCalibration2,
			out var cal2Raw) && cal2Raw.Length >= 9
			? To3x3(cal2Raw)
			: TryGetFloatArray(input, littleEndian, ifd0, TagCameraCalibration2, out var cal20) &&
			  cal20.Length >= 9
				? To3x3(cal20)
				: Identity3x3();
		
		var calibrationIlluminant1 =
			TryGetUnsigned(input, littleEndian, ifd, TagCalibrationIlluminant1, out var illumRaw)
				? ( ushort ) illumRaw
				: TryGetUnsigned(input, littleEndian, ifd0, TagCalibrationIlluminant1,
					out var illum0)
					? ( ushort ) illum0
					: ( ushort ) 21; // Default to D65 (21) instead of unknown (0)
		var calibrationIlluminant2 =
			TryGetUnsigned(input, littleEndian, ifd, TagCalibrationIlluminant2, out var illum2Raw)
				? ( ushort? ) illum2Raw
				: TryGetUnsigned(input, littleEndian, ifd0, TagCalibrationIlluminant2,
					out var illum20)
					? ( ushort? ) illum20
					: ( ushort? ) null;
 
 		image = new DngRawImage
 		{
 			Bayer = bayer,
 			Width = width,
 			Height = height,
 			BitsPerSample = bitsPerSample,
 			BlackLevel = blackLevels,
 			WhiteLevel = whiteLevels,
 			AsShotNeutral = asShotNeutral,
 			ColorMatrix1 = colorMatrix,
 			ColorMatrix2 = colorMatrix2,
 			CfaPattern = cfaPattern,
 			ForwardMatrix1 = forwardMatrix,
 			ForwardMatrix2 = forwardMatrix2,
 			CameraCalibration1 = cameraCalibration1,
 			CameraCalibration2 = cameraCalibration2,
			CalibrationIlluminant1 = calibrationIlluminant1,
			CalibrationIlluminant2 = calibrationIlluminant2
 		};
 
 		return true;
 	}

	private static bool TryGetAsShotNeutral(Stream input, bool littleEndian, IfdDirectory rawIfd,
		IfdDirectory ifd0, out float[] neutral)
	{
		neutral = [1f, 1f, 1f];

		if ( TryReadNeutralFromIfd(input, littleEndian, rawIfd, out neutral) )
		{
			return true;
		}

		if ( TryReadNeutralFromIfd(input, littleEndian, ifd0, out neutral) )
		{
			return true;
		}

		if ( TryGetUnsigned(input, littleEndian, ifd0, TagExifIfd, out var exifIfdOffset) )
		{
			var exifIfd = ReadIfd(input, exifIfdOffset, littleEndian);
			if ( exifIfd != null && TryReadNeutralFromIfd(input, littleEndian, exifIfd, out neutral) )
			{
				return true;
			}
		}

		return false;
	}

	private static bool TryGetAsShotWhiteXy(Stream input, bool littleEndian,
		IfdDirectory rawIfd, IfdDirectory ifd0, out float[] whiteXy)
	{
		whiteXy = [0f, 0f];

		if ( TryReadWhiteXyFromIfd(input, littleEndian, rawIfd, out whiteXy) )
		{
			return true;
		}

		if ( TryReadWhiteXyFromIfd(input, littleEndian, ifd0, out whiteXy) )
		{
			return true;
		}

		if ( TryGetUnsigned(input, littleEndian, ifd0, TagExifIfd, out var exifIfdOffset) )
		{
			var exifIfd = ReadIfd(input, exifIfdOffset, littleEndian);
			if ( exifIfd != null && TryReadWhiteXyFromIfd(input, littleEndian, exifIfd, out whiteXy) )
			{
				return true;
			}
		}

		return false;
	}

	private static bool TryReadNeutralFromIfd(Stream input, bool littleEndian, IfdDirectory ifd,
		out float[] neutral)
	{
		neutral = [1f, 1f, 1f];
		if ( !TryGetFloatArray(input, littleEndian, ifd, TagAsShotNeutral, out var raw) ||
		     raw.Length < 3 )
		{
			return false;
		}

		neutral = [raw[0], raw[1], raw[2]];
		return true;
	}

	private static bool TryReadWhiteXyFromIfd(Stream input, bool littleEndian, IfdDirectory ifd,
		out float[] whiteXy)
	{
		whiteXy = [0f, 0f];
		if ( !TryGetFloatArray(input, littleEndian, ifd, TagAsShotWhiteXy, out var raw) ||
		     raw.Length < 2 )
		{
			return false;
		}

		whiteXy = [raw[0], raw[1]];
		return true;
	}

	private static bool TryComputeNeutralFromWhiteXy(float[] whiteXy, float[,] xyzToCamera,
		out float[] neutral)
	{
		neutral = [1f, 1f, 1f];
		if ( whiteXy.Length < 2 )
		{
			return false;
		}

		var x = whiteXy[0];
		var y = whiteXy[1];
		if ( x <= 0f || y <= 0f || x + y >= 1f )
		{
			return false;
		}

		// Convert xy (with Y=1) to XYZ.
		var xVal = x / y;
		const float yVal = 1f;
		var zVal = ( 1f - x - y ) / y;

		// ColorMatrix1 is XYZ->Camera in DNG. Multiply to get camera-space neutral.
		var r = xyzToCamera[0, 0] * xVal + xyzToCamera[0, 1] * yVal + xyzToCamera[0, 2] * zVal;
		var g = xyzToCamera[1, 0] * xVal + xyzToCamera[1, 1] * yVal + xyzToCamera[1, 2] * zVal;
		var b = xyzToCamera[2, 0] * xVal + xyzToCamera[2, 1] * yVal + xyzToCamera[2, 2] * zVal;

		if ( r <= 0f || g <= 0f || b <= 0f ||
		     float.IsNaN(r) || float.IsNaN(g) || float.IsNaN(b) ||
		     float.IsInfinity(r) || float.IsInfinity(g) || float.IsInfinity(b) )
		{
			return false;
		}

		neutral = [r, g, b];
		return true;
	}

	private static bool TryGetAnalogBalance(Stream input, bool littleEndian,
		IfdDirectory rawIfd, IfdDirectory ifd0, out float[] analog)
	{
		analog = [1f, 1f, 1f];
		if ( TryGetFloatArray(input, littleEndian, rawIfd, TagAnalogBalance, out var rawVals) &&
		     rawVals.Length >= 3 )
		{
			analog = [rawVals[0], rawVals[1], rawVals[2]];
			return true;
		}

		if ( TryGetFloatArray(input, littleEndian, ifd0, TagAnalogBalance, out var ifd0Vals) &&
		     ifd0Vals.Length >= 3 )
		{
			analog = [ifd0Vals[0], ifd0Vals[1], ifd0Vals[2]];
			return true;
		}

		if ( TryGetUnsigned(input, littleEndian, ifd0, TagExifIfd, out var exifIfdOffset) )
		{
			var exifIfd = ReadIfd(input, exifIfdOffset, littleEndian);
			if ( exifIfd != null &&
			     TryGetFloatArray(input, littleEndian, exifIfd, TagAnalogBalance, out var exifVals) &&
			     exifVals.Length >= 3 )
			{
				analog = [exifVals[0], exifVals[1], exifVals[2]];
				return true;
			}
		}

		return false;
	}
 
 	private static bool TryReadPixels(Stream input, bool littleEndian, IReadOnlyList<uint> offsets,
 		IReadOnlyList<uint> counts, int bitsPerSample, int width, int height, int rowsPerStrip,
 		ushort compression, ushort[,] bayer)
 	{
 		if ( width <= 0 || height <= 0 || rowsPerStrip <= 0 )
 		{
 			return false;
 		}

 		var rowCursor = 0;
 		for ( var i = 0; i < offsets.Count && rowCursor < height; i++ )
 		{
 			var count = ( int ) counts[i];
 			if ( count <= 0 )
 			{
 				return false;
 			}

 			var encoded = new byte[count];
 			if ( !TrySeekAndRead(input, offsets[i], encoded, 0, count) )
 			{
 				return false;
 			}

 			var payload = compression switch
 			{
 				CompressionUncompressed => encoded,
 				CompressionDeflate or CompressionAdobeDeflate => Inflate(encoded),
 				_ => null
 			};

 			if ( payload == null )
 			{
 				return false;
 			}

 			var rowsInStrip = Math.Min(rowsPerStrip, height - rowCursor);
 			var stripPixelCount = checked(rowsInStrip * width);
 			var decoded = DecodePixels(payload, littleEndian, bitsPerSample, stripPixelCount);
 			if ( decoded == null )
 			{
 				return false;
 			}

 			for ( var p = 0; p < decoded.Length; p++ )
 			{
 				var y = rowCursor + p / width;
 				var x = p % width;
 				bayer[y, x] = decoded[p];
 			}

 			rowCursor += rowsInStrip;
 		}

 		return rowCursor == height;
 	}

 	private static bool TryReadPixelsTiled(Stream input, bool littleEndian,
 		IReadOnlyList<uint> offsets, IReadOnlyList<uint> counts, int bitsPerSample, int width,
 		int height, int tileWidth, int tileLength, ushort compression, ushort[,] bayer)
 	{
 		if ( width <= 0 || height <= 0 || tileWidth <= 0 || tileLength <= 0 )
 		{
 			return false;
 		}

 		var tilesAcross = ( width + tileWidth - 1 ) / tileWidth;
 		var tilesDown = ( height + tileLength - 1 ) / tileLength;
 		if ( offsets.Count != tilesAcross * tilesDown )
 		{
 			return false;
 		}

 		var tileIndex = 0;
 		for ( var ty = 0; ty < tilesDown; ty++ )
 		{
 			for ( var tx = 0; tx < tilesAcross; tx++ )
 			{
 				if ( tileIndex >= offsets.Count )
 				{
 					return false;
 				}

 				var count = ( int ) counts[tileIndex];
 				if ( count <= 0 )
 				{
 					return false;
 				}

 				var encoded = new byte[count];
 				if ( !TrySeekAndRead(input, offsets[tileIndex], encoded, 0, count) )
 				{
 					return false;
 				}

 				var payload = compression switch
 				{
 					CompressionUncompressed => encoded,
 					CompressionDeflate or CompressionAdobeDeflate => Inflate(encoded),
 					_ => null
 				};

 				if ( payload == null )
 				{
 					return false;
 				}

 				var tilePixelCount = tileWidth * tileLength;
 				var decoded = DecodePixels(payload, littleEndian, bitsPerSample, tilePixelCount);
 				if ( decoded == null )
 				{
 					return false;
 				}

 				// Copy tile data into Bayer array
 				var tileStartY = ty * tileLength;
 				var tileStartX = tx * tileWidth;
 				for ( var p = 0; p < decoded.Length; p++ )
 				{
 					var py = p / tileWidth;
 					var px = p % tileWidth;
 					var y = tileStartY + py;
 					var x = tileStartX + px;
 					if ( y < height && x < width )
 					{
 						bayer[y, x] = decoded[p];
 					}
 				}

 				tileIndex++;
 			}
 		}

 		return true;
 	}

	private static byte[]? Inflate(byte[] compressed)
	{
		try
		{
			using var input = new MemoryStream(compressed);
			using var zlib = new ZLibStream(input, CompressionMode.Decompress, leaveOpen: false);
			using var output = new MemoryStream();
			zlib.CopyTo(output);
			return output.ToArray();
		}
		catch ( Exception zlibEx )
		{
			// zlib failed, try raw DEFLATE format
			try
			{
				using var input = new MemoryStream(compressed);
				using var deflate = new DeflateStream(input, CompressionMode.Decompress,
					leaveOpen: false);
				using var output = new MemoryStream();
				deflate.CopyTo(output);
				return output.ToArray();
			}
			catch ( Exception deflateEx )
			{
				// Both decompression methods failed
				// Log diagnostic info for debugging
				System.Diagnostics.Debug.WriteLine(
					$"Decompression failed: zlib={zlibEx.GetType().Name}, " +
					$"deflate={deflateEx.GetType().Name}");
				return null;
			}
		}
	}

 	private static ushort[]? DecodePixels(byte[] payload, bool littleEndian, int bitsPerSample,
 		int pixelCount)
 	{
 		if ( bitsPerSample == 8 )
 		{
 			return Decode8(payload, pixelCount);
 		}

 		if ( bitsPerSample == 16 )
 		{
 			return Decode16(payload, littleEndian, pixelCount);
 		}

 		if ( payload.Length >= pixelCount * 2 )
 		{
 			return DecodeWordStored(payload, littleEndian, bitsPerSample, pixelCount);
 		}

 		return littleEndian
 			? DecodePackedLittleEndian(payload, bitsPerSample, pixelCount)
 			: DecodePackedBigEndian(payload, bitsPerSample, pixelCount);
 	}

 	private static ushort[]? Decode8(byte[] payload, int pixelCount)
 	{
 		if ( payload.Length < pixelCount )
 		{
 			return null;
 		}

 		var result = new ushort[pixelCount];
 		for ( var i = 0; i < pixelCount; i++ )
 		{
 			result[i] = payload[i];
 		}

 		return result;
 	}

 	private static ushort[]? Decode16(byte[] payload, bool littleEndian, int pixelCount)
 	{
 		if ( payload.Length < pixelCount * 2 )
 		{
 			return null;
 		}

 		var result = new ushort[pixelCount];
 		for ( var i = 0; i < pixelCount; i++ )
 		{
 			var span = payload.AsSpan(i * 2, 2);
 			result[i] = littleEndian
 				? BinaryPrimitives.ReadUInt16LittleEndian(span)
 				: BinaryPrimitives.ReadUInt16BigEndian(span);
 		}

 		return result;
 	}

 	private static ushort[]? DecodeWordStored(byte[] payload, bool littleEndian, int bitsPerSample,
 		int pixelCount)
 	{
 		var words = Decode16(payload, littleEndian, pixelCount);
 		if ( words == null )
 		{
 			return null;
 		}

 		var mask = ( 1u << bitsPerSample ) - 1u;
 		for ( var i = 0; i < words.Length; i++ )
 		{
 			words[i] = ( ushort ) ( words[i] & mask );
 		}

 		return words;
 	}

 	private static ushort[]? DecodePackedLittleEndian(byte[] payload, int bitsPerSample,
 		int pixelCount)
 	{
 		var requiredBytes = ( pixelCount * bitsPerSample + 7 ) / 8;
 		if ( payload.Length < requiredBytes )
 		{
 			return null;
 		}

 		var result = new ushort[pixelCount];
 		var bitIndex = 0;
 		for ( var i = 0; i < pixelCount; i++ )
 		{
 			var byteIndex = bitIndex >> 3;
 			var bitOffset = bitIndex & 7;
 			ulong scratch = 0;
 			for ( var b = 0; b < 4 && byteIndex + b < payload.Length; b++ )
 			{
 				scratch |= ( ulong ) payload[byteIndex + b] << ( b * 8 );
 			}

 			result[i] = ( ushort ) ( ( scratch >> bitOffset ) & ( ( 1u << bitsPerSample ) - 1u ) );
 			bitIndex += bitsPerSample;
 		}

 		return result;
 	}

 	private static ushort[]? DecodePackedBigEndian(byte[] payload, int bitsPerSample,
 		int pixelCount)
 	{
 		var requiredBytes = ( pixelCount * bitsPerSample + 7 ) / 8;
 		if ( payload.Length < requiredBytes )
 		{
 			return null;
 		}

 		var result = new ushort[pixelCount];
 		var bitCursor = 0;
 		for ( var i = 0; i < pixelCount; i++ )
 		{
 			uint value = 0;
 			for ( var b = 0; b < bitsPerSample; b++ )
 			{
 				var absoluteBit = bitCursor + b;
 				var byteIndex = absoluteBit >> 3;
 				var bitInByte = 7 - ( absoluteBit & 7 );
 				var bit = ( payload[byteIndex] >> bitInByte ) & 1;
 				value = ( value << 1 ) | ( uint ) bit;
 			}

 			result[i] = ( ushort ) value;
 			bitCursor += bitsPerSample;
 		}

 		return result;
 	}

 	private static float[,] To3x3(IReadOnlyList<float> v)
 	{
 		return new[,]
 		{
 			{ v[0], v[1], v[2] },
 			{ v[3], v[4], v[5] },
 			{ v[6], v[7], v[8] }
 		};
 	}

 	private static float[,] Identity3x3()
 	{
 		return new[,]
 		{
 			{ 1f, 0f, 0f },
 			{ 0f, 1f, 0f },
 			{ 0f, 0f, 1f }
 		};
 	}

 	private static bool TryParseHeader(Stream input, out bool littleEndian, out uint firstIfd)
 	{
 		littleEndian = true;
 		firstIfd = 0;
 		Span<byte> header = stackalloc byte[8];
 		if ( input.Read(header) < 8 )
 		{
 			return false;
 		}

 		if ( header[0] == 'I' && header[1] == 'I' )
 		{
 			littleEndian = true;
 		}
 		else if ( header[0] == 'M' && header[1] == 'M' )
 		{
 			littleEndian = false;
 		}
 		else
 		{
 			return false;
 		}

 		var magic = ReadUInt16(header[2..], littleEndian);
 		if ( magic != 42 )
 		{
 			return false;
 		}

 		firstIfd = ReadUInt32(header[4..], littleEndian);
 		return firstIfd > 0 && firstIfd < input.Length;
 	}

 	private static IfdDirectory? ReadIfd(Stream input, uint offset, bool littleEndian)
 	{
 		if ( !TrySeek(input, offset) )
 		{
 			return null;
 		}

 		Span<byte> countBuf = stackalloc byte[2];
 		if ( input.Read(countBuf) < 2 )
 		{
 			return null;
 		}

 		var count = ReadUInt16(countBuf, littleEndian);
 		if ( count == 0 || count > 4096 )
 		{
 			return null;
 		}

 		var entries = new Dictionary<ushort, IfdEntry>();
 		for ( var i = 0; i < count; i++ )
 		{
 			Span<byte> e = stackalloc byte[12];
 			if ( input.Read(e) < 12 )
 			{
 				return null;
 			}

 			var tag = ReadUInt16(e, littleEndian);
 			entries[tag] = new IfdEntry
 			{
 				Type = ReadUInt16(e[2..], littleEndian),
 				Count = ReadUInt32(e[4..], littleEndian),
 				ValueOrOffset = ReadUInt32(e[8..], littleEndian)
 			};
 		}

 		return new IfdDirectory(entries);
 	}

 	private static bool TryGetUnsigned(Stream input, bool littleEndian, IfdDirectory ifd,
 		ushort tag, out uint value)
 	{
 		value = 0;
 		if ( !ifd.Entries.TryGetValue(tag, out var entry) )
 		{
 			return false;
 		}

 		var values = ReadUnsignedValues(input, littleEndian, entry, 1);
 		if ( values.Length == 0 )
 		{
 			return false;
 		}

 		value = values[0];
 		return true;
 	}

 	private static bool TryGetUnsignedArray(Stream input, bool littleEndian, IfdDirectory ifd,
 		ushort tag, out uint[] values)
 	{
 		values = [];
 		if ( !ifd.Entries.TryGetValue(tag, out var entry) )
 		{
 			return false;
 		}

 		values = ReadUnsignedValues(input, littleEndian, entry, ( int ) entry.Count);
 		return values.Length > 0;
 	}

 	private static bool TryGetByteArray(Stream input, bool littleEndian, IfdDirectory ifd,
 		ushort tag, out byte[] values)
 	{
 		values = [];
 		if ( !ifd.Entries.TryGetValue(tag, out var entry) || entry.Type != 1 )
 		{
 			return false;
 		}

 		values = ReadBytes(input, littleEndian, entry);
 		return values.Length > 0;
 	}

 	private static bool TryGetFloat(Stream input, bool littleEndian, IfdDirectory ifd,
 		ushort tag, out float value)
 	{
 		value = 0;
 		if ( !TryGetFloatArray(input, littleEndian, ifd, tag, out var values) || values.Length == 0 )
 		{
 			return false;
 		}

 		value = values[0];
 		return true;
 	}

 	private static bool TryGetFloatArray(Stream input, bool littleEndian, IfdDirectory ifd,
 		ushort tag, out float[] values)
 	{
 		values = [];
 		if ( !ifd.Entries.TryGetValue(tag, out var entry) )
 		{
 			return false;
 		}

 		values = entry.Type switch
 		{
 			3 or 4 => ReadUnsignedValues(input, littleEndian, entry, ( int ) entry.Count)
 				.Select(v => ( float ) v).ToArray(),
 			5 => ReadRationalValues(input, littleEndian, entry, signed: false),
 			10 => ReadRationalValues(input, littleEndian, entry, signed: true),
 			11 => ReadFloatValues(input, littleEndian, entry, isDouble: false),
 			12 => ReadFloatValues(input, littleEndian, entry, isDouble: true),
 			_ => []
 		};
 		return values.Length > 0;
 	}

	private static float[] ReadFloatValues(Stream input, bool littleEndian, IfdEntry entry,
		bool isDouble)
	{
		if ( entry.Count == 0 )
		{
			return [];
		}

		var typeSize = isDouble ? 8 : 4;
		var totalSize = checked(( int ) entry.Count * typeSize);
		var data = ReadRawValueBytes(input, littleEndian, entry, totalSize);
		if ( data.Length < totalSize )
		{
			return [];
		}

		var values = new float[entry.Count];
		for ( var i = 0; i < entry.Count; i++ )
		{
			if ( isDouble )
			{
				var bits = littleEndian
					? BinaryPrimitives.ReadUInt64LittleEndian(data.AsSpan(i * 8, 8))
					: BinaryPrimitives.ReadUInt64BigEndian(data.AsSpan(i * 8, 8));
				values[i] = ( float ) BitConverter.Int64BitsToDouble(( long ) bits);
			}
			else
			{
				var bits = littleEndian
					? BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(i * 4, 4))
					: BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(i * 4, 4));
				values[i] = BitConverter.Int32BitsToSingle(( int ) bits);
			}
		}

		return values;
	}

 	private static uint[] ReadUnsignedValues(Stream input, bool littleEndian, IfdEntry entry,
 		int wanted)
 	{
 		if ( entry.Type is not ( 3 or 4 ) || entry.Count == 0 )
 		{
 			return [];
 		}

 		var typeSize = entry.Type == 3 ? 2 : 4;
 		var totalSize = checked(( int ) entry.Count * typeSize);
 		var data = ReadRawValueBytes(input, littleEndian, entry, totalSize);
 		if ( data.Length < totalSize )
 		{
 			return [];
 		}

 		var count = Math.Min(( int ) entry.Count, wanted);
 		var result = new uint[count];
 		for ( var i = 0; i < count; i++ )
 		{
 			result[i] = entry.Type == 3
 				? ReadUInt16(data.AsSpan(i * 2, 2), littleEndian)
 				: ReadUInt32(data.AsSpan(i * 4, 4), littleEndian);
 		}

 		return result;
 	}

 	private static float[] ReadRationalValues(Stream input, bool littleEndian, IfdEntry entry,
 		bool signed)
 	{
 		if ( entry.Count == 0 )
 		{
 			return [];
 		}

 		var total = checked(( int ) entry.Count * 8);
 		if ( !TrySeek(input, entry.ValueOrOffset) )
 		{
 			return [];
 		}

 		var buf = new byte[total];
 		if ( input.Read(buf, 0, total) < total )
 		{
 			return [];
 		}

 		var values = new float[entry.Count];
 		for ( var i = 0; i < entry.Count; i++ )
 		{
 			var pos = i * 8;
 			if ( signed )
 			{
 				var n = ( int ) ReadUInt32(buf.AsSpan(pos, 4), littleEndian);
 				var d = ( int ) ReadUInt32(buf.AsSpan(pos + 4, 4), littleEndian);
 				values[i] = d == 0 ? 0 : ( float ) n / d;
 			}
 			else
 			{
 				var n = ReadUInt32(buf.AsSpan(pos, 4), littleEndian);
 				var d = ReadUInt32(buf.AsSpan(pos + 4, 4), littleEndian);
 				values[i] = d == 0 ? 0 : ( float ) n / d;
 			}
 		}

 		return values;
 	}

 	private static byte[] ReadBytes(Stream input, bool littleEndian, IfdEntry entry)
 	{
 		var total = ( int ) entry.Count;
 		if ( total <= 0 )
 		{
 			return [];
 		}

 		return ReadRawValueBytes(input, littleEndian, entry, total);
 	}

 	private static byte[] ReadRawValueBytes(Stream input, bool littleEndian, IfdEntry entry,
 		int totalSize)
 	{
 		if ( totalSize <= 4 )
 		{
 			Span<byte> inline = stackalloc byte[4];
 			if ( littleEndian )
 			{
 				BinaryPrimitives.WriteUInt32LittleEndian(inline, entry.ValueOrOffset);
 			}
 			else
 			{
 				BinaryPrimitives.WriteUInt32BigEndian(inline, entry.ValueOrOffset);
 			}

 			return inline[..totalSize].ToArray();
 		}

 		if ( !TrySeek(input, entry.ValueOrOffset) )
 		{
 			return [];
 		}

 		var bytes = new byte[totalSize];
 		return input.Read(bytes, 0, totalSize) >= totalSize ? bytes : [];
 	}

 	private static bool TrySeek(Stream input, long offset)
 	{
 		if ( !input.CanSeek || offset < 0 || offset >= input.Length )
 		{
 			return false;
 		}

 		input.Seek(offset, SeekOrigin.Begin);
 		return true;
 	}

 	private static bool TrySeekAndRead(Stream input, long offset, byte[] buffer,
 		int writeOffset, int count)
 	{
 		if ( !TrySeek(input, offset) )
 		{
 			return false;
 		}

 		return input.Read(buffer, writeOffset, count) >= count;
 	}

 	private static ushort ReadUInt16(ReadOnlySpan<byte> bytes, bool littleEndian)
 	{
 		return littleEndian
 			? BinaryPrimitives.ReadUInt16LittleEndian(bytes)
 			: BinaryPrimitives.ReadUInt16BigEndian(bytes);
 	}

 	private static uint ReadUInt32(ReadOnlySpan<byte> bytes, bool littleEndian)
 	{
 		return littleEndian
 			? BinaryPrimitives.ReadUInt32LittleEndian(bytes)
 			: BinaryPrimitives.ReadUInt32BigEndian(bytes);
 	}

 	private sealed class IfdDirectory(Dictionary<ushort, IfdEntry> entries)
 	{
 		public Dictionary<ushort, IfdEntry> Entries { get; } = entries;
 	}

 	private sealed class IfdEntry
 	{
 		public ushort Type { get; init; }
 		public uint Count { get; init; }
 		public uint ValueOrOffset { get; init; }
 	}
}

