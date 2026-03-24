using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.TiffEmbeded;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.
	TiffEmbeded;

[TestClass]
public class StreamPrimitivesTests
{
	[TestMethod]
	public void TrySeek_Seekable_ReturnsTrueAndSeeks()
	{
		using var ms = new MemoryStream(new byte[100]);
		var ok = StreamPrimitives.TrySeek(ms, 50);
		Assert.IsTrue(ok);
		Assert.AreEqual(50, ms.Position);
	}

	[TestMethod]
	public void TrySeek_NonSeekable_ReturnsFalseAndPositionUnchanged()
	{
		var data = new byte[10];
		using var ns = new NonSeekableMemoryStream(data);
		var ok = StreamPrimitives.TrySeek(ns, 5);
		Assert.IsFalse(ok);
		Assert.AreEqual(0, ns.Position);
	}

	[TestMethod]
	public void TryGetRemainingBytes_Seekable_ReturnsRemaining()
	{
		using var ms = new MemoryStream(new byte[100]);
		ms.Position = 10;
		var got = StreamPrimitives.TryGetRemainingBytes(ms, out var remaining);
		Assert.IsTrue(got);
		Assert.AreEqual(90, remaining);
	}

	[TestMethod]
	public void TryGetRemainingBytes_NonSeekable_ReturnsFalse()
	{
		using var ns = new NonSeekableMemoryStream(new byte[50]);
		var got = StreamPrimitives.TryGetRemainingBytes(ns, out var remaining);
		Assert.IsFalse(got);
		Assert.AreEqual(0, remaining);
	}

	[TestMethod]
	public void ClampIndirectCount_ReturnsZero_WhenRequestedZero()
	{
		using var ms = new MemoryStream(new byte[100]);
		var r = StreamPrimitives.ClampIndirectCount(ms, 0, 4, 0, 10);
		Assert.AreEqual(0u, r);
	}

	[TestMethod]
	public void ClampIndirectCount_OffsetBeyondLength_ReturnsZero()
	{
		using var ms = new MemoryStream(new byte[10]);
		var r = StreamPrimitives.ClampIndirectCount(ms, 100u, 4, 10, 100);
		Assert.AreEqual(0u, r);
	}

	[TestMethod]
	public void ClampIndirectCount_BoundedByHardCapAndFile()
	{
		using var ms = new MemoryStream(new byte[100]); // available 100
		// type 3 -> 2 bytes per value
		var r = StreamPrimitives.ClampIndirectCount(ms, 0u, 3, 100u, 10u);
		Assert.AreEqual(10u, r); // bounded by hardCap
	}

	[TestMethod]
	public void ClampIndirectCount_FileLimited()
	{
		using var ms = new MemoryStream(new byte[20]);
		// offset 10 => available 10 bytes, type !=3 => 4 bytes/value => maxFromFile = 2
		var r = StreamPrimitives.ClampIndirectCount(ms, 10u, 4, 10u, 100u);
		Assert.AreEqual(2u, r);
	}

	[TestMethod]
	public void ClampIndirectCount_LengthThrows_ReturnsBounded()
	{
		using var ts = new ThrowingLengthStream(new byte[10]);
		var r = StreamPrimitives.ClampIndirectCount(ts, 0u, 4, 5u, 3u);
		// should return bounded (min(requested, hardCap) == 3) since Length threw
		Assert.AreEqual(3u, r);
	}

	[TestMethod]
	public void ReadIndirectOffsets_UInt32_LittleEndian_WritesOffsets()
	{
		var values = new[] { 0x11223344u, 0xAABBCCDDu };
		var buf = new byte[5 + values.Length * 4];
		// offset padding 5
		var offset = 5;
		for ( var i = 0; i < values.Length; i++ )
		{
			var v = values[i];
			var pos = offset + i * 4;
			buf[pos + 0] = ( byte ) ( v & 0xFF );
			buf[pos + 1] = ( byte ) ( ( v >> 8 ) & 0xFF );
			buf[pos + 2] = ( byte ) ( ( v >> 16 ) & 0xFF );
			buf[pos + 3] = ( byte ) ( ( v >> 24 ) & 0xFF );
		}

		using var ms = new MemoryStream(buf);
		var list = new List<uint>();
		StreamPrimitives.ReadIndirectOffsets(ms, ( uint ) offset, 4, ( uint ) values.Length, true,
			list);
		CollectionAssert.AreEqual(new List<uint>(values), list);
	}

	[TestMethod]
	public void ReadIndirectOffsets_UInt32_BigEndian_WritesOffsets()
	{
		var values = new[] { 0x11223344u, 0xAABBCCDDu };
		var buf = new byte[3 + values.Length * 4];
		var offset = 3;
		for ( var i = 0; i < values.Length; i++ )
		{
			var v = values[i];
			var pos = offset + i * 4;
			buf[pos + 0] = ( byte ) ( ( v >> 24 ) & 0xFF );
			buf[pos + 1] = ( byte ) ( ( v >> 16 ) & 0xFF );
			buf[pos + 2] = ( byte ) ( ( v >> 8 ) & 0xFF );
			buf[pos + 3] = ( byte ) ( v & 0xFF );
		}

		using var ms = new MemoryStream(buf);
		var list = new List<uint>();
		StreamPrimitives.ReadIndirectOffsets(ms, ( uint ) offset, 4, ( uint ) values.Length, false,
			list);
		CollectionAssert.AreEqual(new List<uint>(values), list);
	}

	[TestMethod]
	public void ReadIndirectOffsets_UInt16_LittleEndian_WritesOffsets()
	{
		var values = new ushort[] { 0x1122, 0x3344 };
		var buf = new byte[2 + values.Length * 2];
		var offset = 2;
		for ( var i = 0; i < values.Length; i++ )
		{
			var v = values[i];
			var pos = offset + i * 2;
			buf[pos + 0] = ( byte ) ( v & 0xFF );
			buf[pos + 1] = ( byte ) ( ( v >> 8 ) & 0xFF );
		}

		using var ms = new MemoryStream(buf);
		var list = new List<uint>();
		StreamPrimitives.ReadIndirectOffsets(ms, ( uint ) offset, 3, ( uint ) values.Length, true,
			list);
		Assert.AreEqual(values[0], list[0]);
		Assert.AreEqual(values[1], list[1]);
	}

	[TestMethod]
	public void ReadIndirectOffsets_CountZero_DoesNothing()
	{
		var buf = new byte[20];
		using var ms = new MemoryStream(buf);
		var list = new List<uint> { 99 };
		StreamPrimitives.ReadIndirectOffsets(ms, 0u, 4, 0u, true, list);
		// unchanged
		Assert.HasCount(1, list);
		Assert.AreEqual(99u, list[0]);
	}

	[TestMethod]
	public void ReadIndirectOffsets_InsufficientBytes_DoesNotAdd()
	{
		var buf = new byte[10];
		// offset near end so not enough bytes for requested
		using var ms = new MemoryStream(buf);
		var list = new List<uint>();
		// request 4 values (needs 16 bytes) but available less
		StreamPrimitives.ReadIndirectOffsets(ms, 2u, 4, 4u, true, list);
		Assert.IsEmpty(list);
	}

	[TestMethod]
	public void ReadIndirectOffsets_NonSeekable_DoesNothing()
	{
		var buf = new byte[100];
		using var ns = new NonSeekableMemoryStream(buf);
		var list = new List<uint>();
		StreamPrimitives.ReadIndirectOffsets(ns, 10u, 4, 2u, true, list);
		Assert.AreEqual(0, list.Count);
	}

	private sealed class NonSeekableMemoryStream(byte[] buffer) : MemoryStream(buffer)
	{
		public override bool CanSeek => false;

		public override long Seek(long offset, SeekOrigin loc)
		{
			throw new NotSupportedException();
		}
	}

	private sealed class ThrowingLengthStream(byte[] buf) : MemoryStream(buf)
	{
		public override long Length => throw new InvalidOperationException("Length not available");
	}
}
