using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.native.FileSystem;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.native.FileSystem;

[TestClass]
public class MacOsNativeFilePickerTests
{
	[TestMethod]
	public void TryPickFolder_WhenCreateOpenPanelFails_ReturnsFalse()
	{
		var fake = new FakeMacOsNativeFilePickerNative { PanelToReturn = IntPtr.Zero };
		var logger = new FakeIWebLogger();
		var sut = new MacOsNativeFilePicker(fake, logger);

		var result = sut.TryPickFolder();

		Assert.IsFalse(result.Success);
		Assert.IsNull(result.Path);
		Assert.IsNull(result.BookmarkToken);
	}

	[TestMethod]
	public void TryPickFolder_WhenUserCancels_ReturnsFalse()
	{
		var fake = new FakeMacOsNativeFilePickerNative { RunModalResult = false };
		var logger = new FakeIWebLogger();
		var sut = new MacOsNativeFilePicker(fake, logger);

		var result = sut.TryPickFolder();

		Assert.IsFalse(result.Success);
		Assert.IsNull(result.Path);
		Assert.IsNull(result.BookmarkToken);
		Assert.IsTrue(fake.ConfigureCalled);
		Assert.IsFalse(fake.LastIncludeFilesValue);
	}

	[TestMethod]
	public void TryPickFolder_WhenSelectedUrlIsZero_ReturnsFalse()
	{
		var fake = new FakeMacOsNativeFilePickerNative { SelectedUrlToReturn = IntPtr.Zero };
		var logger = new FakeIWebLogger();
		var sut = new MacOsNativeFilePicker(fake, logger);

		var result = sut.TryPickFolder();

		Assert.IsFalse(result.Success);
		Assert.IsNull(result.Path);
		Assert.IsNull(result.BookmarkToken);
	}

	[TestMethod]
	public void TryPickFolder_WhenBookmarkCreationFails_ReturnsFalse()
	{
		var fake = new FakeMacOsNativeFilePickerNative { BookmarkDataToReturn = IntPtr.Zero };
		var logger = new FakeIWebLogger();
		var sut = new MacOsNativeFilePicker(fake, logger);

		var result = sut.TryPickFolder();

		Assert.IsFalse(result.Success);
		Assert.IsNull(result.Path);
		Assert.IsNull(result.BookmarkToken);
	}

	[TestMethod]
	public void TryPickFolder_WhenBookmarkBytesEmpty_ReturnsFalse()
	{
		var fake = new FakeMacOsNativeFilePickerNative { NsDataBytesToReturn = [] };
		var logger = new FakeIWebLogger();
		var sut = new MacOsNativeFilePicker(fake, logger);

		var result = sut.TryPickFolder();

		Assert.IsFalse(result.Success);
		Assert.IsNull(result.Path);
		Assert.IsNull(result.BookmarkToken);
	}

	[TestMethod]
	public void TryPickFolder_WhenSuccessful_ReturnsPathAndBookmarkToken()
	{
		const string expectedPath = "/tmp/selected-folder";
		var expectedBytes = new byte[] { 4, 3, 2, 1 };
		var fake = new FakeMacOsNativeFilePickerNative
		{
			PathToReturn = expectedPath,
			NsDataBytesToReturn = expectedBytes
		};
		var logger = new FakeIWebLogger();
		var sut = new MacOsNativeFilePicker(fake, logger);

		var result = sut.TryPickFolder();

		Assert.IsTrue(result.Success);
		Assert.AreEqual(expectedPath, result.Path);
		Assert.AreEqual(Convert.ToBase64String(expectedBytes), result.BookmarkToken);
	}

	[TestMethod]
	public void TryPickFolder_WhenIncludeFilesTrue_ForwardsOptionToNative()
	{
		var fake = new FakeMacOsNativeFilePickerNative();
		var logger = new FakeIWebLogger();
		var sut = new MacOsNativeFilePicker(fake, logger);

		_ = sut.TryPickFolder(true);

		Assert.IsTrue(fake.LastIncludeFilesValue);
	}

	[TestMethod]
	public void TryPickFolder_WhenModalReturnsFalse_ReturnsFailureWithError()
	{
		var fake = new FakeMacOsNativeFilePickerNative { RunModalResult = false };
		var logger = new FakeIWebLogger();
		var sut = new MacOsNativeFilePicker(fake, logger);

		var result = sut.TryPickFolder();

		Assert.IsFalse(result.Success);
		Assert.IsNotNull(result.Error);
		Assert.IsTrue(result.Error.Contains("main thread", StringComparison.OrdinalIgnoreCase) || 
		              result.Error.Contains("cancel", StringComparison.OrdinalIgnoreCase));
	}
}

