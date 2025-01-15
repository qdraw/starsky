using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.storage.Storage;

namespace starskytest.starsky.foundation.storage.Storage;

[TestClass]
public sealed class ThumbnailNameHelperTest
{
	[TestMethod]
	public void GetSize_TinyMeta_Enum()
	{
		var result = ThumbnailNameHelper.GetSize(ThumbnailSize.TinyMeta);
		Assert.AreEqual(150, result);
	}

	[TestMethod]
	[DataRow(ThumbnailSize.TinyIcon, 4)]
	[DataRow(ThumbnailSize.TinyMeta, 150)]
	[DataRow(ThumbnailSize.Small, 300)]
	[DataRow(ThumbnailSize.Large, 1000)]
	[DataRow(ThumbnailSize.ExtraLarge, 2000)]
	public void GetSize_Enum(ThumbnailSize size, int expected)
	{
		var result = ThumbnailNameHelper.GetSize(size);
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void GetSize_Enum_Invalid()
	{
		Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
			ThumbnailNameHelper.GetSize(ThumbnailSize.Unknown));
	}

	[TestMethod]
	[DataRow(4, ThumbnailSize.TinyIcon)]
	[DataRow(150, ThumbnailSize.TinyMeta)]
	[DataRow(300, ThumbnailSize.Small)]
	[DataRow(1000, ThumbnailSize.Large)]
	[DataRow(2000, ThumbnailSize.ExtraLarge)]
	[DataRow(9999999, ThumbnailSize.Unknown)]
	public void GetSize_Int(int size, ThumbnailSize expected)
	{
		var result = ThumbnailNameHelper.GetSize(size);
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void Combine_Compare()
	{
		var result =
			ThumbnailNameHelper.Combine("test_hash", 2000, new AppSettings().ThumbnailImageFormat);
		var result2 = ThumbnailNameHelper.Combine("test_hash", ThumbnailSize.ExtraLarge,
			new AppSettings().ThumbnailImageFormat);

		Assert.AreEqual(result, result2);
	}

	[TestMethod]
	public void Combine_Enum_Invalid()
	{
		Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
			ThumbnailNameHelper.Combine(string.Empty, ThumbnailSize.Unknown,
				ThumbnailImageFormat.jpg));
	}

	[TestMethod]
	public void GetSize_Name_ExtraLarge()
	{
		var input = ThumbnailNameHelper.Combine("01234567890123456789123456",
			2000, new AppSettings().ThumbnailImageFormat);
		var result2 = ThumbnailNameHelper.GetSize(input, ThumbnailImageFormat.jpg);
		Assert.AreEqual(ThumbnailSize.ExtraLarge, result2);
	}

	[TestMethod]
	public void GetSize_Name_Large()
	{
		var input = ThumbnailNameHelper.Combine("01234567890123456789123456",
			ThumbnailSize.Large, new AppSettings().ThumbnailImageFormat);
		var result2 = ThumbnailNameHelper.GetSize(input, ThumbnailImageFormat.jpg);
		Assert.AreEqual(ThumbnailSize.Large, result2);
	}

	[TestMethod]
	public void GetSize_Name_NonValidLength()
	{
		var input = "01234567890123456789123456@859693845";
		var result2 = ThumbnailNameHelper.GetSize(input, ThumbnailImageFormat.jpg);
		Assert.AreEqual(ThumbnailSize.Unknown, result2);
	}

	[TestMethod]
	[DataRow("01234567890123456789123456@859693845", "01234567890123456789123456")]
	[DataRow("01234567890123456789123456@859693845.webp", "01234567890123456789123456")]
	[DataRow("T4CE5GNTHWFQ5AOXD7OYDMJERA@2000.jpg", "T4CE5GNTHWFQ5AOXD7OYDMJERA")]
	[DataRow("T4CE5GNTHWFQ5AOXD7OYDMJERA@2000", "T4CE5GNTHWFQ5AOXD7OYDMJERA")]
	[DataRow("", "")]
	[DataRow(null, "")]
	public void RemoveSuffix(string? input, string output)
	{
		var result2 = ThumbnailNameHelper.RemoveSuffix(input);
		Assert.AreEqual(output, result2);
	}

	[TestMethod]
	public void GetSize_Name_Large_NonValidLength()
	{
		var input = ThumbnailNameHelper.Combine("non_valid_length", ThumbnailSize.Large,
			new AppSettings().ThumbnailImageFormat);
		var result2 = ThumbnailNameHelper.GetSize(input, ThumbnailImageFormat.jpg);
		Assert.AreEqual(ThumbnailSize.Unknown, result2);
	}

	[TestMethod]
	public void GetSize_Name_UnknownSize()
	{
		const string input = "test_hash@4789358";
		var result2 = ThumbnailNameHelper.GetSize(input, ThumbnailImageFormat.jpg);
		Assert.AreEqual(ThumbnailSize.Unknown, result2);
	}
}
