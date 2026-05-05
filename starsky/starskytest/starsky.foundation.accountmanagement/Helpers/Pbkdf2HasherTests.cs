using System;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.accountmanagement.Helpers;
using starsky.foundation.database.Models.Account;

namespace starskytest.starsky.foundation.accountmanagement.Helpers;

[TestClass]
public class Pbkdf2HasherTests
{
	[TestMethod]
	public void ComputeHash_ReturnsHash_ForValidInputs()
	{
		// Arrange
		const string password = "testPassword";
		var salt = new byte[16];
		RandomNumberGenerator.Fill(salt);

		// Act
		var hash = Pbkdf2Hasher.ComputeHash(password, salt);

		// Assert
		Assert.IsNotNull(hash);
		Assert.IsGreaterThan(0, hash.Length);
	}

	[TestMethod]
	public void ComputeHash_ConsistentResults_ForSameInputs()
	{
		// Arrange
		const string password = "consistentPassword";
		var salt = new byte[16];
		RandomNumberGenerator.Fill(salt);

		// Act
		var hash1 = Pbkdf2Hasher.ComputeHash(password, salt);
		var hash2 = Pbkdf2Hasher.ComputeHash(password, salt);

		// Assert
		Assert.AreEqual(hash1, hash2);
	}

	[TestMethod]
	public void ComputeHash_DifferentResults_ForDifferentSalts()
	{
		// Arrange
		const string password = "testPassword";
		var salt1 = new byte[16];
		var salt2 = new byte[16];
		RandomNumberGenerator.Fill(salt1);
		RandomNumberGenerator.Fill(salt2);

		// Act
		var hash1 = Pbkdf2Hasher.ComputeHash(password, salt1);
		var hash2 = Pbkdf2Hasher.ComputeHash(password, salt2);

		// Assert
		Assert.AreNotEqual(hash1, hash2);
	}

	[TestMethod]
	public void ComputeHash_DifferentIterationCounts_ReturnsDifferentHashes()
	{
		const string password = "testPassword";
		var salt = new byte[16];
		RandomNumberGenerator.Fill(salt);

		var hash1 =
			Pbkdf2Hasher.ComputeHash(password, salt,
				IterationCountType.IterateLegacySha1); // 10,000 SHA-1
		var hash2 = Pbkdf2Hasher.ComputeHash(password, salt); // 600,000 SHA-256 (default)

		Assert.AreNotEqual(hash1, hash2);
	}

	[TestMethod]
	public void ComputeHash_DifferentIterations_HashesCompareLengths()
	{
		const string password = "testPassword";
		var salt = new byte[16];
		RandomNumberGenerator.Fill(salt);

		var hash1 =
			Pbkdf2Hasher.ComputeHash(password, salt,
				IterationCountType.IterateLegacySha1); // 10,000 SHA-1
		var hash2 = Pbkdf2Hasher.ComputeHash(password, salt); // 600,000 SHA-256 (default)

		Assert.AreEqual(hash1.Length, hash2.Length,
			"Hashes should have the same length regardless of iterations.");
		Assert.AreNotEqual(hash1, hash2, "Hashes should differ due to different iteration counts.");
	}

	[TestMethod]
	public void ComputeHash_ThrowsArgumentNullException_ForNullSalt()
	{
		// Arrange
		const string password = "testPassword";

		// Act and Assert
		Assert.ThrowsExactly<ArgumentNullException>(() =>
			Pbkdf2Hasher.ComputeHash(password, null!));
	}

	[TestMethod]
	public void ComputeHash_ThrowsArgumentNullException_ForNullPassword()
	{
		// Arrange
		var salt = new byte[16];
		RandomNumberGenerator.Fill(salt);

		// Act and Assert
		Assert.ThrowsExactly<ArgumentNullException>(() => Pbkdf2Hasher.ComputeHash(null!, salt));
	}

	[TestMethod]
	public void ComputeHash_ReturnsDifferentHashes_ForDifferentPasswords()
	{
		// Arrange
		var salt = new byte[16];
		RandomNumberGenerator.Fill(salt);

		// Act
		var hash1 = Pbkdf2Hasher.ComputeHash("password1", salt);
		var hash2 = Pbkdf2Hasher.ComputeHash("password2", salt);

		// Assert
		Assert.AreNotEqual(hash1, hash2);
	}

	[TestMethod]
	public void GenerateRandomSalt_ShouldReturnCorrectLength()
	{
		var salt = Pbkdf2Hasher.GenerateRandomSalt();
		Assert.HasCount(32, salt); // 256 / 8 = 32 bytes
	}

	[TestMethod]
	public void GenerateRandomSalt_ShouldReturnUniqueValues()
	{
		// Act
		var salt1 = Pbkdf2Hasher.GenerateRandomSalt();
		var salt2 = Pbkdf2Hasher.GenerateRandomSalt();

		// Assert NOT EQ
		Assert.AreNotEqual(salt1, salt2); // Check that the two salts are different
	}
}
