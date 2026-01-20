using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Interfaces;

namespace starsky.foundation.storage.Services;

/// <summary>
///     Specialized hasher for MP4 video files
///     Optimizes performance by hashing only the mdat atom (media data) instead of the entire file
/// </summary>
[SuppressMessage("Usage",
	"S4790:Make sure this weak hash algorithm is not used in a sensitive context here.",
	Justification = "Not used for passwords")]
public sealed class Mp4FileHasher(IStorage iStorage, IWebLogger logger)
{
	/// <summary>
	///     Represents an MP4 atom header
	/// </summary>
	private struct Mp4Atom
	{
		public long Size;
		public string Type;
		public long DataOffset;
	}

	/// <summary>
	///     Maximum bytes of video content to hash (256 KB)
	///     Balances between collision resistance and performance
	/// </summary>
	private const int MaxBytesToHash = 256 * 1024;

	/// <summary>
	///     Buffer size for reading atom data
	/// </summary>
	private const int BufferSize = 8192;

	/// <summary>
	///     Hash MP4 video content by reading only the mdat atom
	///     This is significantly faster than hashing the entire file for large video files
	/// </summary>
	/// <param name="fullFilePath">Path to the MP4 file</param>
	/// <returns>Base32 encoded MD5 hash of video content, or empty string if no mdat atom found</returns>
	public async Task<string> HashMp4VideoContentAsync(string fullFilePath)
	{
		using var md5 = System.Security.Cryptography.MD5.Create();
		var buffer = ArrayPool<byte>.Shared.Rent(BufferSize);
		
		try
		{
			await using var stream = iStorage.ReadStream(fullFilePath, FileHash.MaxReadSize);
			return await ProcessMp4AtomsAsync(stream, md5, buffer);
		}
		catch ( Exception e )
		{
			logger.LogError($"Mp4FileHasher.HashMp4VideoContentAsync Error: {e.Message}");
			return string.Empty;
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(buffer);
		}
	}

	/// <summary>
	///     Processes MP4 atoms and finds/hashes the mdat atom
	/// </summary>
	private async Task<string> ProcessMp4AtomsAsync(Stream stream,
		System.Security.Cryptography.MD5 md5, byte[] buffer)
	{
		if ( stream == Stream.Null )
		{
			return string.Empty;
		}
		
		while ( true )
		{
			var atom = await ReadAtomAsync(stream);
			if ( atom == null )
			{
				break;
			}

			logger.LogDebug(
				$"Found atom: Type={atom.Value.Type}, " +
				$"Size={atom.Value.Size}, DataOffset={atom.Value.DataOffset}");

			var payloadSize = atom.Value.Size - 8;

			if ( atom.Value.Type == "mdat" )
			{
				return await HashMdatAtomAsync(stream, md5, buffer, payloadSize);
			}

			if ( !await SkipAtomAsync(stream, buffer, payloadSize) )
			{
				return string.Empty;
			}
		}

		// No mdat atom found, return empty to fall back to standard hashing
		return string.Empty;
	}

	/// <summary>
	///     Hashes the mdat atom content
	/// </summary>
	private static async Task<string> HashMdatAtomAsync(Stream stream,
		System.Security.Cryptography.MD5 md5, byte[] buffer, long payloadSize)
	{
		var remaining = Math.Min(payloadSize, MaxBytesToHash);
		while ( remaining > 0 )
		{
			var toRead = ( int ) Math.Min(buffer.Length, remaining);
			var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, toRead));

			if ( bytesRead <= 0 )
			{
				break;
			}

			md5.TransformBlock(buffer, 0, bytesRead, null, 0);
			remaining -= bytesRead;
		}

		md5.TransformFinalBlock([], 0, 0);
		var hash = md5.Hash;
		return Base32.Encode(hash!);
	}

	/// <summary>
	///     Skips a non-mdat atom by seeking or reading past its content
	/// </summary>
	/// <returns>True if skip was successful, false if an error occurred</returns>
	private static async Task<bool> SkipAtomAsync(Stream stream, byte[] buffer, long payloadSize)
	{
		try
		{
			if ( payloadSize > 0 && stream.CanSeek )
			{
				stream.Seek(payloadSize, SeekOrigin.Current);
			}
			else
			{
				await SkipByReadingAsync(stream, buffer, payloadSize);
			}

			return true;
		}
		catch ( IOException )
		{
			// If seek/read fails, return empty to fall back to standard hashing
			return false;
		}
	}

	/// <summary>
	///     Skips data by reading and discarding bytes
	/// </summary>
	private static async Task SkipByReadingAsync(Stream stream, byte[] buffer, long toSkip)
	{
		while ( toSkip > 0 )
		{
			var toRead = ( int ) Math.Min(buffer.Length, toSkip);
			var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, toRead));

			if ( bytesRead <= 0 )
			{
				break;
			}

			toSkip -= bytesRead;
		}
	}

	/// <summary>
	///     Reads a single MP4 atom from a stream
	///     MP4 atoms have an 8-byte header: 4-byte size (big-endian) + 4-byte type (ASCII)
	///     If size field equals 1, the actual size is in the following 8 bytes (extended size)
	/// </summary>
	/// <param name="stream">Stream to read from</param>
	/// <param name="ct">Cancellation token</param>
	/// <returns>MP4 atom or null if stream is exhausted</returns>
	private static async Task<Mp4Atom?> ReadAtomAsync(
		Stream stream,
		CancellationToken ct = default)
	{
		var header = new byte[8];

		var read = await stream.ReadAsync(header.AsMemory(0, 8), ct);
		if ( read < 8 )
		{
			return null;
		}

		var size = BinaryPrimitives.ReadUInt32BigEndian(header.AsSpan(0, 4));
		var type = Encoding.ASCII.GetString(header, 4, 4);

		long atomSize = size;

		// Large-size atom: size field = 1, actual size follows in next 8 bytes
		if ( size != 1 )
		{
			return new Mp4Atom { Size = atomSize, Type = type, DataOffset = stream.Position };
		}

		var largeSize = new byte[8];
		read = await stream.ReadAsync(largeSize.AsMemory(0, 8), ct);
		if ( read < 8 )
		{
			return null;
		}

		atomSize = ( long ) BinaryPrimitives.ReadUInt64BigEndian(largeSize);

		return new Mp4Atom { Size = atomSize, Type = type, DataOffset = stream.Position };
	}
}
