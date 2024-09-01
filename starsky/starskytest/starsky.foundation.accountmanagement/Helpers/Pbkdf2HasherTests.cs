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
		string password = "testPassword";
		byte[] salt = new byte[16];
		RandomNumberGenerator.Fill(salt);

		// Act
		string hash = Pbkdf2Hasher.ComputeHash(password, salt);

		// Assert
		Assert.IsNotNull(hash);
		Assert.IsTrue(hash.Length > 0);
	}

	[TestMethod]
	public void ComputeHash_ConsistentResults_ForSameInputs()
	{
		// Arrange
		string password = "consistentPassword";
		byte[] salt = new byte[16];
		RandomNumberGenerator.Fill(salt);

		// Act
		string hash1 = Pbkdf2Hasher.ComputeHash(password, salt);
		string hash2 = Pbkdf2Hasher.ComputeHash(password, salt);

		// Assert
		Assert.AreEqual(hash1, hash2);
	}

	[TestMethod]
	public void ComputeHash_DifferentResults_ForDifferentSalts()
	{
		// Arrange
		string password = "testPassword";
		byte[] salt1 = new byte[16];
		byte[] salt2 = new byte[16];
		RandomNumberGenerator.Fill(salt1);
		RandomNumberGenerator.Fill(salt2);

		// Act
		string hash1 = Pbkdf2Hasher.ComputeHash(password, salt1);
		string hash2 = Pbkdf2Hasher.ComputeHash(password, salt2);

		// Assert
		Assert.AreNotEqual(hash1, hash2);
	}

	[TestMethod]
	public void ComputeHash_DifferentIterationCounts_ReturnsDifferentHashes()
	{
		// Arrange
		string password = "testPassword";
		byte[] salt = new byte[16];
		RandomNumberGenerator.Fill(salt);

		// Act
		string hash1 = Pbkdf2Hasher.ComputeHash(password, salt, false); // 10,000 iterations
		string hash2 = Pbkdf2Hasher.ComputeHash(password, salt, true);  // 100,000 iterations

		// Assert
		Assert.AreNotEqual(hash1, hash2);
	}

	[TestMethod]
	public void ComputeHash_DifferentIterations_HashesCompareLengths()
	{
		// Arrange
		string password = "testPassword";
		byte[] salt = new byte[16];
		RandomNumberGenerator.Fill(salt);

		// Act
		string hash1 = Pbkdf2Hasher.ComputeHash(password, salt); // 10,000 iterations
		string hash2 = Pbkdf2Hasher.ComputeHash(password, salt, true);  // 100,000 iterations

		// Assert
		Assert.AreEqual(hash1.Length, hash2.Length, "Hashes should have the same length regardless of iterations.");
		Assert.AreNotEqual(hash1, hash2, "Hashes should differ due to different iteration counts.");
	}

	[TestMethod]
	[ExpectedException(typeof(ArgumentNullException))]
	public void ComputeHash_ThrowsArgumentNullException_ForNullPassword()
	{
		// Arrange
		byte[] salt = new byte[16];
		RandomNumberGenerator.Fill(salt);

		// Act
		Pbkdf2Hasher.ComputeHash(null, salt);
	}

	[TestMethod]
	[ExpectedException(typeof(ArgumentNullException))]
	public void ComputeHash_ThrowsArgumentNullException_ForNullSalt()
	{
		// Arrange
		string password = "testPassword";

		// Act
		Pbkdf2Hasher.ComputeHash(password, null);
	}

	[TestMethod]
	public void ComputeHash_ReturnsDifferentHashes_ForDifferentPasswords()
	{
		// Arrange
		byte[] salt = new byte[16];
		RandomNumberGenerator.Fill(salt);

		// Act
		string hash1 = Pbkdf2Hasher.ComputeHash("password1", salt);
		string hash2 = Pbkdf2Hasher.ComputeHash("password2", salt);

		// Assert
		Assert.AreNotEqual(hash1, hash2);
	}
}
