using System;
using System.IO;
using System.Reflection;
using System.Text;
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

	[TestMethod]
	public void TryReadAtomHeader_ShortHeader_ReturnsFalse()
	{
		var method = typeof(FindMdat).GetMethod("TryReadAtomHeader",
			BindingFlags.NonPublic | BindingFlags.Static);
		Assert.IsNotNull(method);

		using var ms = new MemoryStream(new byte[4]);
		var args = new object?[] { ms, ms.Length, 0L, null, 0L, 0, 0L };
		var res = ( bool ) method.Invoke(null, args)!;
		Assert.IsFalse(res);
	}

	[TestMethod]
	public void TryReadAtomHeader_ExtendedSize_ClampToLongMax()
	{
		var method = typeof(FindMdat).GetMethod("TryReadAtomHeader",
			BindingFlags.NonPublic | BindingFlags.Static);
		Assert.IsNotNull(method);

		// construct header: size32==1, type 'mdat', size64 = ulong.MaxValue
		using var ms = new MemoryStream();
		ms.Write([0, 0, 0, 1], 0, 4);
		ms.Write("mdat"u8.ToArray(), 0, 4);
		ms.Write([0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF], 0, 8);
		ms.Position = 0;

		var args = new object?[] { ms, ms.Length, 0L, null, 0L, 0, 0L };
		var res = ( bool ) method.Invoke(null, args)!;
		Assert.IsTrue(res);
		Assert.AreEqual("mdat", args[3] as string);
		Assert.AreEqual(16, ( int ) args[5]!);
		Assert.AreEqual(long.MaxValue, ( long ) args[4]!);
	}

	[TestMethod]
	public void TryReadAtomHeader_ExtendedSize_SmallSize_ReturnsSize()
	{
		var method = typeof(FindMdat).GetMethod("TryReadAtomHeader",
			BindingFlags.NonPublic | BindingFlags.Static);
		Assert.IsNotNull(method);

		using var ms = new MemoryStream();
		ms.Write(new byte[] { 0, 0, 0, 1 }, 0, 4);
		ms.Write(Encoding.ASCII.GetBytes("free"), 0, 4);
		// size64 = 20
		ms.Write(new byte[] { 0, 0, 0, 0, 0, 0, 0, 20 }, 0, 8);
		ms.Position = 0;

		var args = new object?[] { ms, ms.Length, 0L, null, 0L, 0, 0L };
		var res = ( bool ) method.Invoke(null, args)!;
		Assert.IsTrue(res);
		Assert.AreEqual("free", args[3] as string);
		Assert.AreEqual(16, ( int ) args[5]!);
		Assert.AreEqual(20L, ( long ) args[4]!);
	}

	[TestMethod]
	public void TryReadAtomHeader_TruncatedExtendedSize_ReturnsFalse()
	{
		var method = typeof(FindMdat).GetMethod("TryReadAtomHeader",
			BindingFlags.NonPublic | BindingFlags.Static);
		Assert.IsNotNull(method);

		using var ms = new MemoryStream();
		ms.Write(new byte[] { 0, 0, 0, 1 }, 0, 4);
		ms.Write(Encoding.ASCII.GetBytes("mdat"), 0, 4);
		// only 4 bytes for extended size (need 8)
		ms.Write(new byte[] { 1, 2, 3, 4 }, 0, 4);
		ms.Position = 0;

		var args = new object?[] { ms, ms.Length, 0L, null, 0L, 0, 0L };
		var res = ( bool ) method.Invoke(null, args)!;
		Assert.IsFalse(res);
	}

	[TestMethod]
	public void HashMdatPayload_NullPayloadLen_UsesMaxBytes()
	{
		var path = TempFile();
		try
		{
			File.WriteAllBytes(path, new byte[] { 1, 2, 3, 4, 5 });
			var fi = new FileInfo(path);
			var (md5Hex, b32) = FindMdat.HashMdatPayload(fi, 0, null, 3);
			Assert.IsFalse(string.IsNullOrEmpty(md5Hex));
			Assert.IsFalse(string.IsNullOrEmpty(b32));
		}
		finally
		{
			File.Delete(path);
		}
	}

	[TestMethod]
	public void FindFirstMdat_InvalidAtomSize_NegativeToSkip_ReturnsNull()
	{
		// create a file with one atom whose size is smaller than header (size=4)
		var path = TempFile();
		try
		{
			using var ms = new MemoryStream();
			ms.Write([0, 0, 0, 4], 0, 4);
			ms.Write("free"u8.ToArray(), 0, 4);
			File.WriteAllBytes(path, ms.ToArray());

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
	public void HashMdatPayload_ReadBeyondEOF_BreaksLoop()
	{
		var path = TempFile();
		try
		{
			File.WriteAllBytes(path, [1, 2, 3]);
			var fi = new FileInfo(path);
			// request dataOffset beyond EOF
			var (md5Hex, b32) = FindMdat.HashMdatPayload(fi, 1000, 10, 10);
			Assert.IsFalse(string.IsNullOrEmpty(md5Hex));
			Assert.IsFalse(string.IsNullOrEmpty(b32));
		}
		finally
		{
			File.Delete(path);
		}
	}

	[TestMethod]
	public void Base32NoPadding_EmptyArray_ReturnsEmpty()
	{
		var method = typeof(FindMdat).GetMethod("Base32NoPadding",
			BindingFlags.NonPublic | BindingFlags.Static);
		Assert.IsNotNull(method);
		var res = ( string ) method.Invoke(null, [Array.Empty<byte>()])!;
		Assert.AreEqual(string.Empty, res);
	}

	[TestMethod]
	public void FindFirstMdat_StreamOverloads_Handle_NonSeekable_And_ZeroSize()
	{
		// 1) Extended size == 0 -> atomSize == 0 -> FindFirstMdat should return null
		using var ms1 = new MemoryStream();
		ms1.Write([0, 0, 0, 1], 0, 4); // size32 == 1
		ms1.Write("free"u8.ToArray(), 0, 4);
		ms1.Write(new byte[8], 0, 8); // extended size == 0
		ms1.Position = 0;
		var res1 = FindMdat.FindFirstMdat(ms1, ms1.Length);
		Assert.IsNull(res1);

		// 2) Non-seekable stream: TrySkip fallback should return false and FindFirstMdat returns null
		var ms2 = new MemoryStream();
		// first atom size=12 (header 8 + 4 payload) but we won't include payload -> toSkip=4 but not enough data
		ms2.Write([0, 0, 0, 12], 0, 4);
		ms2.Write("free"u8.ToArray(), 0, 4);
		// no payload
		ms2.Position = 0;
		using var s = new ThrowOnSeekStream(ms2.ToArray());
		var res2 = FindMdat.FindFirstMdat(s, s.Length);
		Assert.IsNull(res2);
	}

	[TestMethod]
	public void TryReadAtomHeader_TruncatedExtendedSize_ReturnsFalse_NewFile()
	{
		var method = typeof(FindMdat).GetMethod("TryReadAtomHeader",
			BindingFlags.NonPublic | BindingFlags.Static);
		Assert.IsNotNull(method);

		using var ms = new MemoryStream();
		ms.Write(new byte[] { 0, 0, 0, 1 }, 0, 4);
		ms.Write(Encoding.ASCII.GetBytes("mdat"), 0, 4);
		// only 4 bytes for extended size (need 8)
		ms.Write(new byte[] { 1, 2, 3, 4 }, 0, 4);
		ms.Position = 0;

		var args = new object?[] { ms, ms.Length, 0L, null, 0L, 0, 0L };
		var res = ( bool ) method.Invoke(null, args)!;
		Assert.IsFalse(res);
	}

	[TestMethod]
	public void HashMdatPayload_ForceNullHashFlag_UsesEmptyDigest()
	{
		var oldFlag = FindMdat.ForceNullHashForTests;
		try
		{
			FindMdat.ForceNullHashForTests = true;
			var path = TempFile();
			try
			{
				File.WriteAllBytes(path, new byte[] { 1, 2, 3 });
				var fi = new FileInfo(path);
				var (md5Hex, b32) = FindMdat.HashMdatPayload(fi, 0, null, 10);
				// When Hash is forced null, production code should fall back to empty arrays -> empty strings
				Assert.AreEqual(string.Empty, md5Hex);
				Assert.AreEqual(string.Empty, b32);
			}
			finally
			{
				File.Delete(path);
			}
		}
		finally
		{
			FindMdat.ForceNullHashForTests = oldFlag;
		}
	}

	[TestMethod]
	public void FindMdatProgram_Main_ExtendedMdat_PayloadLenNull_UsesHashBytes_NewFile()
	{
		var path = TempFile();
		try
		{
			// size32 = 1, type = "mdat", extended size = 4 (< headerSize 16) -> PayloadLen null
			using var ms = new MemoryStream();
			ms.Write(new byte[] { 0, 0, 0, 1 }, 0, 4);
			ms.Write(Encoding.ASCII.GetBytes("mdat"), 0, 4);
			ms.Write(new byte[] { 0, 0, 0, 0, 0, 0, 0, 4 }, 0, 8);
			File.WriteAllBytes(path, ms.ToArray());

			// With --hash-bytes specified, Main should use hashBytes when PayloadLen is null
			var exit = FindMdatProgram.Main(new[] { path, "--hash-bytes", "2" });
			Assert.AreEqual(0, exit);
		}
		finally
		{
			File.Delete(path);
		}
	}

	private sealed class ThrowOnSeekStream(byte[] data) : MemoryStream(data)
	{
		public override long Seek(long offset, SeekOrigin loc)
		{
			throw new IOException("seek not supported");
		}
	}
}
