using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.TiffEmbeded;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.TiffEmbeded;

[TestClass]
public class TiffEmbeddedPreviewExtractorStreamPrimitivesReadTests
{
	[TestMethod]
	public void ReadUInt16_LittleEndian_ReturnsExpected()
	{
		var data = new byte[] { 0x34, 0x12 };
		var span = new ReadOnlySpan<byte>(data);
		var result = TiffEmbeddedPreviewExtractor.ReadUInt16(span, true);
		Assert.AreEqual(0x1234, result);
	}

	[TestMethod]
	public void ReadUInt16_BigEndian_ReturnsExpected()
	{
		var data = new byte[] { 0x12, 0x34 };
		var span = new ReadOnlySpan<byte>(data);
		var result = TiffEmbeddedPreviewExtractor.ReadUInt16(span, false);
		Assert.AreEqual(0x1234, result);
	}

	[TestMethod]
	public void ReadUInt32_LittleEndian_ReturnsExpected()
	{
		var data = new byte[] { 0x78, 0x56, 0x34, 0x12 };
		var span = new ReadOnlySpan<byte>(data);
		var result = TiffEmbeddedPreviewExtractor.ReadUInt32(span, true);
		Assert.AreEqual(0x12345678u, result);
	}

	[TestMethod]
	public void ReadUInt32_BigEndian_ReturnsExpected()
	{
		var data = new byte[] { 0x12, 0x34, 0x56, 0x78 };
		var span = new ReadOnlySpan<byte>(data);
		var result = TiffEmbeddedPreviewExtractor.ReadUInt32(span, false);
		Assert.AreEqual(0x12345678u, result);
	}

	[TestMethod]
	public void ReadScalarValue_Type3_LittleEndian_ReturnsLow16Bits()
	{
		// rawValue 0xAABBCCDD -> low 16 bits = 0xCCDD
		uint raw = 0xAABBCCDDu;
		var r = TiffEmbeddedPreviewExtractor.ReadScalarValue(3, raw, true);
		Assert.AreEqual(0xCCDDu, r);
	}

	[TestMethod]
	public void ReadScalarValue_Type3_BigEndian_ReturnsHigh16Bits()
	{
		// rawValue 0xAABBCCDD -> high 16 bits = 0xAABB
		uint raw = 0xAABBCCDDu;
		var r = TiffEmbeddedPreviewExtractor.ReadScalarValue(3, raw, false);
		Assert.AreEqual(0xAABBu, r);
	}

	[TestMethod]
	public void ReadScalarValue_Type4_ReturnsRaw()
	{
		uint raw = 0xDEADBEEFu;
		var r = TiffEmbeddedPreviewExtractor.ReadScalarValue(4, raw, true);
		Assert.AreEqual(raw, r);
	}

	[TestMethod]
	public void ReadScalarValue_UnknownType_ReturnsZero()
	{
		uint raw = 0x12345678u;
		var r = TiffEmbeddedPreviewExtractor.ReadScalarValue(99, raw, true);
		Assert.AreEqual(0u, r);
	}
}
