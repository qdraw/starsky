using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.rename.Models;
using starsky.foundation.database.Models;

namespace starskytest.starsky.feature.rename.Models;

[TestClass]
public class RenameTokenPatternExceptionTest
{
	[TestMethod]
	public void GenerateFileName_InvalidPattern_ThrowsInvalidOperationException()
	{
		// Arrange: pattern that will leave braces after replacement, and is not a valid filename
		const string pattern = "{invalidtoken}.jpg";
		var tokenPattern = new RenameTokenPattern(pattern);
		var fileIndexItem = new FileIndexItem
		{
			FileName = "file.jpg", DateTime = new DateTime(2022, 1, 1)
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
			DateTime = new DateTime(2022, 1, 1)
		};

		// Act & Assert
		Assert.ThrowsExactly<ArgumentNullException>(() =>
			tokenPattern.GenerateFileName(fileIndexItem));
	}
}
