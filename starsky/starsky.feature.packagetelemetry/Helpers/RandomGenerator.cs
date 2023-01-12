using System;
using System.Linq;

namespace starsky.feature.packagetelemetry.Helpers;

/// <summary>
/// @see: https://gist.github.com/enif-lee/c2c38c53d8cd2febb2f14922153352c0
/// </summary>
public static class RandomGenerator
{
	private const string Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
	private static readonly Random Random = new Random();

	/// <summary>
	///     Random token generator, start time dependent
	///     NOTE : If you need a powerful random string generator
	///     Reference: https://stackoverflow.com/questions/32932679/using-rngcryptoserviceprovider-to-generate-random-string
	/// </summary>
	/// <param name="length"></param>
	/// <returns></returns>
	public static string GenerateToken(int length)
	{
		return GenerateToken(Alphabet, length);
	}

	private static string GenerateToken(string characters, int length)
	{
		return new string(Enumerable
			.Range(0, length)
			.Select(num => characters[Random.Next() % characters.Length])
			.ToArray());
	}

}
