using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using starsky.foundation.http.Interfaces;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Interfaces;

namespace starsky.foundation.writemeta.Services.ExifToolDownloader;

public class CheckSums(IStorage storage, IHttpClientHelper httpClientHelper, IWebLogger logger)
{
	internal async Task<KeyValuePair<bool, string>?> DownloadCheckSums(bool isMirror = false)
	{
		var url = !isMirror
			? ExifToolLocations.CheckSumLocation
			: ExifToolLocations.CheckSumLocationMirror;
			
		var checksums = await httpClientHelper.ReadString(url);
		if ( checksums.Key )
		{
			return checksums;
		}

		return null;
	}
	
	/// <summary>
	/// Check if SHA1 hash is valid
	/// Instead of SHA1CryptoServiceProvider, we use SHA1.Create
	/// </summary>
	/// <param name="fullFilePath">path of exiftool.exe</param>
	/// <param name="checkSumOptions">list of sha1 hashes</param>
	/// <returns></returns>
	internal bool CheckSha1(string fullFilePath, IEnumerable<string> checkSumOptions)
	{
		using var buffer = storage.ReadStream(fullFilePath);
		using var hashAlgorithm = SHA1.Create();

		var byteHash = hashAlgorithm.ComputeHash(buffer);
		var hash = BitConverter.ToString(byteHash).Replace("-", string.Empty)
			.ToLowerInvariant();
		return checkSumOptions.AsEnumerable().Any(p =>
			p.Equals(hash, StringComparison.InvariantCultureIgnoreCase));
	}

	/// <summary>
	/// Parse the content of checksum file
	/// </summary>
	/// <param name="checksumsValue">input file: see test for example</param>
	/// <param name="max">max number of SHA1 results</param>
	/// <returns></returns>
	internal string[] GetChecksumsFromTextFile(string checksumsValue, int max = 8)
	{
		var regexExifToolForWindowsName = new Regex("[a-z0-9]{40}",
			RegexOptions.None, TimeSpan.FromMilliseconds(100));
		var results = regexExifToolForWindowsName.Matches(checksumsValue).Select(m => m.Value)
			.ToArray();
		if ( results.Length < max ) return results;

		logger.LogError(
			$"More than {max} checksums found, this is not expected, code stops now");
		return [];
	}
	
	
	internal static string GetUnixTarGzFromChecksum(string checksumsValue)
	{
		// (?<=SHA1\()Image-ExifTool-[\d\.]+\.zip
		var regexExifToolForWindowsName = new Regex(
			@"(?<=SHA1\()Image-ExifTool-[0-9\.]+\.tar.gz",
			RegexOptions.None, TimeSpan.FromMilliseconds(100));
		return regexExifToolForWindowsName.Match(checksumsValue).Value;
	}

	internal static string GetWindowsZipFromChecksum(string checksumsValue)
	{
		// (?<=SHA1\()exiftool-[\d\.]+\.zip
		var regexExifToolForWindowsName = new Regex(@"(?<=SHA1\()exiftool-[0-9\.]+\.zip",
			RegexOptions.None, TimeSpan.FromMilliseconds(100));
		return regexExifToolForWindowsName.Match(checksumsValue).Value;
	}
	
}
