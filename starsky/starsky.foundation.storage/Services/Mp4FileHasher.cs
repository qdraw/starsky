using System;
using System.Buffers;
using System.Buffers.Binary;
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

	private readonly IWebLogger _logger = logger;

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
		await using var stream = iStorage.ReadStream(fullFilePath, FileHash.MaxReadSize);
		if ( stream == Stream.Null )
		{
			return string.Empty;
		}

		using var md5 = System.Security.Cryptography.MD5.Create();
		var buffer = ArrayPool<byte>.Shared.Rent(BufferSize);
		try
		{
			while ( true )
			{
				var atom = await ReadAtomAsync(stream);
				if ( atom == null )
				{
					break;
				}

				_logger.LogDebug($"Found atom: Type={atom?.Type}, Size={atom?.Size}, DataOffset={atom?.DataOffset}");

				var payloadSize = atom.Value.Size - 8;

				if ( atom.Value.Type == "mdat" )
				{
					// Hash up to MaxBytesToHash of video content
					var remaining = Math.Min(payloadSize, MaxBytesToHash);

					while ( remaining > 0 )
					{
						var toRead = ( int ) Math.Min(buffer.Length, remaining);
						var bytesRead = await stream.ReadAsync(
							buffer.AsMemory(0, toRead));

						if ( bytesRead <= 0 )
						{
							break;
						}

						md5.TransformBlock(buffer, 0, bytesRead, null, 0);
						remaining -= bytesRead;
					}

					// Found and hashed the mdat atom, we're done
					md5.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
					var hash = md5.Hash;
					return Base32.Encode(hash!);
				}

				// Skip non-mdat atoms
				// Note: payloadSize could be very large, so we skip carefully
				try
				{
					if ( payloadSize > 0 && stream.CanSeek )
					{
						stream.Seek(payloadSize, SeekOrigin.Current);
					}
					else
					{
						// If we can't seek, read and discard
						var toSkip = payloadSize;
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
				}
				catch ( IOException )
				{
					// If seek/read fails, return empty to fall back to standard hashing
					return string.Empty;
				}
			}
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(buffer);
		}

		// No mdat atom found, return empty to fall back to standard hashing
		return string.Empty;
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
		if ( size == 1 )
		{
			var largeSize = new byte[8];
			read = await stream.ReadAsync(largeSize.AsMemory(0, 8), ct);
			if ( read < 8 )
			{
				return null;
			}

			atomSize = ( long ) BinaryPrimitives.ReadUInt64BigEndian(largeSize);
		}

		return new Mp4Atom { Size = atomSize, Type = type, DataOffset = stream.Position };
	}
}
