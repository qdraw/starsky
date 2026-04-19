using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.thumbnailgeneration.GenerationFactory.RawDng;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.RawDng;

[TestClass]
public class RawDngRealFilesFlowTests
{
	public TestContext TestContext { get; set; } = null!;

	[TestMethod]
	public void TryRunToJpeg_WithProvidedRealDngFiles_ReportsPerFileResult()
	{
		string[] files =
		[
			"/Users/dion/data/testcontent/raws/20260308_210002_DSC05386-Verbeterd-NR.dng",
			"/Users/dion/data/testcontent/raws/Apple - iPhone XS - 16bit (4_3).dng",
			"/Users/dion/data/testcontent/raws/Apple iPhone 13 Pro (ProRAW mode) IMG_3234.dng",
			"/Users/dion/data/testcontent/raws/Canon - EOS 5D Mark III - 16bit 16bit RAW.dng",
			"/Users/dion/data/testcontent/raws/DJI - FC7303 - 16bit (16_9).dng",
			"/Users/dion/data/testcontent/raws/Google - Pixel 8 Pro - 16bit (4_3).dng",
			"/Users/dion/data/testcontent/raws/HUAWEI - EVA-AL00 - 16bit (4_3).dng",
			"/Users/dion/data/testcontent/raws/Leica - M (Typ 240) - 16bit 16bit compressed (3_2).dng",
			"/Users/dion/data/testcontent/raws/leica_cl_01.dng",
			"/Users/dion/data/testcontent/raws/OnePlus - ONEPLUS A6003 - 16bit (4_3).dng",
			"/Users/dion/data/testcontent/raws/Pentax - K-3 II - 14bit (3_2).dng",
			"/Users/dion/data/testcontent/raws/Pentax - K-S1 - 12bit (3_2).dng",
			"/Users/dion/data/testcontent/raws/pentax_k_1_mark_ii_01.dng",
			"/Users/dion/data/testcontent/raws/Plustek - OpticFilm 8100 - 16bit (4_3).dng",
			"/Users/dion/data/testcontent/raws/RAW_LEICA_M8.dng",
			"/Users/dion/data/testcontent/raws/Samsung - Galaxy S22 Ulra - 4_3.dng",
			"/Users/dion/data/testcontent/raws/Xiaomi - Redmi Note 7 - 16bit (4_3).dng"
		];

		var failed = new List<string>();
		var succeeded = new List<string>();
		var unsupported = new List<string>();

		// Create a temp output directory for generated JPEG files
		var tempDir = Path.Combine(Path.GetTempPath(), "starsky_rawdng_tests");

		if ( Directory.Exists(tempDir) )
		{
			Directory.Delete(tempDir, true);
		}
		
		Directory.CreateDirectory(tempDir);

		foreach ( var file in files )
		{
			if ( !File.Exists(file) )
			{
				failed.Add($"MISSING|{file}");
				continue;
			}

			// Inspect DNG metadata before processing
			var dngInfo = InspectDngMetadata(file);
			TestContext.WriteLine($"FILE_TYPE|{Path.GetFileName(file)}|{dngInfo}");

			using var input = File.OpenRead(file);
			using var captureInput = File.OpenRead(file);
			// Create a unique output filename in the temp folder
			var baseName = Path.GetFileNameWithoutExtension(file);
			if ( string.IsNullOrEmpty(baseName) )
			{
				baseName = "output";
			}

			// sanitize baseName for file system just in case
			foreach ( var c in Path.GetInvalidFileNameChars() )
			{
				baseName = baseName.Replace(c, '_');
			}

			var outputPath = Path.Combine(tempDir,
				baseName + "_" + Guid.NewGuid().ToString("N") + ".jpg");
			var rawCaptureBase = Path.Combine(tempDir,
				baseName + "_" + Guid.NewGuid().ToString("N"));
			if ( DngSubsetReader.TryExtractRawCapture(captureInput, out var capture,
				    out var captureError) && capture != null )
			{
				WriteRawCaptureArtifacts(rawCaptureBase, capture);
				TestContext.WriteLine(
					$"RAW_CAPTURE|{file}|meta={rawCaptureBase}.rawmeta.txt|data={rawCaptureBase}.rawpayload.bin");
			}
			else
			{
				TestContext.WriteLine($"RAW_CAPTURE_FAIL|{file}|{captureError}");
			}

			using var output = File.Open(outputPath, FileMode.Create, FileAccess.Write);
			var ok = RawDngPipelineRunner.TryRunToJpeg(input, output, out var error);
			output.Flush();
			long outLength = 0;
			try
			{
				outLength = new FileInfo(outputPath).Length;
			}
			catch
			{
				// if for some reason getting file info fails, fall back to stream length
				outLength = output.Length;
			}

			var result = $"{( ok ? "OK" : "FAIL" )}|{outLength}|{error}|{file}|{outputPath}";
			TestContext.WriteLine(result);
			if ( ok )
			{
				succeeded.Add(result);
			}
			else if ( IsExpectedUnsupported(error) )
			{
				unsupported.Add(result);
			}
			else
			{
				failed.Add(result);
			}
		}

		TestContext.WriteLine(
			$"SUMMARY|ok={succeeded.Count}|unsupported={unsupported.Count}|fail={failed.Count}");
		Assert.IsGreaterThan(0, succeeded.Count,
			"Expected at least one DNG file to pass current subset pipeline.");


		if ( failed.Count > 0 )
		{
			Assert.Fail("DNG flow failures:\n" + string.Join("\n", failed));
		}
	}

	private static void WriteRawCaptureArtifacts(string outputBasePath, DngRawCapture capture)
	{
		var metaPath = outputBasePath + ".rawmeta.txt";
		var payloadPath = outputBasePath + ".rawpayload.bin";
		using ( var sw = new StreamWriter(metaPath) )
		{
			sw.WriteLine($"width={capture.Width}");
			sw.WriteLine($"height={capture.Height}");
			sw.WriteLine($"bitsPerSample={capture.BitsPerSample}");
			sw.WriteLine($"compression={capture.Compression}");
			sw.WriteLine($"isTiled={capture.IsTiled}");
			sw.WriteLine($"rowsPerStrip={capture.RowsPerStrip}");
			sw.WriteLine($"tileWidth={capture.TileWidth}");
			sw.WriteLine($"tileLength={capture.TileLength}");
			sw.WriteLine($"cfaPattern={string.Join(',', capture.CfaPattern)}");
			sw.WriteLine($"chunkCount={capture.EncodedChunks.Length}");
			for ( var i = 0; i < capture.EncodedChunks.Length; i++ )
			{
				sw.WriteLine($"chunk[{i}].length={capture.EncodedChunks[i].Length}");
			}
		}

		using var fs = File.Open(payloadPath, FileMode.Create, FileAccess.Write);
		for ( var i = 0; i < capture.EncodedChunks.Length; i++ )
		{
			fs.Write(capture.EncodedChunks[i], 0, capture.EncodedChunks[i].Length);
		}
	}

	private static string InspectDngMetadata(string filePath)
	{
		try
		{
			using var fs = File.OpenRead(filePath);
			// Read minimal TIFF header to extract basic info
			var header = new byte[8];
			if ( fs.Read(header, 0, 8) < 8 )
			{
				return "INVALID_HEADER";
			}

			// Check byte order
			var littleEndian = header[0] == 'I' && header[1] == 'I';
			if ( !littleEndian && !( header[0] == 'M' && header[1] == 'M' ) )
			{
				return "NOT_TIFF";
			}

		// Extract from IFD0 first to see if there are SubIFDs
		uint width = 0, height = 0, bitsPerSample = 0, compression = 0;
		var ifdOffset = ReadUInt32(header, 4, littleEndian);
		var subIfdOffset = 0u;

		if ( ifdOffset > 0 && ifdOffset < fs.Length )
		{
			var (w, h, b, c, sub) = ReadIfdMetadata(fs, ifdOffset, littleEndian);
			width = w;
			height = h;
			bitsPerSample = b;
			compression = c;
			subIfdOffset = sub;
		}

		// If there's a SubIFD with CFA photometric (raw image), use that instead
		if ( subIfdOffset > 0 && subIfdOffset < fs.Length )
		{
			var (w, h, b, c, _) = ReadIfdMetadata(fs, subIfdOffset, littleEndian);
			if ( w > 0 && h > 0 ) // SubIFD has valid dimensions
			{
				width = w;
				height = h;
				bitsPerSample = b;
				compression = c;
			}
		}

			var compressionName = compression switch
			{
				1 => "Uncompressed",
				8 => "Deflate",
				32946 => "AdobeDeflate",
				7 => "JPEG",
				_ => $"Unknown({compression})"
			};

			return
				$"{width}x{height}|{bitsPerSample}bit|{compressionName}";
		}
		catch ( Exception ex )
		{
			return $"ERROR:{ex.Message}";
		}
	}

	private static (uint width, uint height, uint bits, uint compression, uint subIfd)
		ReadIfdMetadata(Stream fs, uint ifdOffset, bool littleEndian)
	{
		uint width = 0, height = 0, bits = 0, compression = 0, subIfd = 0;

		try
		{
			fs.Seek(ifdOffset, SeekOrigin.Begin);
			var countBuf = new byte[2];
			if ( fs.Read(countBuf, 0, 2) < 2 )
			{
				return (0, 0, 0, 0, 0);
			}

			var entryCount = ReadUInt16(countBuf, 0, littleEndian);
			for ( var i = 0; i < entryCount && fs.Position < fs.Length - 12; i++ )
			{
				var entry = new byte[12];
				if ( fs.Read(entry, 0, 12) < 12 )
				{
					break;
				}

				var tag = ReadUInt16(entry, 0, littleEndian);
				var type = ReadUInt16(entry, 2, littleEndian);
				var count = ReadUInt32(entry, 4, littleEndian);
				var value = ReadUInt32(entry, 8, littleEndian);

				if ( tag == 0x0100 )
				{
					width = value; // ImageWidth
				}
				else if ( tag == 0x0101 )
				{
					height = value; // ImageLength
				}
				else if ( tag == 0x0102 )
				{
					bits = value; // BitsPerSample
				}
				else if ( tag == 0x0103 )
				{
					compression = value; // Compression
				}
				else if ( tag == 0x014A && count > 0 )
				{
					// SubIFDs - get first one
					subIfd = value;
				}
			}
		}
		catch
		{
			// Ignore read errors
		}

		return (width, height, bits, compression, subIfd);
	}

	private static ushort ReadUInt16(byte[] data, int offset, bool littleEndian)
	{
		if ( offset + 1 >= data.Length )
		{
			return 0;
		}

		return littleEndian
			? ( ushort ) ( data[offset] | ( data[offset + 1] << 8 ) )
			: ( ushort ) ( ( data[offset] << 8 ) | data[offset + 1] );
	}

	private static uint ReadUInt32(byte[] data, int offset, bool littleEndian)
	{
		if ( offset + 3 >= data.Length )
		{
			return 0;
		}

		return littleEndian
			? ( uint ) ( data[offset] | ( data[offset + 1] << 8 ) | ( data[offset + 2] << 16 ) |
			             ( data[offset + 3] << 24 ) )
			: ( uint ) ( ( data[offset] << 24 ) | ( data[offset + 1] << 16 ) |
			             ( data[offset + 2] << 8 ) | data[offset + 3] );
	}

	private static bool IsExpectedUnsupported(string error)
	{
		return error.StartsWith("Only uncompressed DNG is supported",
			       StringComparison.OrdinalIgnoreCase)
		       || error.StartsWith("Unsupported bits per sample",
			       StringComparison.OrdinalIgnoreCase);
	}
}
