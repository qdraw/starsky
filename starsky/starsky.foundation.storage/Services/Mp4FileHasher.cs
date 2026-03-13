using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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
	///     Maximum bytes of video content to hash (1024 KB)
	///     Balances between collision resistance and performance
	/// </summary>
	private const int MaxBytesToHash = 1024 * 1024;

	private const int MaxReadVideoSize = 1024 * 1024; // 1024 KB

	/// <summary>
	///     Buffer size for reading atom data
	/// </summary>
	private const int BufferSize = 8192;

	/// <summary>
	///     Hash MP4 video content by reading only the mdat atom
	///     This is significantly faster than hashing the entire file for
	///     large video files
	/// </summary>
	/// <param name="fullFilePath">Path to the MP4 file</param>
	/// <returns>
	///     Base32 encoded MD5 hash of video content,
	///     or empty string if no mdat atom found
	/// </returns>
	public async Task<string> HashMp4VideoContentAsync(string fullFilePath)
	{
		using var md5 = MD5.Create();
		var buffer = ArrayPool<byte>.Shared.Rent(BufferSize);

		try
		{
			await using var stream = iStorage.ReadStream(fullFilePath, MaxReadVideoSize);
			return await ProcessMp4AtomsAsync(stream, md5, buffer, CancellationToken.None);
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
	internal async Task<string> ProcessMp4AtomsAsync(Stream stream,
		MD5 md5, byte[] buffer, CancellationToken cancellationToken)
	{
		if ( stream == Stream.Null )
		{
			return string.Empty;
		}

		// collect mdats for seekable streams, for non-seekable hash on-the-fly
		var mdats = new List<Mp4Atom>();
		while ( true )
		{
			var atom = await ReadAtomAsync(stream, cancellationToken);
			if ( atom == null )
			{
				break;
			}

			logger.LogDebug(
				$"Found atom: Type={atom.Value.Type}, " +
				$"Size={atom.Value.Size}, HeaderSize={atom.Value.HeaderSize}, DataOffset={atom.Value.DataOffset}");

			var payloadSize = atom.Value.Size - atom.Value.HeaderSize;
			if ( payloadSize < 0 )
			{
				logger.LogInformation("Mp4FileHasher.ProcessMp4AtomsAsync invalid payload size");
				return string.Empty;
			}

			if ( atom.Value.Type == "mdat" )
			{
				if ( stream.CanSeek )
				{
					mdats.Add(atom.Value);
					if ( !await SkipAtomAsync(stream, buffer, payloadSize) )
					{
						logger.LogInformation(
							"Mp4FileHasher.ProcessMp4AtomsAsync Failed to skip mdat atom");
						return string.Empty;
					}
				}
				else
				{
					// non-seekable: hash immediately from current position
					return await HashMdatAtomAsync(stream, md5, buffer, payloadSize);
				}

				continue;
			}

			// non-mdat atoms
			if ( payloadSize == 0 )
			{
				logger.LogInformation(
					"Mp4FileHasher.ProcessMp4AtomsAsync invalid zero-size non-mdat atom");
				return string.Empty;
			}

			if ( await SkipAtomAsync(stream, buffer, payloadSize) )
			{
				continue;
			}

			logger.LogInformation(
				"Mp4FileHasher.ProcessMp4AtomsAsync Failed to skip non-mdat atom");
			return string.Empty;
		}

		if ( mdats.Count == 0 )
		{
			return string.Empty; // no mdats found
		}

		// Hash collected mdats for seekable streams
		return await HashMdatAtomsSeekableAsync(stream, md5, buffer, mdats);
	}

	/// <summary>
	///     Hashes the mdat atom content
	/// </summary>
	private static async Task<string> HashMdatAtomAsync(Stream stream,
		MD5 md5, byte[] buffer, long payloadSize)
	{
		var remaining = Math.Min(payloadSize, MaxBytesToHash);
		long totalHashed = 0;
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
			totalHashed += bytesRead;
		}

		// If nothing was hashed, return empty to indicate fallback is needed
		if ( totalHashed == 0 )
		{
			return string.Empty;
		}

		md5.TransformFinalBlock([], 0, 0);
		var hash = md5.Hash;
		return Base32.Encode(hash!);
	}

	private static async Task<string> HashMdatAtomsSeekableAsync(Stream stream, MD5 md5,
		byte[] buffer,
		List<Mp4Atom> mdats)
	{
		// Choose mdats by largest payload first
		var sorted = mdats.OrderByDescending(a => a.Size - a.HeaderSize).ToList();
		long remainingToHash = MaxBytesToHash;
		long totalHashed = 0;
		foreach ( var atom in sorted )
		{
			var payloadSize = atom.Size - atom.HeaderSize;
			if ( payloadSize <= 0 )
			{
				continue;
			}

			var toHash = ( int ) Math.Min(payloadSize, remainingToHash);
			if ( toHash <= 0 )
			{
				break;
			}

			stream.Seek(atom.DataOffset, SeekOrigin.Begin);
			long rem = toHash;
			while ( rem > 0 )
			{
				var chunk = ( int ) Math.Min(buffer.Length, rem);
				var read = await stream.ReadAsync(buffer.AsMemory(0, chunk));
				if ( read <= 0 )
				{
					break;
				}

				md5.TransformBlock(buffer, 0, read, null, 0);
				rem -= read;
				totalHashed += read;
				remainingToHash -= read;
			}

			if ( remainingToHash <= 0 )
			{
				break;
			}
		}

		if ( totalHashed == 0 )
		{
			return string.Empty;
		}

		md5.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
		return Base32.Encode(md5.Hash!);
	}

	/// <summary>
	///     Skips a non-mdat atom by seeking or reading past its content
	/// </summary>
	/// <returns>True if skip was successful, false if an error occurred</returns>
	internal static async Task<bool> SkipAtomAsync(Stream stream, byte[] buffer, long payloadSize)
	{
		try
		{
			if ( payloadSize > 0 && stream.CanSeek )
			{
				// Clamp seek to remaining length to avoid exceptions on truncated files
				try
				{
					var remaining = Math.Max(0L, stream.Length - stream.Position);
					var toSeek = Math.Min(payloadSize, remaining);
					stream.Seek(toSeek, SeekOrigin.Current);
				}
				catch ( NotSupportedException )
				{
					// Fallback to reading when seek is not supported
					await SkipByReadingAsync(stream, buffer, payloadSize);
				}
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
		var headerSize = 8;

		// size == 0 : atom extends to end of file (if stream supports Length)
		if ( size == 0 )
		{
			if ( !stream.CanSeek )
			{
				// Unknown size and cannot determine, treat as invalid
				return null;
			}

			var startPos = stream.Position - headerSize;
			atomSize = Math.Max(0, stream.Length - startPos);
			headerSize = 8;
			return new Mp4Atom
			{
				Size = atomSize,
				Type = type,
				DataOffset = stream.Position,
				HeaderSize = headerSize
			};
		}

		// Large-size atom: size field = 1, actual size follows in next 8 bytes
		if ( size == 1 )
		{
			var largeSize = new byte[8];
			read = await stream.ReadAsync(largeSize.AsMemory(0, 8), ct);
			if ( read < 8 )
			{
				return null;
			}

			atomSize = ( long ) BinaryPrimitives.ReadUInt64BigEndian(largeSize);
			headerSize = 16;
			return new Mp4Atom
			{
				Size = atomSize,
				Type = type,
				DataOffset = stream.Position,
				HeaderSize = headerSize
			};
		}

		// Normal atom with 32-bit size
		return new Mp4Atom
		{
			Size = atomSize, Type = type, DataOffset = stream.Position, HeaderSize = headerSize
		};
	}

	/// <summary>
	///     Represents an MP4 atom header
	/// </summary>
	private struct Mp4Atom
	{
		public long Size;
		public string Type;
		public long DataOffset;
		public int HeaderSize;
	}
}
