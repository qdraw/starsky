using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.rename.Models;
using starsky.foundation.database.Models;

namespace starskytest.starsky.feature.rename.Models;

[TestClass]
public class RenameTokenPatternTest
{
	[TestMethod]
	public void GenerateFileName_InvalidPattern_ThrowsInvalidOperationException()
	{
		// Arrange: pattern that will leave braces after replacement, and is not a valid filename
		const string pattern = "{invalidtoken}.jpg";
		var tokenPattern = new RenameTokenPattern(pattern);
		var fileIndexItem = new FileIndexItem
		{
			FileName = "file.jpg",
			DateTime = new DateTime(2022, 1, 1,
				0, 0, 0, DateTimeKind.Local)
		};

		// Act & Assert
		Assert.ThrowsExactly<InvalidOperationException>(() =>
			tokenPattern.GenerateFileName(fileIndexItem));
	}

	[TestMethod]
	public void GenerateFileName_ArgumentNullException()
	{
		// Arrange: pattern that will leave braces after replacement, and is not a valid filename
		const string pattern = "{invalidtoken}.jpg";
		var tokenPattern = new RenameTokenPattern(pattern);
		var fileIndexItem = new FileIndexItem
		{
			FileName = "",
			DateTime = new DateTime(2022, 1, 1,
				0, 0, 0, DateTimeKind.Local)
		};

		// Act & Assert
		Assert.ThrowsExactly<ArgumentNullException>(() =>
			tokenPattern.GenerateFileName(fileIndexItem));
	}

	[TestMethod]
	public void GenerateFileName_Seqn_WithoutExtension()
	{
		const string pattern = "{filenamebase}{seqn}";
		var tokenPattern = new RenameTokenPattern(pattern);
		var fileIndexItem = new FileIndexItem
		{
			FileName = "test", DateTime = new DateTime(2022, 1, 1, 0, 0, 0, DateTimeKind.Local)
		};

		// Act & Assert
		Assert.ThrowsExactly<InvalidOperationException>(() =>
			tokenPattern.GenerateFileName(fileIndexItem, 2));
	}

	[TestMethod]
	public void GenerateFileName_ValidatePattern_Null()
	{
		// Act & Assert
		Assert.ThrowsExactly<ArgumentNullException>(() => { _ = new RenameTokenPattern(null!); });
	}

	[TestMethod]
	public void GenerateFileName_Empty()
	{
		var tokenPattern = new RenameTokenPattern(string.Empty);

		Assert.AreEqual("Pattern cannot be empty", tokenPattern.Errors.First());
	}

	[TestMethod]
	public void GenerateFileName_NonValidItemInBrackets()
	{
		var tokenPattern = new RenameTokenPattern("{test}");

		Assert.AreEqual("Unknown token: {test}", tokenPattern.Errors.First());
	}

	[TestMethod]
	[DataRow("example", 5, "example-5")]
	[DataRow("example", 0, "example-0")]
	[DataRow("example.jpg", 0, "example-0.jpg")]
	[DataRow("example.jpg", 5, "example-5.jpg")]
	public void InsertSequenceBeforeExtension(string input, int sequenceNumber, string expected)
	{
		var result = RenameTokenPattern.InsertSequenceBeforeExtension(input, sequenceNumber);
		Assert.AreEqual(expected, result);
	}
}
