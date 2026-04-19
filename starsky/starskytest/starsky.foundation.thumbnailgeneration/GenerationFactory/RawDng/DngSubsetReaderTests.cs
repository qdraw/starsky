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

	private static MemoryStream BuildMinimalDng()
	{
		var raw = new byte[8];
		WriteU16(raw, 0, 100);
		WriteU16(raw, 2, 200);
		WriteU16(raw, 4, 300);
		WriteU16(raw, 6, 400);
		return BuildMinimalDng(16, raw, raw.Length);
	}

	private static MemoryStream BuildMinimalDng(ushort bitsPerSample, byte[] rawPayload,
		int stripByteCount)
	{
		var data = new byte[512];

		// TIFF header: little-endian, magic 42, IFD0 at offset 8
		data[0] = ( byte ) 'I';
		data[1] = ( byte ) 'I';
		WriteU16(data, 2, 42);
		WriteU32(data, 4, 8);

		var entryCount = 14;
		WriteU16(data, 8, ( ushort ) entryCount);
		var entryBase = 10;

		var rawDataOffset = 220u;
		var asShotNeutralOffset = 240u;
		var colorMatrixOffset = 264u;

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
		WriteIfdEntry(data, entryBase + idx++ * 12, 0xC61A, 3, 1, 64); // black
		WriteIfdEntry(data, entryBase + idx++ * 12, 0xC61D, 3, 1, 1023); // white
		WriteIfdEntry(data, entryBase + idx * 12, 0xC628, 5, 3, asShotNeutralOffset); // AsShotNeutral

		// Next IFD = 0
		WriteU32(data, entryBase + entryCount * 12, 0);

		Array.Copy(rawPayload, 0, data, ( int ) rawDataOffset,
			Math.Min(rawPayload.Length, data.Length - ( int ) rawDataOffset));

		// AsShotNeutral rationals: 2/1,1/1,2/1
		WriteRational(data, ( int ) asShotNeutralOffset + 0, 2, 1);
		WriteRational(data, ( int ) asShotNeutralOffset + 8, 1, 1);
		WriteRational(data, ( int ) asShotNeutralOffset + 16, 2, 1);

		// Optional ColorMatrix1 (identity) to exercise parser
		for ( var i = 0; i < 9; i++ )
		{
			WriteSignedRational(data, ( int ) colorMatrixOffset + i * 8, i % 4 == 0 ? 1 : 0, 1);
		}

		// Patch in ColorMatrix1 entry by replacing AsShotNeutral slot if needed is intentionally omitted in minimal case.

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


