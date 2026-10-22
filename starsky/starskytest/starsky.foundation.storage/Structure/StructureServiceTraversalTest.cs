using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Structure;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.storage.Structure;

[TestClass]
public sealed class StructureServiceTraversalTest
{
	[TestMethod]
	[DataRow("/../../x/{filenamebase}.ext")]
	[DataRow("/../yyyy/{filenamebase}.ext")]
	[DataRow("/yyyy/../../{filenamebase}.ext")]
	[DataRow("/yyyy/\\.\\./{filenamebase}.ext")] // escaped dots parse to '..'
	public void ParseSubfolders_DotDotSegments_AreNotEmitted(string structure)
	{
		// the header/param goes through StructureRegexHelper.StructureCheck first
		var accepted = AppSettingsStructureModelAccepts(structure);
		if ( !accepted )
		{
			// rejected by the model, safe
			return;
		}

		var service = new StructureService(new FakeSelectorStorage(),
			new AppSettingsStructureModel(structure), new FakeIWebLogger());
		var subfolders = service.ParseSubfolders(new StructureInputModel(
			new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Local), "file", "jpg",
			ExtensionRolesHelper.ImageFormat.jpg, string.Empty));

		Assert.DoesNotContain("..", subfolders,
			$"structure '{structure}' produced '{subfolders}'");
	}

	[TestMethod]
	[DataRow("/../../x/{filenamebase}.ext")]
	[DataRow("/../yyyy/{filenamebase}.ext")]
	[DataRow("/yyyy/../../{filenamebase}.ext")]
	[DataRow("/yyyy/ ../{filenamebase}.ext")]
	public void StructureCheck_DotDotSegments_AreRejected(string structure)
	{
		Assert.IsFalse(AppSettingsStructureModelAccepts(structure));
	}

	[TestMethod]
	public void StructureCheck_NormalStructure_IsAccepted()
	{
		Assert.IsTrue(AppSettingsStructureModelAccepts("/yyyy/MM/yyyy_MM_dd/{filenamebase}.ext"));
		Assert.IsTrue(AppSettingsStructureModelAccepts("/yyyy/MM/dd_v1.0/{filenamebase}.ext"));
	}

	private static bool AppSettingsStructureModelAccepts(string structure)
	{
		return global::starsky.foundation.platform.Models.Structure.StructureRegexHelper
			.StructureCheck(structure);
	}
}
