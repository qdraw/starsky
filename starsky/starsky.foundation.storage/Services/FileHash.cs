using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using starsky.foundation.platform.Extensions;
using starsky.foundation.storage.Interfaces;

namespace starsky.foundation.storage.Services
{

	public class FileHash
	{
		private readonly IStorage _iStorage;

		public FileHash(IStorage iStorage)
		{
			_iStorage = iStorage;
		}


		// Two public interfaces
		// Returns list of hashCodes
		// or one hashcode (base32)


		/// <summary>
		/// Get the hashCodes of a array of files
		/// Uses the default timeout
		/// </summary>
		/// <param name="filesInDirectorySubPath">array</param>
		/// <returns>array of base32 hashes</returns>
		public List<KeyValuePair<string, bool>> GetHashCode(
			string[] filesInDirectorySubPath)
		{
			return filesInDirectorySubPath
				.Select(subPath => _calcHashCode(subPath)).ToList();
		}

		/// <summary>
		/// Returns a Base32 case insensitive fileHash, used with the default timeout of 1 minute
		/// </summary>
		/// <param name="subPath">subPath</param>
		/// <param name="timeoutInMilliseconds">Timeout in ms seconds, before a random string will be returned</param>
		/// <returns>base32 hash</returns>
		public KeyValuePair<string, bool> GetHashCode(string subPath,
			int timeoutInMilliseconds = 30000)
		{
			return _calcHashCode(subPath, timeoutInMilliseconds);
		}

		// Here are some tricks used to avoid that CalculateMd5Async keeps waiting forever.
		// In some cases hashing a file keeps waiting forever (at least on linux-arm)

		private KeyValuePair<string, bool> _calcHashCode(string subPath,
			int timeoutInMilliseconds = 30000)
		{
			var q = Md5TimeoutAsyncWrapper(subPath, timeoutInMilliseconds)
				.Result;
			return q;
		}

		// Wrapper to do Async tasks -- add variable to test make it in a unit test shorter
		private async Task<KeyValuePair<string, bool>> Md5TimeoutAsyncWrapper(
			string fullFileName, int timeoutInMilliseconds)
		{
			// adding .ConfigureAwait(false) may NOT be what you want, but google it.
			return await Task.Run(() =>
					GetHashCodeAsync(fullFileName, timeoutInMilliseconds))
				.ConfigureAwait(false);
		}

		/// <summary>
		/// Get FileHash Async in the timeoutSeconds time
		/// </summary>
		/// <param name="fullFileName">full filePath on disk to have the file</param>
		/// <param name="timeoutInMilliseconds">number of milli seconds to be hashed</param>
		/// <returns></returns>
		public async Task<KeyValuePair<string, bool>> GetHashCodeAsync(
			string fullFileName, int timeoutInMilliseconds = 30000)
		{
			try
			{
				var code = await CalculateMd5Async(fullFileName)
					.TimeoutAfter(timeoutInMilliseconds);

				if ( string.IsNullOrEmpty(code) ) {
					return new KeyValuePair<string, bool>(
						Base32.Encode(
						GenerateRandomBytes(27)
						) + "_T", false);
				}
				return new KeyValuePair<string, bool>(code, true);
			}
			catch ( TimeoutException )
			{
				// Sometimes a Calc keeps waiting for days
				Console.WriteLine(
					">>>>>>>>>>>            Timeout Md5 Hashing::: "
					+ fullFileName
					+ "            <<<<<<<<<<<<");
				return new KeyValuePair<string, bool>(
					Base32.Encode(GenerateRandomBytes(27)) + "_T", false);
			}
		}

		/// <summary>
		/// Create a random string
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

			// Create a new RNGCryptoServiceProvider.
			var rand = new RNGCryptoServiceProvider();

			// Fill the buffer with random bytes.
			rand.GetBytes(randBytes);

			// return the bytes.
			return randBytes;
		}

		/// <summary>
		/// 114Kb
		/// </summary>
		public const int MaxReadSize = 114688;

		/// <summary>
		/// Calculate the hash based on the first 16 Kilobytes of the file
		/// @see https://stackoverflow.com/a/45573180
		/// </summary>
		/// <param name="fullFilePath">full File Path on disk</param>
		/// <returns>Task with a md5 hash</returns>
		private async Task<string> CalculateMd5Async(string fullFilePath)
		{
			if ( !_iStorage.ExistFile(fullFilePath) ) return string.Empty;
			using ( var stream = _iStorage.ReadStream(fullFilePath, MaxReadSize) ) 
			{
				return await CalculateHashAsync(stream);
			}
		}

		public static async Task<string> CalculateHashAsync(Stream stream)
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
						.ReadAsync(block, 0, block.Length)
						.ConfigureAwait(false) ) > 0 )
					{
						md5.TransformBlock(block, 0, length, null, 0);
					}

					md5.TransformFinalBlock(block, 0, 0);
					stream.Close();

					var hash = md5.Hash;
					return Base32.Encode(hash);
				}
			}
			finally
			{
				ArrayPool<byte>.Shared.Return(block);
			}
		}

	}
}
