using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using starsky.foundation.storage.Interfaces;

namespace starsky.foundation.storage.Helpers;

public class CheckSha256Helper(IStorage hostFileSystemStorage)
{
	/// <summary>
	///     Check if SHA256 hash is valid
	///     Instead of SHA1CryptoServiceProvider, we use SHA256.Create
	/// </summary>
	/// <param name="fullFilePath">path of exiftool.exe</param>
	/// <param name="checkSumOptions">list of SHA256 hashes</param>
	/// <returns></returns>
	public bool CheckSha256(string fullFilePath, IEnumerable<string> checkSumOptions)
	{
		using var buffer = hostFileSystemStorage.ReadStream(fullFilePath);
		using var hashAlgorithm = SHA256.Create();

		var byteHash = hashAlgorithm.ComputeHash(buffer);
		var hash = BitConverter.ToString(byteHash).Replace("-", string.Empty).ToLowerInvariant();
		return checkSumOptions.AsEnumerable()
			.Any(p => p.Equals(hash, StringComparison.InvariantCultureIgnoreCase));
	}
}
