using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace starsky.foundation.storage.Services;

public sealed class MdatInfo
{
	public long HeaderOffset { get; set; }
	public string AtomType { get; set; } = string.Empty;
	public long? AtomSize { get; set; }
	public int HeaderSize { get; set; }
	public long DataOffset { get; set; }
	public long? PayloadLen { get; set; }
}

public static class FindMdat
{
	// Find the first 'mdat' atom in an MP4 file and return info similar to the python script.
	// Refactored to reduce cognitive complexity by extracting helpers for reading headers and skipping.
	public static MdatInfo? FindFirstMdat(FileInfo file)
	{
		using var fs = file.OpenRead();
		var fileLen = fs.Length;

		while ( true )
		{
			var posHeader = fs.Position;

			if ( !TryReadAtomHeader(fs, fileLen, posHeader, out var atomType, out var atomSize,
				    out var headerSize, out var dataOffset) )
			{
				return null;
			}

			long? payloadLen = atomSize >= headerSize ? atomSize - headerSize : null;

			if ( atomType == "mdat" )
			{
				return new MdatInfo
				{
					HeaderOffset = posHeader,
					AtomType = atomType,
					AtomSize = atomSize,
					HeaderSize = headerSize,
					DataOffset = dataOffset,
					PayloadLen = payloadLen
				};
			}

			// skip payload to next atom
			if ( atomSize <= 0 )
			{
				// can't determine next atom, abort
				return null;
			}

			var toSkip = atomSize - headerSize;
			if ( toSkip < 0 )
			{
				// Invalid atom size; abort
				return null;
			}

			if ( !TrySkip(fs, toSkip) )
			{
				return null;
			}
		}
	}

	// Try to read an atom header at the current stream position.
	// Returns false on unexpected EOF or read error.
	private static bool TryReadAtomHeader(Stream fs, long fileLen, long posHeader,
		out string atomType, out long atomSize, out int headerSize, out long dataOffset)
	{
		atomType = string.Empty;
		atomSize = 0;
		headerSize = 0;
		dataOffset = 0;

		var header = new byte[8];
		var headerRead = fs.Read(header, 0, header.Length);
		if ( headerRead < 8 )
		{
			// EOF reached while scanning; no atom header found
			return false;
		}

		var size32 = ReadUInt32BigEndian(header);
		atomType = Encoding.ASCII.GetString(header, 4, 4);

		if ( size32 == 0 )
		{
			// Atom extends to EOF if seekable
			atomSize = fileLen - posHeader;
			headerSize = 8;
			dataOffset = fs.Position;
			return true;
		}

		if ( size32 == 1 )
		{
			// extended size in next 8 bytes
			var large = new byte[8];
			var r = fs.Read(large, 0, large.Length);
			if ( r < 8 )
			{
				// Unexpected EOF reading extended size
				return false;
			}

			var size64 = ReadUInt64BigEndian(large);
			// clamp to long.MaxValue if the 64-bit size would overflow signed long
			const ulong longMaxAsUlong = 9223372036854775807UL;
			if ( size64 > longMaxAsUlong )
			{
				atomSize = long.MaxValue;
			}
			else
			{
				atomSize = ( long ) size64;
			}

			headerSize = 16;
			// After reading extended size, data starts at current position
			dataOffset = fs.Position;
			return true;
		}

		atomSize = size32;
		headerSize = 8;
		dataOffset = fs.Position;
		return true;
	}

	private static bool TrySkip(Stream fs, long toSkip)
	{
		try
		{
			fs.Seek(toSkip, SeekOrigin.Current);
			return true;
		}
		catch ( IOException )
		{
			// fallback read/skip
			var remaining = toSkip;
			var buffer = new byte[65536];
			while ( remaining > 0 )
			{
				var read = fs.Read(buffer, 0, ( int ) Math.Min(buffer.Length, remaining));
				if ( read <= 0 )
				{
					return false;
				}

				remaining -= read;
			}

			return true;
		}
	}

	public static (string md5Hex, string base32) HashMdatPayload(FileInfo file, long dataOffset,
		long? payloadLen, int maxBytes)
	{
		var toRead = payloadLen.HasValue ? Math.Min(payloadLen.Value, maxBytes) : maxBytes;
		using var md5 = MD5.Create();
		using var fs = file.OpenRead();
		fs.Seek(dataOffset, SeekOrigin.Begin);
		var buffer = new byte[65536];
		var remaining = toRead;
		while ( remaining > 0 )
		{
			var r = fs.Read(buffer, 0, ( int ) Math.Min(buffer.Length, remaining));
			if ( r <= 0 )
			{
				break;
			}

			md5.TransformBlock(buffer, 0, r, null, 0);
			remaining -= r;
		}

		md5.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
		var digest = md5.Hash ?? Array.Empty<byte>();
		return ( BitConverter.ToString(digest).Replace("-", string.Empty).ToLowerInvariant(),
			Base32NoPadding(digest) );
	}

	private static uint ReadUInt32BigEndian(Span<byte> b)
	{
		return ( ( uint ) b[0] << 24 ) | ( ( uint ) b[1] << 16 ) | ( ( uint ) b[2] << 8 ) | b[3];
	}

	private static ulong ReadUInt64BigEndian(Span<byte> b)
	{
		return ( ( ulong ) b[0] << 56 ) | ( ( ulong ) b[1] << 48 ) | ( ( ulong ) b[2] << 40 ) |
		       ( ( ulong ) b[3] << 32 ) |
		       ( ( ulong ) b[4] << 24 ) | ( ( ulong ) b[5] << 16 ) | ( ( ulong ) b[6] << 8 ) | b[7];
	}

	private static string Base32NoPadding(byte[] data)
	{
		const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
		if ( data.Length == 0 )
		{
			return string.Empty;
		}

		var outputLength = ( data.Length * 8 + 4 ) / 5; // ceil(bits/5)
		var result = new StringBuilder(outputLength);
		int buffer = data[0];
		var next = 1;
		var bitsLeft = 8;

		while ( bitsLeft > 0 || next < data.Length )
		{
			if ( bitsLeft < 5 )
			{
				if ( next < data.Length )
				{
					buffer <<= 8;
					buffer |= data[next++] & 0xff;
					bitsLeft += 8;
				}
				else
				{
					var pad = 5 - bitsLeft;
					buffer <<= pad;
					bitsLeft += pad;
				}
			}

			var index = ( buffer >> ( bitsLeft - 5 ) ) & 0x1f;
			bitsLeft -= 5;
			result.Append(alphabet[index]);
		}

		return result.ToString();
	}
}

// Optional console entry similar to the python script usage.
// This class is safe to include inside the library; if compiled as a console app the Main will be used.
public static class FindMdatProgram
{
	public static int Main(string[] args)
	{
		if ( args.Length == 0 )
		{
			Console.WriteLine("Usage: find_mdat <file> [--hash-bytes N]");
			return 2;
		}

		var filePath = args[0];
		var hashBytes = 0;
		for ( var i = 1; i < args.Length; i++ )
		{
			if ( args[i] == "--hash-bytes" && i + 1 < args.Length )
			{
				int.TryParse(args[i + 1], out hashBytes);
			}
		}

		var fi = new FileInfo(filePath);
		if ( !fi.Exists )
		{
			Console.WriteLine("File not found: " + filePath);
			return 2;
		}

		var info = FindMdat.FindFirstMdat(fi);
		if ( info == null )
		{
			Console.WriteLine("No mdat found.");
			return 1;
		}

		Console.WriteLine("Found first mdat:");
		Console.WriteLine($"  header_offset: {info.HeaderOffset}");
		Console.WriteLine($"  atom_type: {info.AtomType}");
		Console.WriteLine($"  atom_size: {info.AtomSize}");
		Console.WriteLine($"  header_size: {info.HeaderSize}");
		Console.WriteLine($"  data_offset: {info.DataOffset}");
		Console.WriteLine($"  payload_len: {info.PayloadLen}");

		if ( hashBytes <= 0 )
		{
			return 0;
		}

		var payloadLen = info.PayloadLen ?? hashBytes;
		var (md5Hex, b32) =
			FindMdat.HashMdatPayload(fi, info.DataOffset, payloadLen, hashBytes);
		Console.WriteLine(
			$"\nMD5 (hex) of first {Math.Min(payloadLen, hashBytes)} payload bytes: {md5Hex}");
		Console.WriteLine($"Base32: {b32}");

		return 0;
	}
}
