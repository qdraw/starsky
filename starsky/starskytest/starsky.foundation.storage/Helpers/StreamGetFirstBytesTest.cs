using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.storage.Helpers;

namespace starskytest.starsky.foundation.storage.Helpers;

[TestClass]
public class StreamGetFirstBytesTest
{
	[TestMethod]
	public async Task GetFirstBytesAsync_ReturnsCorrectNumberOfBytes()
	{
		// Arrange
		var testData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
		var originalStream = new MemoryStream(testData);

		// Act
		var resultStream = await StreamGetFirstBytes.GetFirstBytesAsync(originalStream, 5);

		// Assert
		Assert.AreEqual(5, resultStream.Length);
	}

	[TestMethod]
	public async Task GetFirstBytesAsync_ReturnsCorrectData()
	{
		// Arrange
		var testData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
		var originalStream = new MemoryStream(testData);

		// Act
		var resultStream = await StreamGetFirstBytes.GetFirstBytesAsync(originalStream, 5);

		// Assert
		var resultData = resultStream.ToArray();
		CollectionAssert.AreEqual(new byte[] { 1, 2, 3, 4, 5 }, resultData);
	}

	[TestMethod]
	public async Task GetFirstBytesAsync_ReturnsEmptyStreamIfOriginalStreamIsEmpty()
	{
		// Arrange
		var originalStream = new MemoryStream();

		// Act
		var resultStream = await StreamGetFirstBytes.GetFirstBytesAsync(originalStream, 5);

		// Assert
		Assert.AreEqual(0, resultStream.Length);
	}

	[TestMethod]
	public async Task GetFirstBytesAsync_ReturnsEmptyStreamIfRequestedCountIsZero()
	{
		// Arrange
		var testData = new byte[] { 1, 2, 3, 4, 5 };
		var originalStream = new MemoryStream(testData);

		// Act
		var resultStream = await StreamGetFirstBytes.GetFirstBytesAsync(originalStream, 0);

		// Assert
		Assert.AreEqual(0, resultStream.Length);
	}

	[TestMethod]
	[SuppressMessage("ReSharper", "MustUseReturnValue")]
	[SuppressMessage("Performance",
		"CA1835:Prefer the \'Memory\'-based overloads for \'ReadAsync\' and \'WriteAsync\'")]
	public async Task GetFirstBytesAsync_SourceStreamContentRemainsSame()
	{
		// Arrange
		var testData = new byte[] { 1, 2, 3, 4, 5 };
		var originalStream = new MemoryStream(testData);

		// Create a copy of the original stream content
		var originalContent = new byte[testData.Length];
		originalStream.Position = 0;
		await originalStream.ReadAsync(originalContent, 0, originalContent.Length,
			CancellationToken.None);

		// Act
		var resultStream = await StreamGetFirstBytes.GetFirstBytesAsync(originalStream, 5);

		// Create a copy of the result stream content
		var resultContent = new byte[resultStream.Length];
		resultStream.Position = 0;
		await resultStream.ReadAsync(resultContent, 0, resultContent.Length,
			CancellationToken.None);

		// Assert
		CollectionAssert.AreEqual(originalContent, resultContent);
	}

	[TestMethod]
	public void GetFirstBytes_ReturnsCorrectNumberOfBytes()
	{
		// Test for: Sync Version
		
		// Arrange
		var testData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
		var originalStream = new MemoryStream(testData);

		// Act
		var resultStream = StreamGetFirstBytes.GetFirstBytes(originalStream, 5);

		// Assert
		Assert.AreEqual(5, resultStream.Length);
	}

	[TestMethod]
	public void GetFirstBytes_ReturnsCorrectData()
	{
		// Test for: Sync Version

		// Arrange
		var testData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
		var originalStream = new MemoryStream(testData);

		// Act
		var resultStream = StreamGetFirstBytes.GetFirstBytes(originalStream, 5);

		// Assert
		var resultData = resultStream.ToArray();
		CollectionAssert.AreEqual(new byte[] { 1, 2, 3, 4, 5 }, resultData);
	}

	[TestMethod]
	public void GetFirstBytes_ReturnsEmptyStreamIfOriginalStreamIsEmpty()
	{
		// Test for: Sync Version

		// Arrange
		var originalStream = new MemoryStream();

		// Act
		var resultStream = StreamGetFirstBytes.GetFirstBytes(originalStream, 5);

		// Assert
		Assert.AreEqual(0, resultStream.Length);
	}

	[TestMethod]
	public void GetFirstBytes_ReturnsEmptyStreamIfRequestedCountIsZero()
	{
		// Test for: Sync Version

		// Arrange
		var testData = new byte[] { 1, 2, 3, 4, 5 };
		var originalStream = new MemoryStream(testData);

		// Act
		var resultStream = StreamGetFirstBytes.GetFirstBytes(originalStream, 0);

		// Assert
		Assert.AreEqual(0, resultStream.Length);
	}

	[TestMethod]
	[SuppressMessage("ReSharper", "MustUseReturnValue")]
	public void GetFirstBytes_SourceStreamContentRemainsSame()
	{
		// Test for: Sync Version

		// Arrange
		var testData = new byte[] { 1, 2, 3, 4, 5 };
		var originalStream = new MemoryStream(testData);

		// Create a copy of the original stream content
		var originalContent = new byte[testData.Length];
		originalStream.Position = 0;
		originalStream.Read(originalContent, 0, originalContent.Length);

		// Act
		var resultStream = StreamGetFirstBytes.GetFirstBytes(originalStream, 5);

		// Create a copy of the result stream content
		var resultContent = new byte[resultStream.Length];
		resultStream.Position = 0;
		resultStream.Read(resultContent, 0, resultContent.Length);

		// Assert
		CollectionAssert.AreEqual(originalContent, resultContent);
	}
}
