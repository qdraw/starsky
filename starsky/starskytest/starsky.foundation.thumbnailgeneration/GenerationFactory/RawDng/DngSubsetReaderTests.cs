using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.thumbnailgeneration.GenerationFactory.RawDng;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.RawDng;

[TestClass]
public class DngSubsetReaderTests
{
	[TestMethod]
	public void TryLoad_WithMinimalUncompressed16BitCfaDng_LoadsBayerAndMetadata()
	{
		using var ms = BuildMinimalDng();

		var ok = DngSubsetReader.TryLoad(ms, out var image, out var error);

		Assert.IsTrue(ok, error);
		Assert.IsNotNull(image);
		Assert.AreEqual(2, image.Width);
		Assert.AreEqual(2, image.Height);
		Assert.AreEqual(16, image.BitsPerSample);
		// BlackLevel and WhiteLevel are now arrays
		Assert.IsTrue(image.BlackLevel.Length >= 1);
		Assert.AreEqual(64f, image.BlackLevel[0]);
		Assert.IsTrue(image.WhiteLevel.Length >= 1);
		Assert.AreEqual(1023f, image.WhiteLevel[0]);
		CollectionAssert.AreEqual(new byte[] { 0, 1, 1, 2 }, image.CfaPattern);
		Assert.AreEqual(( ushort ) 100, image.Bayer[0, 0]);
		Assert.AreEqual(( ushort ) 200, image.Bayer[0, 1]);
		Assert.AreEqual(( ushort ) 300, image.Bayer[1, 0]);
		Assert.AreEqual(( ushort ) 400, image.Bayer[1, 1]);
	}

	[TestMethod]
	public void TryLoad_WithMinimalUncompressed8BitCfaDng_Loads8BitPixels()
	{
		var raw = new byte[] { 10, 20, 30, 40 };
		using var ms = BuildMinimalDng(8, raw, raw.Length);

		var ok = DngSubsetReader.TryLoad(ms, out var image, out var error);

		Assert.IsTrue(ok, error);
		Assert.IsNotNull(image);
		Assert.AreEqual(8, image.BitsPerSample);
		Assert.AreEqual(( ushort ) 10, image.Bayer[0, 0]);
		Assert.AreEqual(( ushort ) 20, image.Bayer[0, 1]);
		Assert.AreEqual(( ushort ) 30, image.Bayer[1, 0]);
		Assert.AreEqual(( ushort ) 40, image.Bayer[1, 1]);
	}

	[TestMethod]
	public void TryLoad_WithMinimalUncompressed14BitPackedCfaDng_LoadsPackedPixels()
	{
		var raw = Pack14LittleEndian([100, 200, 300, 400]);
		using var ms = BuildMinimalDng(14, raw, raw.Length);

		var ok = DngSubsetReader.TryLoad(ms, out var image, out var error);

		Assert.IsTrue(ok, error);
		Assert.IsNotNull(image);
		Assert.AreEqual(14, image.BitsPerSample);
		Assert.AreEqual(( ushort ) 100, image.Bayer[0, 0]);
		Assert.AreEqual(( ushort ) 200, image.Bayer[0, 1]);
		Assert.AreEqual(( ushort ) 300, image.Bayer[1, 0]);
		Assert.AreEqual(( ushort ) 400, image.Bayer[1, 1]);
	}

	[TestMethod]
	public void TryLoad_WithD50Illuminant_StoresLeicaIlluminant()
	{
		// Leica cameras typically use D50 illuminant (code 23)
		using var ms = BuildMinimalDng(16, new byte[8], 8, illuminant: 23);

		var ok = DngSubsetReader.TryLoad(ms, out var image, out var error);

		Assert.IsTrue(ok, error);
		Assert.IsNotNull(image);
		Assert.AreEqual(( ushort ) 23, image.CalibrationIlluminant1, "Leica D50 illuminant");
	}

	[TestMethod]
	public void TryLoad_WithMissingIlluminant_DefaultsToD65()
	{
		// When illuminant is missing, should default to D65 (21), not unknown (0)
		using var ms = BuildMinimalDng(16, new byte[8], 8, illuminant: null);

		var ok = DngSubsetReader.TryLoad(ms, out var image, out var error);

		Assert.IsTrue(ok, error);
		Assert.IsNotNull(image);
		Assert.AreEqual(( ushort ) 21, image.CalibrationIlluminant1, "Should default to D65");
	}

	[TestMethod]
	public void TryLoad_WithPerChannelBlackWhiteLevels_PreservesArray()
	{
		// Leica stores per-CFA-site black/white levels like [60, 50, 50, 60]
		var blackLevels = new float[] { 60f, 50f, 50f, 60f };
		var whiteLevels = new float[] { 4000f, 4000f, 4000f, 4000f };
		using var ms = BuildMinimalDng(16, new byte[8], 8, blackLevels: blackLevels, whiteLevels: whiteLevels);

		var ok = DngSubsetReader.TryLoad(ms, out var image, out var error);

		Assert.IsTrue(ok, error);
		Assert.IsNotNull(image);
		CollectionAssert.AreEqual(blackLevels, image.BlackLevel);
		CollectionAssert.AreEqual(whiteLevels, image.WhiteLevel);
	}

	[TestMethod]
	public void TryLoad_WithReducedPreviewRawAndFullRawSubIfds_SelectsFullResolutionRaw()
	{
		using var ms = BuildDngWithPreviewAndFullRawSubIfds();

		var ok = DngSubsetReader.TryLoad(ms, out var image, out var error);

		Assert.IsTrue(ok, error);
		Assert.IsNotNull(image);
		Assert.AreEqual(4, image.Width);
		Assert.AreEqual(4, image.Height);
		Assert.AreEqual(16, image.BitsPerSample);
		Assert.AreEqual(( ushort ) 100, image.Bayer[0, 0]);
		Assert.AreEqual(( ushort ) 1600, image.Bayer[3, 3]);
	}

	private static MemoryStream BuildMinimalDng()
	{
		var raw = new byte[8];
		WriteU16(raw, 0, 100);
		WriteU16(raw, 2, 200);
		WriteU16(raw, 4, 300);
		WriteU16(raw, 6, 400);
		return BuildMinimalDng(16, raw, raw.Length);
	}

	private static MemoryStream BuildMinimalDng(ushort bitsPerSample = 16, byte[]? rawPayload = null,
		int stripByteCount = 0, float[]? blackLevels = null, float[]? whiteLevels = null,
		ushort? illuminant = null)
	{
		if ( rawPayload == null )
		{
			rawPayload = new byte[8];
			WriteU16(rawPayload, 0, 100);
			WriteU16(rawPayload, 2, 200);
			WriteU16(rawPayload, 4, 300);
			WriteU16(rawPayload, 6, 400);
		}

		stripByteCount = rawPayload.Length;

		var data = new byte[512];

		// TIFF header: little-endian, magic 42, IFD0 at offset 8
		data[0] = ( byte ) 'I';
		data[1] = ( byte ) 'I';
		WriteU16(data, 2, 42);
		WriteU32(data, 4, 8);

		// Prepare black/white level arrays
		blackLevels ??= [64f, 64f, 64f, 64f];
		whiteLevels ??= [1023f, 1023f, 1023f, 1023f];

		// Count entries needed
		var hasBlackArray = blackLevels.Length > 1;
		var hasWhiteArray = whiteLevels.Length > 1;
		var entryCount = 14 + ( hasBlackArray ? 1 : 0 ) + ( hasWhiteArray ? 1 : 0 );

		WriteU16(data, 8, ( ushort ) entryCount);
		var entryBase = 10;

		var rawDataOffset = 220u;
		var asShotNeutralOffset = 240u;
		var colorMatrixOffset = 264u;
		var blackLevelOffset = 300u;
		var whiteLevelOffset = 332u;

		var idx = 0;
		WriteIfdEntry(data, entryBase + idx++ * 12, 0x0100, 4, 1, 2); // width
		WriteIfdEntry(data, entryBase + idx++ * 12, 0x0101, 4, 1, 2); // height
		WriteIfdEntry(data, entryBase + idx++ * 12, 0x0102, 3, 1, bitsPerSample); // bits
		WriteIfdEntry(data, entryBase + idx++ * 12, 0x0103, 3, 1, 1); // compression
		WriteIfdEntry(data, entryBase + idx++ * 12, 0x0106, 3, 1, 32803); // CFA photometric
		WriteIfdEntry(data, entryBase + idx++ * 12, 0x0111, 4, 1, rawDataOffset); // strip offset
		WriteIfdEntry(data, entryBase + idx++ * 12, 0x0115, 3, 1, 1); // samples per pixel
		WriteIfdEntry(data, entryBase + idx++ * 12, 0x0116, 4, 1, 2); // rows per strip
		WriteIfdEntry(data, entryBase + idx++ * 12, 0x0117, 4, 1, ( uint ) stripByteCount); // strip byte counts
		WriteIfdEntry(data, entryBase + idx++ * 12, 0x828D, 3, 2, 0x00020002); // CFA repeat 2x2 inline
		WriteIfdEntry(data, entryBase + idx++ * 12, 0x828E, 1, 4, 0x02010100); // CFA pattern RGGB inline

		if ( hasBlackArray )
		{
			WriteIfdEntry(data, entryBase + idx++ * 12, 0xC61A, 5, ( uint ) blackLevels.Length, blackLevelOffset); // black array
		}
		else
		{
			WriteIfdEntry(data, entryBase + idx++ * 12, 0xC61A, 3, 1, ( uint ) ( int ) blackLevels[0]); // black scalar
		}

		if ( hasWhiteArray )
		{
			WriteIfdEntry(data, entryBase + idx++ * 12, 0xC61D, 5, ( uint ) whiteLevels.Length, whiteLevelOffset); // white array
		}
		else
		{
			WriteIfdEntry(data, entryBase + idx++ * 12, 0xC61D, 3, 1, ( uint ) ( int ) whiteLevels[0]); // white scalar
		}

		WriteIfdEntry(data, entryBase + idx++ * 12, 0xC628, 5, 3, asShotNeutralOffset); // AsShotNeutral

		if ( illuminant.HasValue )
		{
			WriteIfdEntry(data, entryBase + idx++ * 12, 0xC65A, 3, 1, illuminant.Value); // CalibrationIlluminant1
		}

		// Next IFD = 0
		WriteU32(data, entryBase + entryCount * 12, 0);

		Array.Copy(rawPayload, 0, data, ( int ) rawDataOffset,
			Math.Min(rawPayload.Length, data.Length - ( int ) rawDataOffset));

		// AsShotNeutral rationals: 2/1,1/1,2/1
		WriteRational(data, ( int ) asShotNeutralOffset + 0, 2, 1);
		WriteRational(data, ( int ) asShotNeutralOffset + 8, 1, 1);
		WriteRational(data, ( int ) asShotNeutralOffset + 16, 2, 1);

		// Black level array
		if ( hasBlackArray && blackLevelOffset + blackLevels.Length * 8 <= data.Length )
		{
			for ( var i = 0; i < blackLevels.Length; i++ )
			{
				WriteRational(data, ( int ) blackLevelOffset + i * 8, ( uint ) blackLevels[i], 1);
			}
		}

		// White level array
		if ( hasWhiteArray && whiteLevelOffset + whiteLevels.Length * 8 <= data.Length )
		{
			for ( var i = 0; i < whiteLevels.Length; i++ )
			{
				WriteRational(data, ( int ) whiteLevelOffset + i * 8, ( uint ) whiteLevels[i], 1);
			}
		}

		return new MemoryStream(data);
	}

	private static byte[] Pack14LittleEndian(ushort[] values)
	{
		var totalBits = values.Length * 14;
		var packed = new byte[( totalBits + 7 ) / 8];
		var bitIndex = 0;
		foreach ( var value in values )
		{
			for ( var bit = 0; bit < 14; bit++ )
			{
				if ( ( ( value >> bit ) & 0x1 ) != 0 )
				{
					var targetBit = bitIndex + bit;
					packed[targetBit / 8] |= ( byte ) ( 1 << ( targetBit % 8 ) );
				}
			}

			bitIndex += 14;
		}

		return packed;
	}

	private static MemoryStream BuildDngWithPreviewAndFullRawSubIfds()
	{
		var previewRaw = new byte[] { 1, 2, 3, 4 };
		var fullRaw = new byte[4 * 4 * 2];
		for ( var i = 0; i < 16; i++ )
		{
			WriteU16(fullRaw, i * 2, ( ushort ) ( ( i + 1 ) * 100 ) );
		}

		var data = new byte[1024];

		data[0] = ( byte ) 'I';
		data[1] = ( byte ) 'I';
		WriteU16(data, 2, 42);
		WriteU32(data, 4, 8);

		const uint subIfdArrayOffset = 32;
		const uint previewIfdOffset = 80;
		const uint fullIfdOffset = 256;
		const uint previewRawOffset = 192;
		const uint fullRawOffset = 512;

		WriteU16(data, 8, 1);
		WriteIfdEntry(data, 10, 0x014A, 4, 2, subIfdArrayOffset);
		WriteU32(data, 22, 0);

		WriteU32(data, ( int ) subIfdArrayOffset, previewIfdOffset);
		WriteU32(data, ( int ) subIfdArrayOffset + 4, fullIfdOffset);

		WriteRawSubIfd(data, ( int ) previewIfdOffset, width: 2, height: 2, bitsPerSample: 8,
			rawOffset: previewRawOffset, stripByteCount: previewRaw.Length, newSubFileType: 1);
		WriteRawSubIfd(data, ( int ) fullIfdOffset, width: 4, height: 4, bitsPerSample: 16,
			rawOffset: fullRawOffset, stripByteCount: fullRaw.Length, newSubFileType: 0);

		Array.Copy(previewRaw, 0, data, ( int ) previewRawOffset, previewRaw.Length);
		Array.Copy(fullRaw, 0, data, ( int ) fullRawOffset, fullRaw.Length);

		return new MemoryStream(data);
	}

	private static void WriteRawSubIfd(byte[] data, int offset, uint width, uint height,
		ushort bitsPerSample, uint rawOffset, int stripByteCount, uint newSubFileType)
	{
		WriteU16(data, offset, 10);
		var entryBase = offset + 2;
		var idx = 0;
		WriteIfdEntry(data, entryBase + idx++ * 12, 0x00FE, 4, 1, newSubFileType);
		WriteIfdEntry(data, entryBase + idx++ * 12, 0x0100, 4, 1, width);
		WriteIfdEntry(data, entryBase + idx++ * 12, 0x0101, 4, 1, height);
		WriteIfdEntry(data, entryBase + idx++ * 12, 0x0102, 3, 1, bitsPerSample);
		WriteIfdEntry(data, entryBase + idx++ * 12, 0x0103, 3, 1, 1);
		WriteIfdEntry(data, entryBase + idx++ * 12, 0x0106, 3, 1, 32803);
		WriteIfdEntry(data, entryBase + idx++ * 12, 0x0111, 4, 1, rawOffset);
		WriteIfdEntry(data, entryBase + idx++ * 12, 0x0115, 3, 1, 1);
		WriteIfdEntry(data, entryBase + idx++ * 12, 0x0116, 4, 1, height);
		WriteIfdEntry(data, entryBase + idx++ * 12, 0x0117, 4, 1, ( uint ) stripByteCount);
		WriteU32(data, entryBase + 10 * 12, 0);
	}

	private static void WriteIfdEntry(byte[] data, int offset, ushort tag, ushort type,
		uint count, uint value)
	{
		WriteU16(data, offset, tag);
		WriteU16(data, offset + 2, type);
		WriteU32(data, offset + 4, count);
		WriteU32(data, offset + 8, value);
	}

	private static void WriteU16(byte[] data, int offset, ushort value)
	{
		data[offset] = ( byte ) ( value & 0xFF );
		data[offset + 1] = ( byte ) ( ( value >> 8 ) & 0xFF );
	}

	private static void WriteU32(byte[] data, int offset, uint value)
	{
		data[offset] = ( byte ) ( value & 0xFF );
		data[offset + 1] = ( byte ) ( ( value >> 8 ) & 0xFF );
		data[offset + 2] = ( byte ) ( ( value >> 16 ) & 0xFF );
		data[offset + 3] = ( byte ) ( ( value >> 24 ) & 0xFF );
	}

	private static void WriteRational(byte[] data, int offset, uint numerator, uint denominator)
	{
		WriteU32(data, offset, numerator);
		WriteU32(data, offset + 4, denominator);
	}

	private static void WriteSignedRational(byte[] data, int offset, int numerator,
		int denominator)
	{
		WriteU32(data, offset, unchecked(( uint ) numerator));
		WriteU32(data, offset + 4, unchecked(( uint ) denominator));
	}
}


