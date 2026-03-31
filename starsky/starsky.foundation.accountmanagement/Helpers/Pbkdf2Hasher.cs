// Copyright © 2018 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using starsky.foundation.database.Models.Account;

namespace starsky.foundation.accountmanagement.Helpers;

public static class Pbkdf2Hasher
{
	/// <summary>
	///     Compute a PBKDF2 hash whose algorithm and iteration count are driven by
	///     <paramref name="iterationType" />.  Default is 600 K iterations / SHA-256
	///     (OWASP 2023 recommendation).
	/// </summary>
	public static string ComputeHash(string password, byte[] salt,
		IterationCountType iterationType = IterationCountType.Iterate600KSha256)
	{
		var iterationCount = ( int ) iterationType;
		if ( iterationCount <= 0 )
		{
			iterationCount = 600_000;
		}

		var prf = iterationType == IterationCountType.IterateLegacySha1
			? KeyDerivationPrf.HMACSHA1
			: KeyDerivationPrf.HMACSHA256;

		return Convert.ToBase64String(
			KeyDerivation.Pbkdf2(
				password,
				salt,
				prf,
				iterationCount,
				256 / 8
			)
		);
	}

	/// <summary>
	///     Legacy overload kept for verifying hashes stored before the
	///     <see cref="IterationCountType" />-based API existed.
	///     <c>iteration100K=false</c> → 10 000 iterations / SHA-1 (legacy).
	///     <c>iteration100K=true</c>  → 100 000 iterations / SHA-256.
	///     New code should call the <see cref="IterationCountType" /> overload instead.
	/// </summary>
	[Obsolete("Use ComputeHash(password, salt, IterationCountType) instead.")]
	public static string ComputeHash(string password, byte[] salt,
		bool iteration100K, bool useSha256 = true)
	{
		var type = !iteration100K || !useSha256
			? IterationCountType.IterateLegacySha1
			: IterationCountType.Iterate100KSha256;

		return ComputeHash(password, salt, type);
	}

	/// <summary>
	///     Generate a cryptographically random 32-byte (256-bit) salt.
	/// </summary>
	public static byte[] GenerateRandomSalt()
	{
		var salt = new byte[256 / 8]; // 32 bytes – recommended minimum

		using var rng = RandomNumberGenerator.Create();
		rng.GetBytes(salt);

		return salt;
	}
}
