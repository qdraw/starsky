using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.foundation.native.FileSystem.Interfaces;
using starsky.foundation.native.FileSystem.Models;

namespace starskytest.Controllers;

[TestClass]
public class MacOsNativeFilePickerControllerTest
{
	[TestMethod]
	public void PickFolder_DefaultIncludeFilesFalse_ReturnsPickerResult()
	{
		var expected = new MacOsFolderPickResult
		{
			Success = true,
			Path = "/tmp/folder",
			BookmarkToken = "abc"
		};
		var picker = new FakeMacOsNativeFilePicker(expected);
		var sut = new MacOsNativeFilePickerController(picker);

		var action = sut.PickFolder();
		var ok = action.Result as OkObjectResult;
		var result = ok?.Value as MacOsFolderPickResult;

		Assert.AreEqual(200, ok?.StatusCode);
		Assert.IsNotNull(result);
		Assert.IsTrue(result.Success);
		Assert.IsFalse(picker.LastIncludeFiles);
	}

	[TestMethod]
	public void PickFolder_IncludeFilesTrue_ForwardsOption()
	{
		var picker = new FakeMacOsNativeFilePicker(new MacOsFolderPickResult());
		var sut = new MacOsNativeFilePickerController(picker);

		_ = sut.PickFolder(true);

		Assert.IsTrue(picker.LastIncludeFiles);
	}

	private sealed class FakeMacOsNativeFilePicker : IMacOsNativeFilePicker
	{
		private readonly MacOsFolderPickResult _result;
		public bool LastIncludeFiles { get; private set; }

		public FakeMacOsNativeFilePicker(MacOsFolderPickResult result)
		{
			_result = result;
		}

		public MacOsFolderPickResult TryPickFolder(bool includeFiles = false)
		{
			LastIncludeFiles = includeFiles;
			return _result;
		}
	}
}

