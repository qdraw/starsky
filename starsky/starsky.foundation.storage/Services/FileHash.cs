using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Interfaces;

namespace starsky.foundation.storage.Services;

[SuppressMessage("Usage",
	"S4790:Make sure this weak hash algorithm is not used in a sensitive context here.",
	Justification = "Not used for passwords")]
public sealed class FileHash
{
	public const string GeneratedPostFix = "_T";

	/// <summary>
	///     114Kb
	/// </summary>
	public const int MaxReadSize = 114688;

	private readonly IStorage _iStorage;
	private readonly IWebLogger _logger;
	private readonly Mp4FileHasher _mp4Hasher;

	/// <summary>
	///     Two public interfaces
	///     Returns list of hashCodes
	///     or one hashcode (base32)
	/// </summary>
	/// <param name="iStorage"></param>
	/// <param name="logger"></param>
	public FileHash(IStorage iStorage, IWebLogger logger)
	{
		_iStorage = iStorage;
		_logger = logger;
		_mp4Hasher = new Mp4FileHasher(iStorage, logger);
	}


	/// <summary>
	///     Get the hashCodes of an array of files
	///     Uses the default timeout
	/// </summary>
	/// <param name="filesInDirectorySubPath">array</param>
	/// <returns>array of base32 hashes</returns>
	public List<KeyValuePair<string, bool>> GetHashCode(
		string[] filesInDirectorySubPath)
	{
		return filesInDirectorySubPath
			.Select(subPath => _calcHashCode(subPath,
				null)).ToList();
	}

	/// <summary>
	///     Returns a Base32 caseInsensitive fileHash, used with the default timeout of 1 minute
	/// </summary>
	/// <param name="subPath">subPath</param>
	/// <param name="timeoutInMilliseconds">Timeout in ms seconds, before a random string will be returned</param>
	/// <returns>base32 hash</returns>
	public KeyValuePair<string, bool> GetHashCode(string subPath,
		ExtensionRolesHelper.ImageFormat? imageFormat,
		int timeoutInMilliseconds = 30000)
	{
		return _calcHashCode(subPath, imageFormat, timeoutInMilliseconds);
	}

	// Here are some tricks used to avoid that CalculateMd5Async keeps waiting forever.
	// In some cases hashing a file keeps waiting forever (at least on linux-arm)

	private KeyValuePair<string, bool> _calcHashCode(string subPath
		, ExtensionRolesHelper.ImageFormat? imageFormat,
		int timeoutInMilliseconds = 30000)
	{
		var q = Md5TimeoutAsyncWrapper(subPath,
				imageFormat, timeoutInMilliseconds)
			.Result;
		return q;
	}

	/// <summary>
	///     Wrapper to do Async tasks -- add variable to test make it in a unit test shorter
	/// </summary>
	/// <param name="fullFileName"></param>
	/// <param name="timeoutInMilliseconds"></param>
	/// <returns></returns>
	private async Task<KeyValuePair<string, bool>> Md5TimeoutAsyncWrapper(
		string fullFileName, ExtensionRolesHelper.ImageFormat? imageFormat,
		int timeoutInMilliseconds)
	{
		// adding .ConfigureAwait(false) may NOT be what you want, but google it.
		return await Task.Run(() =>
				GetHashCodeAsync(fullFileName, imageFormat, timeoutInMilliseconds))
			.ConfigureAwait(false);
	}

	/// <summary>
	///     Get FileHash Async in the timeoutSeconds time
	/// </summary>
	/// <param name="fullFileName">full filePath on disk to have the file</param>
	/// <param name="timeoutInMilliseconds">number of milliseconds to be hashed</param>
	/// <returns></returns>
	public async Task<KeyValuePair<string, bool>> GetHashCodeAsync(
		string fullFileName,
		ExtensionRolesHelper.ImageFormat? imageFormat,
		int timeoutInMilliseconds = 30000)
	{
		try
		{
			var code = await CalculateMd5Async(fullFileName, imageFormat)
				.TimeoutAfter(timeoutInMilliseconds);

			if ( string.IsNullOrEmpty(code) )
			{
				return new KeyValuePair<string, bool>(
					Base32.Encode(
						GenerateRandomBytes(27)
					) + GeneratedPostFix, false);
			}

			return new KeyValuePair<string, bool>(code, true);
		}
		catch ( TimeoutException )
		{
			// Sometimes a Calc keeps waiting for days
			_logger.LogError(
				">>>>>>>>>>>            Timeout Md5 Hashing::: "
				+ fullFileName
				+ "            <<<<<<<<<<<<");
			return new KeyValuePair<string, bool>(
				Base32.Encode(GenerateRandomBytes(27)) + "_T", false);
		}
	}

	/// <summary>
	///     Create a random string
	/// </summary>
	/// <param name="length">number of chars</param>
	/// <returns>random string</returns>
	public static byte[] GenerateRandomBytes(int length)
	{
		// Create a buffer
		byte[] randBytes;

		if ( length >= 1 )
		{
			randBytes = new byte[length];
		}
		else
		{
			randBytes = new byte[1];
		}

		// Create a new RNGCryptoServiceProvider
		var rand = RandomNumberGenerator.Create();

		// Fill the buffer with random bytes.
		rand.GetBytes(randBytes);

		// return the bytes.
		return randBytes;
	}

	/// <summary>
	///     Calculate the hash based on the first 16 Kilobytes of the file
	///     For MP4 files, attempts to hash only the mdat atom (video content) for performance
	///     @see https://stackoverflow.com/a/45573180
	/// </summary>
	/// <param name="fullFilePath">full File Path on disk</param>
	/// <returns>Task with a md5 hash</returns>
	private async Task<string> CalculateMd5Async(string fullFilePath,
		ExtensionRolesHelper.ImageFormat? imageFormat)
	{
		if ( !_iStorage.ExistFile(fullFilePath) )
		{
			return string.Empty;
		}

		imageFormat ??= new ExtensionRolesHelper(_logger).GetImageFormat(
			_iStorage.ReadStream(fullFilePath,
				160));

		// Check if this is an MP4 file
		if ( imageFormat == ExtensionRolesHelper.ImageFormat.mp4 )
		{
			var mp4Hash = await _mp4Hasher.HashMp4VideoContentAsync(fullFilePath);
			if ( !string.IsNullOrEmpty(mp4Hash) )
			{
				return mp4Hash;
			}
		}

		// Standard file hashing for non-MP4 files or if MP4 hashing fails
		using ( var stream = _iStorage.ReadStream(fullFilePath, MaxReadSize) )
		{
			if ( stream == Stream.Null )
			{
				return string.Empty;
			}

			return await CalculateHashAsync(stream);
		}
	}


	/// <summary>
	///     Does NOT seek at 0
	/// </summary>
	/// <param name="stream">memory or filestream</param>
	/// <param name="dispose">dispose afterwards</param>
	/// <param name="cancellationToken">cancel token</param>
	/// <returns>fileHash</returns>
	public static async Task<string> CalculateHashAsync(Stream stream, bool dispose = true,
		CancellationToken cancellationToken = default)
	{
		var block =
			ArrayPool<byte>.Shared
				.Rent(16384); // 16 Kilobytes each (7 times)
		try
		{
			using ( var md5 = MD5.Create() )
			{
				int length;
				while ( ( length = await stream
					       .ReadAsync(block, cancellationToken)
					       .ConfigureAwait(false) ) > 0 )
				{
					md5.TransformBlock(block, 0, length, null, 0);
				}

				md5.TransformFinalBlock(block, 0, 0);

				if ( dispose )
				{
					await stream.FlushAsync(cancellationToken);
					stream.Close();
					await stream.DisposeAsync(); // also flush
				}

				var hash = md5.Hash;
				return Base32.Encode(hash!);
			}
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(block);
		}
	}
}
