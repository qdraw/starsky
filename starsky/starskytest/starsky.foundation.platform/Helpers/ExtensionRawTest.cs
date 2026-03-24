using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Helpers;

namespace starskytest.starsky.foundation.platform.Helpers;

[TestClass]
public sealed class ExtensionRawTest
{
	[TestMethod]
	public void Detect_Null_ReturnsUnknown()
	{
		var result = ExtensionRaw.Detect(null!);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.unknown, result);
	}

	[TestMethod]
	public void Detect_BytesTooShort_ReturnsUnknown()
	{
		var result = ExtensionRaw.Detect([0x49, 0x49, 0x2A]);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.unknown, result);
	}

	[TestMethod]
	public void Detect_NonTiffAndNoMarker_ReturnsUnknown()
	{
		var result = ExtensionRaw.Detect(new byte[400]);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.unknown, result);
	}

	[TestMethod]
	public void Detect_LittleEndianWithInvalidMagic_ReturnsUnknown()
	{
		var bytes = "II\0\0\0\0\0\0"u8.ToArray();
		var result = ExtensionRaw.Detect(bytes);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.unknown, result);
	}

	[TestMethod]
	public void Detect_BigEndianWithInvalidMagic_ReturnsUnknown()
	{
		var bytes = "MM\0\0\0\0\0\0"u8.ToArray();
		var result = ExtensionRaw.Detect(bytes);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.unknown, result);
	}

	[TestMethod]
	public void Detect_ValidTiffHeaderButLessThan8Bytes_ReturnsUnknown()
	{
		var bytes = new byte[] { 0x49, 0x49, 0x2A, 0x00 };
		var result = ExtensionRaw.Detect(bytes);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.unknown, result);
	}

	[TestMethod]
	public void Detect_PartialEndianPrefix_ReturnsUnknown()
	{
		var bytesLittlePartial = new byte[] { 0x49, 0x00, 0x2A, 0x00, 0, 0, 0, 0 };
		var bytesBigPartial = new byte[] { 0x4D, 0x00, 0x00, 0x2A, 0, 0, 0, 0 };

		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.unknown,
			ExtensionRaw.Detect(bytesLittlePartial));
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.unknown,
			ExtensionRaw.Detect(bytesBigPartial));
	}

	[TestMethod]
	public void Detect_BigEndianWithNonZeroThirdByte_ReturnsUnknown()
	{
		var bytes = new byte[] { 0x4D, 0x4D, 0x01, 0x2A, 0, 0, 0, 0 };
		var result = ExtensionRaw.Detect(bytes);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.unknown, result);
	}

	[TestMethod]
	public void Detect_X3fFovbHeader_ReturnsX3f()
	{
		var bytes = new byte[] { 0x46, 0x4F, 0x56, 0x62, 0x03, 0x00, 0x02, 0x00 };
		var result = ExtensionRaw.Detect(bytes);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.x3f, result);
	}

	[TestMethod]
	public void Detect_Rw2IiuHeader_ReturnsRw2()
	{
		var bytes = new byte[] { 0x49, 0x49, 0x55, 0x00, 0x18, 0x00, 0x00, 0x00 };
		var result = ExtensionRaw.Detect(bytes);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.rw2, result);
	}

	[TestMethod]
	public void Detect_Cr2NativeHeader_ReturnsCr2()
	{
		var bytes = new byte[]
		{
			0x49, 0x49, 0x2A, 0x00,
			0x10, 0x00, 0x00, 0x00,
			0x43, 0x52, 0x02, 0x00
		};
		var result = ExtensionRaw.Detect(bytes);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.cr2, result);
	}

	[TestMethod]
	public void Detect_FujiMarkerWithoutTiffHeader_ReturnsRaf()
	{
		var bytes = Encoding.ASCII.GetBytes("fuji");
		var result = ExtensionRaw.Detect(bytes);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.raf, result);
	}

	[TestMethod]
	public void Detect_FirstIfdOutsideProbe_FallsBackToMarker()
	{
		var bytes = CreateTiff(true, 800, 500);
		Encoding.ASCII.GetBytes("SONY", 0, 4, bytes, 32);

		var result = ExtensionRaw.Detect(bytes);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.arw, result);
	}

	[TestMethod]
	public void Detect_FirstIfdBeforeTiffMinimum_FallsBackToMarker()
	{
		var bytes = CreateTiff(true, 400, 4);
		Encoding.ASCII.GetBytes("SONY", 0, 4, bytes, 180);

		var result = ExtensionRaw.Detect(bytes);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.arw, result);
	}

	[TestMethod]
	public void Detect_DetectByMarkerWithShortProbe_UsesLengthGuard()
	{
		var bytes = new byte[] { 0x49, 0x49, 0x2A, 0x00, 4, 0, 0, 0 };
		var result = ExtensionRaw.Detect(bytes);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.unknown, result);
	}

	[TestMethod]
	public void Detect_DngTagInIfd_ReturnsDng()
	{
		var bytes = CreateTiff(true, 400, 8,
			( 50706, 1, 4, [1, 4, 0, 0], null ));

		var result = ExtensionRaw.Detect(bytes);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.dng, result);
	}

	[TestMethod]
	public void Detect_MakeSonyAsciiOffset_ReturnsArw()
	{
		var bytes = CreateTiff(true, 400, 8,
			( 0x010F, 2, 5, Encoding.ASCII.GetBytes("SONY\0"), null ));

		var result = ExtensionRaw.Detect(bytes);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.arw, result);
	}

	[TestMethod]
	public void Detect_MakeFujiInlineValue_ReturnsRaf()
	{
		var bytes = CreateTiff(true, 400, 8,
			( 0x010F, 2, 4, Encoding.ASCII.GetBytes("FUJI"), null ));

		var result = ExtensionRaw.Detect(bytes);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.raf, result);
	}

	[TestMethod]
	public void Detect_MakeNikonBigEndian_ReturnsNef()
	{
		var bytes = CreateTiff(false, 400, 8,
			( 0x010F, 2, 6, Encoding.ASCII.GetBytes("NIKON\0"), null ));

		var result = ExtensionRaw.Detect(bytes);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.nef, result);
	}

	[TestMethod]
	public void Detect_MakeOlympusByMarkerFallback_ReturnsOrf()
	{
		var bytes = CreateTiff(true, 400, 8,
			( 0x010F, 2, 2, [0, 0], null ));
		Encoding.ASCII.GetBytes("OLYMP", 0, 5, bytes, 120);

		var result = ExtensionRaw.Detect(bytes);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.orf, result);
	}

	[TestMethod]
	public void Detect_MakePentaxByMarkerFallback_ReturnsPef()
	{
		var bytes = CreateTiff(true, 400, 8,
			( 0x010F, 2, 2, [0, 0], null ));
		Encoding.ASCII.GetBytes("PENTAX", 0, 6, bytes, 140);

		var result = ExtensionRaw.Detect(bytes);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.pef, result);
	}

	[TestMethod]
	public void Detect_ModelCr3_ReturnsCr3()
	{
		var bytes = CreateTiff(true, 400, 8,
			( 0x0110, 2, 4, Encoding.ASCII.GetBytes("CR3\0"), null ));

		var result = ExtensionRaw.Detect(bytes);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.cr3, result);
	}

	[TestMethod]
	public void Detect_ModelCr2_ReturnsCr2()
	{
		var bytes = CreateTiff(true, 400, 8,
			( 0x0110, 2, 4, Encoding.ASCII.GetBytes("CR2\0"), null ));

		var result = ExtensionRaw.Detect(bytes);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.cr2, result);
	}

	[TestMethod]
	public void Detect_ModelX3f_ReturnsX3f()
	{
		var bytes = CreateTiff(true, 400, 8,
			( 0x0110, 2, 4, Encoding.ASCII.GetBytes("X3F\0"), null ));

		var result = ExtensionRaw.Detect(bytes);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.x3f, result);
	}

	[TestMethod]
	public void Detect_AsciiValueOutsideFirst400_ReturnsUnknown()
	{
		var bytes = CreateTiff(true, 1200, 8,
			( 0x010F, 2, 5, Encoding.ASCII.GetBytes("SONY\0"), 700 ));

		var result = ExtensionRaw.Detect(bytes);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.unknown, result);
	}

	[TestMethod]
	public void Detect_CountOverflowWhenCastToInt_ReturnsUnknown()
	{
		var bytes = CreateTiff(true, 400, 8,
			( 0x010F, 2, uint.MaxValue, Encoding.ASCII.GetBytes("SONY"), 64 ));

		var result = ExtensionRaw.Detect(bytes);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.unknown, result);
	}

	[TestMethod]
	public void Detect_MakeUnknown_ReturnsUnknown()
	{
		var bytes = CreateTiff(true, 400, 8,
			( 0x010F, 2, 5, Encoding.ASCII.GetBytes("ACME\0"), null ));

		var result = ExtensionRaw.Detect(bytes);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.unknown, result);
	}

	[TestMethod]
	public void Detect_ModelUnknown_ReturnsUnknown()
	{
		var bytes = CreateTiff(true, 400, 8,
			( 0x0110, 2, 5, Encoding.ASCII.GetBytes("ABCD\0"), null ));

		var result = ExtensionRaw.Detect(bytes);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.unknown, result);
	}

	[TestMethod]
	public void Detect_MakeTypeIsNotAscii_FallsBackToMarker()
	{
		var bytes = CreateTiff(true, 400, 8,
			( 0x010F, 3, 5, Encoding.ASCII.GetBytes("SONY\0"), null ));
		Encoding.ASCII.GetBytes("SONY", 0, 4, bytes, 180);

		var result = ExtensionRaw.Detect(bytes);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.arw, result);
	}

	[TestMethod]
	public void Detect_MakeAsciiCountZero_FallsBackToMarker()
	{
		var bytes = CreateTiff(true, 400, 8,
			( 0x010F, 2, 0, [], null ));
		Encoding.ASCII.GetBytes("NIKON", 0, 5, bytes, 180);

		var result = ExtensionRaw.Detect(bytes);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.nef, result);
	}

	[TestMethod]
	public void Detect_MakeAsciiNegativeOffset_FallsBackToMarker()
	{
		var bytes = CreateTiff(true, 400, 8,
			( 0x010F, 2, 5, Encoding.ASCII.GetBytes("SONY\0"), -16 ));
		Encoding.ASCII.GetBytes("PANASONIC", 0, 9, bytes, 180);

		var result = ExtensionRaw.Detect(bytes);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.rw2, result);
	}

	[TestMethod]
	public void Detect_UnrelatedTagOnly_FallsBackToMarker()
	{
		var bytes = CreateTiff(true, 400, 8,
			( 0x9999, 2, 4, Encoding.ASCII.GetBytes("TEST"), null ));
		Encoding.ASCII.GetBytes("DNG", 0, 3, bytes, 180);

		var result = ExtensionRaw.Detect(bytes);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.dng, result);
	}

	[TestMethod]
	public void Detect_MarkerScan_WithNonLetterBytes_DoesNotMatch()
	{
		var bytes = CreateTiff(true, 400, 8,
			( 0x9999, 2, 4, Encoding.ASCII.GetBytes("TEST"), null ));
		bytes[200] = ( byte ) '{';

		var result = ExtensionRaw.Detect(bytes);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.unknown, result);
	}

	[TestMethod]
	public void Detect_TruncatedEntryTable_FallsBackToMarker()
	{
		var bytes = CreateTiff(true, 400, 390);
		WriteUInt16(bytes, 390, 3, true);
		Encoding.ASCII.GetBytes("DNG", 0, 3, bytes, 200);

		var result = ExtensionRaw.Detect(bytes);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.dng, result);
	}

	private static byte[] CreateTiff(bool littleEndian, int totalLength, uint firstIfdOffset,
		params (ushort tag, ushort type, uint count, byte[] value, int? offsetOverride)[] entries)
	{
		var bytes = new byte[Math.Max(totalLength, 16)];
		if ( littleEndian )
		{
			bytes[0] = 0x49;
			bytes[1] = 0x49;
			bytes[2] = 0x2A;
			bytes[3] = 0x00;
		}
		else
		{
			bytes[0] = 0x4D;
			bytes[1] = 0x4D;
			bytes[2] = 0x00;
			bytes[3] = 0x2A;
		}

		WriteUInt32(bytes, 4, firstIfdOffset, littleEndian);
		if ( firstIfdOffset + 2 > bytes.Length )
		{
			return bytes;
		}

		var ifdOffset = ( int ) firstIfdOffset;
		WriteUInt16(bytes, ifdOffset, ( ushort ) entries.Length, littleEndian);
		var entriesOffset = ifdOffset + 2;
		var dataCursor = entriesOffset + entries.Length * 12 + 4;

		for ( var i = 0; i < entries.Length; i++ )
		{
			var entryPos = entriesOffset + i * 12;
			if ( entryPos + 12 > bytes.Length )
			{
				break;
			}

			var entry = entries[i];
			WriteUInt16(bytes, entryPos, entry.tag, littleEndian);
			WriteUInt16(bytes, entryPos + 2, entry.type, littleEndian);
			WriteUInt32(bytes, entryPos + 4, entry.count, littleEndian);

			if ( entry.count <= 4 && entry.offsetOverride == null )
			{
				var copyLength = Math.Min(( int ) entry.count, entry.value.Length);
				if ( copyLength > 0 )
				{
					Array.Copy(entry.value, 0, bytes, entryPos + 8, copyLength);
				}

				continue;
			}

			var valueOffset = entry.offsetOverride ?? dataCursor;
			WriteUInt32(bytes, entryPos + 8, ( uint ) valueOffset, littleEndian);
			if ( entry.offsetOverride != null || entry.count <= int.MaxValue )
			{
				if ( entry.offsetOverride == null && entry.count > 0 )
				{
					var count = ( int ) entry.count;
					var copyLength = Math.Min(count, entry.value.Length);
					if ( valueOffset >= 0 && valueOffset + copyLength <= bytes.Length )
					{
						Array.Copy(entry.value, 0, bytes, valueOffset, copyLength);
					}

					dataCursor = valueOffset + count;
				}
			}
		}

		return bytes;
	}

	private static void WriteUInt16(byte[] bytes, int offset, ushort value, bool littleEndian)
	{
		if ( littleEndian )
		{
			bytes[offset] = ( byte ) ( value & 0xFF );
			bytes[offset + 1] = ( byte ) ( value >> 8 );
			return;
		}

		bytes[offset] = ( byte ) ( value >> 8 );
		bytes[offset + 1] = ( byte ) ( value & 0xFF );
	}

	private static void WriteUInt32(byte[] bytes, int offset, uint value, bool littleEndian)
	{
		if ( littleEndian )
		{
			bytes[offset] = ( byte ) ( value & 0xFF );
			bytes[offset + 1] = ( byte ) ( ( value >> 8 ) & 0xFF );
			bytes[offset + 2] = ( byte ) ( ( value >> 16 ) & 0xFF );
			bytes[offset + 3] = ( byte ) ( ( value >> 24 ) & 0xFF );
			return;
		}

		bytes[offset] = ( byte ) ( ( value >> 24 ) & 0xFF );
		bytes[offset + 1] = ( byte ) ( ( value >> 16 ) & 0xFF );
		bytes[offset + 2] = ( byte ) ( ( value >> 8 ) & 0xFF );
		bytes[offset + 3] = ( byte ) ( value & 0xFF );
	}
}

