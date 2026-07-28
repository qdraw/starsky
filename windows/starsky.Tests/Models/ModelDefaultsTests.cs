using Microsoft.VisualStudio.TestTools.UnitTesting;
using Starsky.Windows.Models;

namespace Starsky.Tests.Models;

[TestClass]
public class ModelDefaultsTests
{
	[TestMethod]
	public void AppSettings_Defaults_AreExpected()
	{
		var model = new AppSettings();

		Assert.AreEqual(LocationMode.Local, model.Mode);
		Assert.IsNull(model.RemoteUrl);
		Assert.IsTrue(model.UpdatePolicyEnabled);
		Assert.IsNull(model.LastUpdateWarningUtc);
		Assert.IsNotNull(model.MainWindows);
		Assert.AreEqual(0, model.MainWindows.Count);
		Assert.IsNull(model.SettingsWindow);
	}

	[TestMethod]
	public void AppSettings_Setters_PersistValues()
	{
		var timestamp = DateTimeOffset.UtcNow;
		var window = new WindowStateInfo();
		var settingsWindow = new WindowStateInfo { Route = "settings" };

		var model = new AppSettings
		{
			Mode = LocationMode.Remote,
			RemoteUrl = "https://example.test",
			UpdatePolicyEnabled = false,
			LastUpdateWarningUtc = timestamp,
			MainWindows = new List<WindowStateInfo> { window },
			SettingsWindow = settingsWindow,
		};

		Assert.AreEqual(LocationMode.Remote, model.Mode);
		Assert.AreEqual("https://example.test", model.RemoteUrl);
		Assert.IsFalse(model.UpdatePolicyEnabled);
		Assert.AreEqual(timestamp, model.LastUpdateWarningUtc);
		Assert.AreEqual(1, model.MainWindows.Count);
		Assert.AreSame(window, model.MainWindows[0]);
		Assert.AreSame(settingsWindow, model.SettingsWindow);
	}

	[TestMethod]
	public void DetailViewMetadata_Defaults_AreExpected()
	{
		var model = new DetailViewMetadata();

		Assert.AreEqual(string.Empty, model.ParentDirectory);
		Assert.AreEqual(string.Empty, model.FileCollectionName);
		Assert.AreEqual(0, model.CollectionPaths.Count);
		Assert.AreEqual(0, model.SidecarExtensionsList.Count);
	}

	[TestMethod]
	public void DetailViewMetadata_InitValues_AreStored()
	{
		var collectionPaths = new List<string> { "a.jpg", "a.xmp" };
		var sidecars = new List<string> { "xmp" };

		var model = new DetailViewMetadata
		{
			ParentDirectory = "root/folder",
			FileCollectionName = "a",
			CollectionPaths = collectionPaths,
			SidecarExtensionsList = sidecars,
		};

		Assert.AreEqual("root/folder", model.ParentDirectory);
		Assert.AreEqual("a", model.FileCollectionName);
		Assert.AreSame(collectionPaths, model.CollectionPaths);
		Assert.AreSame(sidecars, model.SidecarExtensionsList);
	}

	[TestMethod]
	public void UrlValidationResult_Defaults_AreExpected()
	{
		var model = new UrlValidationResult();

		Assert.IsFalse(model.IsValid);
		Assert.IsFalse(model.IsLocal);
		Assert.AreEqual(string.Empty, model.Location);
		Assert.IsNull(model.Reason);
	}

	[TestMethod]
	public void UrlValidationResult_Setters_PersistValues()
	{
		var model = new UrlValidationResult
		{
			IsValid = true,
			IsLocal = true,
			Location = "http://localhost:9609",
			Reason = "ok",
		};

		Assert.IsTrue(model.IsValid);
		Assert.IsTrue(model.IsLocal);
		Assert.AreEqual("http://localhost:9609", model.Location);
		Assert.AreEqual("ok", model.Reason);
	}

	[TestMethod]
	public void WindowStateInfo_Defaults_AreExpected()
	{
		var model = new WindowStateInfo();

		Assert.AreEqual("?f=/", model.Route);
		Assert.AreEqual(80, model.X);
		Assert.AreEqual(80, model.Y);
		Assert.AreEqual(1400, model.Width);
		Assert.AreEqual(900, model.Height);
	}

	[TestMethod]
	public void WindowStateInfo_Setters_PersistValues()
	{
		var model = new WindowStateInfo
		{
			Route = "?f=/docs",
			X = 1,
			Y = 2,
			Width = 3,
			Height = 4,
		};

		Assert.AreEqual("?f=/docs", model.Route);
		Assert.AreEqual(1, model.X);
		Assert.AreEqual(2, model.Y);
		Assert.AreEqual(3, model.Width);
		Assert.AreEqual(4, model.Height);
	}

	[TestMethod]
	public void LocationMode_HasExpectedValues()
	{
		Assert.AreEqual(0, (int)LocationMode.Local);
		Assert.AreEqual(1, (int)LocationMode.Remote);
	}
}

