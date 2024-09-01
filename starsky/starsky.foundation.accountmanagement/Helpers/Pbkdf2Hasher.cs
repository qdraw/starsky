// Copyright © 2018 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace starsky.foundation.accountmanagement.Helpers
{
	public static class Pbkdf2Hasher
	{
		/// <summary>
		/// Get secured hash passwords based on a salt
		/// </summary>
		/// <param name="password">password</param>
		/// <param name="salt">to decrypt</param>
		/// <param name="iteration100K">more secure password</param>
		/// <returns>hased password</returns>
		public static string ComputeHash(string password, byte[] salt, bool iteration100K = false)
		{
			var iterationCount = iteration100K ? 100_000 : 10000;
			
			return Convert.ToBase64String(
				KeyDerivation.Pbkdf2(
					password: password,
					salt: salt,
					prf: KeyDerivationPrf.HMACSHA1,
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
			byte[] salt = new byte[128 / 8];

			using ( var rng = RandomNumberGenerator.Create() )
				rng.GetBytes(salt);

			return salt;
		}
	}
}
