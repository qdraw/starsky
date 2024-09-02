using System;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.accountmanagement.Helpers;

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
		Assert.IsTrue(hash.Length > 0);
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
		// Arrange
		const string password = "testPassword";
		var salt = new byte[16];
		RandomNumberGenerator.Fill(salt);

		// Act
		var hash1 = Pbkdf2Hasher.ComputeHash(password, salt, false); // 10,000 iterations
		var hash2 = Pbkdf2Hasher.ComputeHash(password, salt);  // 100,000 iterations

		// Assert
		Assert.AreNotEqual(hash1, hash2);
	}

	[TestMethod]
	public void ComputeHash_DifferentIterations_HashesCompareLengths()
	{
		// Arrange
		const string password = "testPassword";
		var salt = new byte[16];
		RandomNumberGenerator.Fill(salt);

		// Act
		var hash1 = Pbkdf2Hasher.ComputeHash(password, salt,false); // 10,000 iterations
		var hash2 = Pbkdf2Hasher.ComputeHash(password, salt);  // 100,000 iterations

		// Assert
		Assert.AreEqual(hash1.Length, hash2.Length, "Hashes should have the same length regardless of iterations.");
		Assert.AreNotEqual(hash1, hash2, "Hashes should differ due to different iteration counts.");
	}

	[TestMethod]
	public void ComputeHash_ThrowsArgumentNullException_ForNullSalt()
	{
		// Arrange
		const string password = "testPassword";

		// Act and Assert
		Assert.ThrowsException<ArgumentNullException>(() => Pbkdf2Hasher.ComputeHash(password, null!));
	}
	
	[TestMethod]
	public void ComputeHash_ThrowsArgumentNullException_ForNullPassword()
	{
		// Arrange
		var salt = new byte[16];
		RandomNumberGenerator.Fill(salt);

		// Act and Assert
		Assert.ThrowsException<ArgumentNullException>(() => Pbkdf2Hasher.ComputeHash(null!, salt));
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
		// Act
		var salt = Pbkdf2Hasher.GenerateRandomSalt();

		// Assert
		Assert.AreEqual(16, salt.Length); // 128 / 8 = 16
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
