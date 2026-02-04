// Copyright © 2018 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace starsky.foundation.accountmanagement.Helpers;

public static class Pbkdf2Hasher
{
	/// <summary>
	/// Get secured hash passwords based on a salt
	/// </summary>
	/// <param name="password">password</param>
	/// <param name="salt">to decrypt</param>
	/// <param name="iteration100K">more secure password</param>
	/// <param name="useSha256">more secure password</param>
	/// <returns>hashed password</returns>
	public static string ComputeHash(string password, byte[] salt, bool iteration100K = true, bool useSha256 = true)
	{
		// Use 100K iterations for new passwords, and 10K iterations for older stored hashes
		var iterationCount = iteration100K ? 100_000 : 10000;
		var hashType = useSha256 ? KeyDerivationPrf.HMACSHA256 : KeyDerivationPrf.HMACSHA1;

		return Convert.ToBase64String(
			KeyDerivation.Pbkdf2(
				password: password,
				salt: salt,
				prf: hashType,
				iterationCount: iterationCount,
				numBytesRequested: 256 / 8
			)
		);
	}

	/// <summary>
	/// Generate a random salt
	/// </summary>
	/// <returns>random salt</returns>
	public static byte[] GenerateRandomSalt()
	{
		var salt = new byte[128 / 8];

		using var rng = RandomNumberGenerator.Create();
		rng.GetBytes(salt);

		return salt;
	}
}
