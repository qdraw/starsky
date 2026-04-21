using System;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.storage.Services;

namespace starskytest.FindMdatTests;

[TestClass]
public class FindMdatTests
{
	private static string TempFile(string prefix = "findmdat")
	{
		var file = Path.Combine(Path.GetTempPath(), prefix + Guid.NewGuid().ToString("N") + ".bin");
		return file;
	}

	[TestMethod]
	public void FindFirstMdat_ReturnsNull_ForShortFile()
	{
		var path = TempFile();
		try
		{
			File.WriteAllBytes(path, [1, 2, 3, 4]);
			var fi = new FileInfo(path);
			var info = FindMdat.FindFirstMdat(fi);
			Assert.IsNull(info);
		}
		finally
		{
			File.Delete(path);
		}
	}

	[TestMethod]
	public void FindFirstMdat_SizeZero_Mdat()
	{
		var path = TempFile();
		try
		{
			// size32 = 0, type = "mdat", payload = 6 bytes
			var payload = new byte[] { 1, 2, 3, 4, 5, 6 };
			using var ms = new MemoryStream();
			ms.Write(new byte[4], 0, 4); // size=0
			ms.Write("mdat"u8.ToArray(), 0, 4);
			ms.Write(payload, 0, payload.Length);
			File.WriteAllBytes(path, ms.ToArray());

			var fi = new FileInfo(path);
			var info = FindMdat.FindFirstMdat(fi);
			Assert.IsNotNull(info);
			Assert.AreEqual("mdat", info.AtomType);
			Assert.AreEqual(8, info.HeaderSize);
			Assert.AreEqual(8, info.DataOffset);
			Assert.AreEqual(fi.Length - 8, info.PayloadLen);

			var (md5Hex, b32) = FindMdat.HashMdatPayload(fi, info.DataOffset, info.PayloadLen, 4);
			Assert.IsFalse(string.IsNullOrEmpty(md5Hex));
			Assert.IsFalse(string.IsNullOrEmpty(b32));
		}
		finally
		{
			File.Delete(path);
		}
	}

	[TestMethod]
	public void FindFirstMdat_ExtendedSize_Mdat()
	{
		var path = TempFile();
		try
		{
			// size32 = 1, type = "mdat", size64 = 20 (header 16 -> payload 4)
			var payload = new byte[] { 9, 8, 7, 6 };
			using var ms = new MemoryStream();
			ms.Write([0, 0, 0, 1], 0, 4); // size32 == 1
			ms.Write("mdat"u8.ToArray(), 0, 4);
			// size64 big endian 20
			ms.Write([0, 0, 0, 0, 0, 0, 0, 20], 0, 8);
			ms.Write(payload, 0, payload.Length);
			File.WriteAllBytes(path, ms.ToArray());

			var fi = new FileInfo(path);
			var info = FindMdat.FindFirstMdat(fi);
			Assert.IsNotNull(info);
			Assert.AreEqual("mdat", info.AtomType);
			Assert.AreEqual(16, info.HeaderSize);
			Assert.AreEqual(16, info.DataOffset);
			Assert.AreEqual(4, info.PayloadLen);
		}
		finally
		{
			File.Delete(path);
		}
	}

	[TestMethod]
	public void FindFirstMdat_Skips_Atoms()
	{
		var path = TempFile();
		try
		{
			// first atom: free, size=12 (8 header + 4 payload)
			// second atom: mdat, size=12, payload 4
			using var ms = new MemoryStream();
			ms.Write([0, 0, 0, 12], 0, 4);
			ms.Write("free"u8.ToArray(), 0, 4);
			ms.Write([1, 2, 3, 4], 0, 4);

			ms.Write([0, 0, 0, 12], 0, 4);
			ms.Write("mdat"u8.ToArray(), 0, 4);
			ms.Write([5, 6, 7, 8], 0, 4);

			File.WriteAllBytes(path, ms.ToArray());
			var fi = new FileInfo(path);
			var info = FindMdat.FindFirstMdat(fi);
			Assert.IsNotNull(info);
			Assert.AreEqual("mdat", info.AtomType);
			Assert.AreEqual(8, info.HeaderSize);
			// first atom size = 12, header of second = 8 -> data offset should be 12 + 8 = 20
			Assert.AreEqual(12 + 8, info.DataOffset);
		}
		finally
		{
			File.Delete(path);
		}
	}

	[TestMethod]
	public void TrySkip_Fallback_Read_Behaves()
	{
		var privateMethod =
			typeof(FindMdat).GetMethod("TrySkip", BindingFlags.NonPublic | BindingFlags.Static);
		Assert.IsNotNull(privateMethod);

		// Enough data to skip
		var data = new byte[64];
		for ( var i = 0; i < data.Length; i++ )
		{
			data[i] = ( byte ) i;
		}

		using var s = new ThrowOnSeekStream(data);
		var invoked = privateMethod.Invoke(null, [s, 32L]);
		Assert.IsNotNull(invoked);
		var result = ( bool ) invoked;
		Assert.IsTrue(result);

		// Not enough data to skip -> returns false
		using var s2 = new ThrowOnSeekStream(new byte[8]);
		var invoked2 = privateMethod.Invoke(null, [s2, 32L]);
		Assert.IsNotNull(invoked2);
		var result2 = ( bool ) invoked2;
		Assert.IsFalse(result2);
	}

	[TestMethod]
	public void FindMdatProgram_Main_VariousPaths()
	{
		// no args
		Assert.AreEqual(2, FindMdatProgram.Main([]));

		// non-existent file
		Assert.AreEqual(2, FindMdatProgram.Main(["non-existent-file.xyz"]));

		// file with no mdat
		var path = TempFile();
		try
		{
			File.WriteAllBytes(path, new byte[32]);
			Assert.AreEqual(1, FindMdatProgram.Main([path]));

			// with mdat and no hash
			using var ms = new MemoryStream();
			ms.Write(new byte[4], 0, 4); // size=0
			ms.Write("mdat"u8.ToArray(), 0, 4);
			ms.Write([1, 2, 3, 4], 0, 4);
			File.WriteAllBytes(path, ms.ToArray());
			Assert.AreEqual(0, FindMdatProgram.Main([path]));

			// with mdat and hash-bytes
			Assert.AreEqual(0, FindMdatProgram.Main([path, "--hash-bytes", "2"]));
		}
		finally
		{
			File.Delete(path);
		}
	}

	private class ThrowOnSeekStream : MemoryStream
	{
		public ThrowOnSeekStream(byte[] data) : base(data)
		{
		}

		public override long Seek(long offset, SeekOrigin loc)
		{
			throw new IOException("seek not supported");
		}
	}
}
