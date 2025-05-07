using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.writemeta.Helpers;

namespace starskytest.starsky.foundation.database.Models;

[TestClass]
public sealed class FileIndexItemTest
{
	[TestMethod]
	public void FileIndexItemTest_SetTagsToNull()
	{
		var item = new FileIndexItem { Tags = null };
		Assert.AreEqual(item.Tags, string.Empty);
	}

	[TestMethod]
	public void FileIndexItemTest_KeywordsToNull()
	{
		var item = new FileIndexItem { Keywords = null };
		// > read tags instead of keywords
		Assert.AreEqual(item.Tags, string.Empty);
	}

	[TestMethod]
	public void FileIndexItem_DoubleSpaces()
	{
		var item = new FileIndexItem
		{
			Tags = "test0, test1  ,   test2,   test3, " +
			       "test4,   test5, test6, test7,   "
		};

		Assert.AreEqual("test1", item.Keywords?.ToList()[1]);
		Assert.AreEqual("test2", item.Keywords?.ToList()[2]);
		Assert.AreEqual("test5", item.Keywords?.ToList()[5]);
		Assert.AreEqual("test7", item.Keywords?.ToList()[7]);
		Assert.AreEqual(8, item.Keywords?.Count);
	}

	[TestMethod]
	public void FileIndexItemTest_SetDescriptionsToNull()
	{
		var item = new FileIndexItem { Description = null };
		Assert.AreEqual(item.Description, string.Empty);
	}


	[TestMethod]
	public void FileIndexItemTest_SetColorClassTestDefault()
	{
		var input = ColorClassParser.GetColorClass();
		var output = ColorClassParser.Color.None;
		Assert.AreEqual(input, output);
	}

	[TestMethod]
	public void FileIndexItemTest_SetColorClassTestMin1()
	{
		var input = ColorClassParser.GetColorClass("-1");
		var output = ColorClassParser.Color.DoNotChange;
		Assert.AreEqual(input, output);
	}

	[TestMethod]
	[SuppressMessage("ReSharper", "RedundantArgumentDefaultValue")]
	public void FileIndexItemTest_SetColorClassTest0()
	{
		var input = ColorClassParser.GetColorClass("0");
		var output = ColorClassParser.Color.None;
		Assert.AreEqual(input, output);
	}

	[TestMethod]
	public void FileIndexItemTest_SetColorClassTest1()
	{
		var input = ColorClassParser.GetColorClass("1");
		var output = ColorClassParser.Color.Winner;
		Assert.AreEqual(input, output);
	}

	[TestMethod]
	public void FileIndexItemTest_SetColorClassTest2()
	{
		var input = ColorClassParser.GetColorClass("2");
		var output = ColorClassParser.Color.WinnerAlt;
		Assert.AreEqual(input, output);
	}

	[TestMethod]
	public void FileIndexItemTest_SetColorClassTest3()
	{
		var input = ColorClassParser.GetColorClass("3");
		var output = ColorClassParser.Color.Superior;
		Assert.AreEqual(input, output);
	}

	[TestMethod]
	public void FileIndexItemTest_SetColorClassTest4()
	{
		var input = ColorClassParser.GetColorClass("4");
		var output = ColorClassParser.Color.SuperiorAlt;
		Assert.AreEqual(input, output);
	}

	[TestMethod]
	public void FileIndexItemTest_SetColorClassTest5()
	{
		var input = ColorClassParser.GetColorClass("5");
		var output = ColorClassParser.Color.Typical;
		Assert.AreEqual(input, output);
	}

	[TestMethod]
	public void FileIndexItemTest_SetColorClassTest6()
	{
		var input = ColorClassParser.GetColorClass("6");
		var output = ColorClassParser.Color.TypicalAlt;
		Assert.AreEqual(input, output);
	}

	[TestMethod]
	public void FileIndexItemTest_SetColorClassTest7()
	{
		var input = ColorClassParser.GetColorClass("7");
		var output = ColorClassParser.Color.Extras;
		Assert.AreEqual(input, output);
	}

	[TestMethod]
	public void FileIndexItemTest_SetColorClassTest8()
	{
		var input = ColorClassParser.GetColorClass("8");
		var output = ColorClassParser.Color.Trash;
		Assert.AreEqual(input, output);
	}

	[TestMethod]
	public void FileIndexItemTest_GetColorClassListTestEightSeven()
	{
		var input = "8,7";
		var eightSeven = new List<ColorClassParser.Color>
		{
			ColorClassParser.Color.Trash, ColorClassParser.Color.Extras
		};
		var output = FileIndexItem.GetColorClassList(input);
		CollectionAssert.AreEqual(eightSeven, output);
	}

	[TestMethod]
	public void FileIndexItemTest_GetColorClassListString()
	{
		var input = "string";
		var output = FileIndexItem.GetColorClassList(input);
		Assert.AreEqual(0, output.Count); // <= 0
	}

	[TestMethod]
	public void FileIndexItemTest_FileIndexItemTitleTest()
	{
		var fileIndexItem = new FileIndexItem { Title = null };
		Assert.AreEqual(fileIndexItem.Title, string.Empty);
	}

	[TestMethod]
	public void FileIndexItemTest_FileNameNull()
	{
		var t = new FileIndexItem();
		Assert.AreEqual(string.Empty, t.FileName);
	}

	[TestMethod]
	public void FileIndexItemTest_ParentDirectoryNull()
	{
		var t = new FileIndexItem();
		Assert.AreEqual(string.Empty, t.ParentDirectory);
	}

	[TestMethod]
	public void FileIndexItemTest_FilePathNull()
	{
		var t = new FileIndexItem();
		Assert.AreEqual("/", t.FilePath);
	}

	[TestMethod]
	public void FileIndexItemTest_OrientationrelativeRotation0()
	{
		// keep the same
		var t = new FileIndexItem { Orientation = FileIndexItem.Rotation.Horizontal };
		Assert.AreEqual(FileIndexItem.Rotation.Horizontal, t.RelativeOrientation());
	}

	[TestMethod]
	public void FileIndexItemTest_SetOrientationrelativeRotation0()
	{
		var fileObject = new FileIndexItem { Orientation = FileIndexItem.Rotation.Horizontal };
		fileObject.SetRelativeOrientation();
		Assert.AreEqual(FileIndexItem.Rotation.Horizontal, fileObject.Orientation);
	}

	[TestMethod]
	public void FileIndexItemTest_OrientationrelativeRotation_270CwTest()
	{
		var fileObject = new FileIndexItem { Orientation = FileIndexItem.Rotation.Rotate270Cw };
		Assert.AreEqual(FileIndexItem.Rotation.Horizontal, fileObject.RelativeOrientation(1));
	}


	[TestMethod]
	public void FileIndexItemTest_SetOrientationrelativeRotationMinus1()
	{
		var t = new FileIndexItem { Orientation = FileIndexItem.Rotation.Horizontal };
		Assert.AreEqual(FileIndexItem.Rotation.Rotate270Cw, t.RelativeOrientation(-1));
	}

	[TestMethod]
	public void FileIndexItemTest_SetOrientationrelativeRotationPlus1()
	{
		var t = new FileIndexItem { Orientation = FileIndexItem.Rotation.Horizontal };
		Assert.AreEqual(FileIndexItem.Rotation.Rotate90Cw, t.RelativeOrientation(1));
	}

	[TestMethod]
	public void FileIndexItemTest_SetOrientationrelativeRotation_Rotate270Cw_Plus1()
	{
		var t = new FileIndexItem { Orientation = FileIndexItem.Rotation.Rotate270Cw };
		Assert.AreEqual(FileIndexItem.Rotation.Horizontal, t.RelativeOrientation(1));
	}


	[TestMethod]
	public void FileIndexItemTest_SetOrientationrelativeRelativeOrientation_Plus5()
	{
		// test not very good
		var t = new FileIndexItem { Orientation = FileIndexItem.Rotation.Rotate270Cw };
		Assert.AreEqual(FileIndexItem.Rotation.Horizontal, t.RelativeOrientation(5));
	}

	[TestMethod]
	public void FileIndexItemTest_SetAbsoluteOrientation_DoNotChange()
	{
		var rotationItem = new FileIndexItem().SetAbsoluteOrientation();
		Assert.AreEqual(FileIndexItem.Rotation.DoNotChange, rotationItem);
	}

	[TestMethod]
	public void FileIndexItemTest_SetAbsoluteOrientation_Rotate90Cw()
	{
		var rotationItem = new FileIndexItem().SetAbsoluteOrientation("6");
		Assert.AreEqual(FileIndexItem.Rotation.Rotate90Cw, rotationItem);
	}

	[TestMethod]
	public void FileIndexItemTest_SetAbsoluteOrientation_Rotate180()
	{
		var rotationItem = new FileIndexItem().SetAbsoluteOrientation("3");
		Assert.AreEqual(FileIndexItem.Rotation.Rotate180, rotationItem);
	}

	[TestMethod]
	public void FileIndexItemTest_SetAbsoluteOrientation_Rotate270Cw()
	{
		var rotationItem = new FileIndexItem().SetAbsoluteOrientation("8");
		Assert.AreEqual(FileIndexItem.Rotation.Rotate270Cw, rotationItem);
	}

	[TestMethod]
	public void FileIndexItemTest_colorDisplayName_WinnerAlt()
	{
		var colorDisplayName = EnumHelper.GetDisplayName(ColorClassParser.Color.WinnerAlt);
		Assert.AreEqual("Winner Alt", colorDisplayName);
	}

	[TestMethod]
	public void FileIndexItemTest_MakeModel_UsingField()
	{
		var item = new FileIndexItem
		{
			MakeModel = "Apple|iPhone SE|iPhone SE back camera 4.15mm f/2.2"
		};
		Assert.AreEqual("Apple", item.Make);
		Assert.AreEqual("iPhone SE", item.Model);
		Assert.AreEqual("back camera 4.15mm f/2.2", item.LensModel);
	}

	[TestMethod]
	public void LensModel_Defaults()
	{
		var item = new FileIndexItem { MakeModel = string.Empty };
		Assert.AreEqual(string.Empty, item.LensModel);
	}

	[TestMethod]
	public void LensModel_ShouldReplace()
	{
		var item = new FileIndexItem { MakeModel = "test|Canon|Canon Lens" };
		Assert.AreEqual("Lens", item.LensModel);
	}

	[TestMethod]
	public void LensModel_ShouldNotReplace()
	{
		var item = new FileIndexItem { MakeModel = "test||Canon Lens" };
		Assert.AreEqual("Canon Lens", item.LensModel);
	}

	[TestMethod]
	public void FileIndexItemTest_MakeModel_UsingFieldNull()
	{
		var item = new FileIndexItem { MakeModel = null };
		Assert.AreEqual(string.Empty, item.Make);
	}

	[TestMethod]
	public void FileIndexItemTest_MakeModel_UsingFieldNullLensModel()
	{
		var item = new FileIndexItem { MakeModel = null };
		Assert.AreEqual(string.Empty, item.LensModel);
	}

	[TestMethod]
	public void FileIndexItemTest_MakeModel_IgnoreDashDash()
	{
		var item = new FileIndexItem { MakeModel = null };
		item.SetMakeModel("----", 0);
		Assert.AreEqual(string.Empty, item.LensModel);
	}

	[TestMethod]
	public void FileIndexItemTest_SetMakeModel_Model()
	{
		var item = new FileIndexItem();
		item.SetMakeModel("iPhone", 1);

		Assert.AreEqual("iPhone", item.Model);
		Assert.AreEqual("|iPhone|", item.MakeModel);
	}

	[TestMethod]
	public void FileIndexItemTest_SetMakeModel_Make()
	{
		var item = new FileIndexItem();
		item.SetMakeModel("APPLE", 0);

		Assert.AreEqual("Apple", item.Make);
		Assert.AreEqual("Apple||", item.MakeModel);
	}

	[TestMethod]
	public void FileIndexItemTest_SetMakeModel_MakeWrongPipeLength()
	{
		var item = new FileIndexItem { MakeModel = "Apple|||||||" };
		Assert.AreEqual(string.Empty, item.Make);
		Assert.AreEqual(string.Empty, item.Model);
	}

	[TestMethod]
	public void FileIndexItemTest_SetMakeModel_WrongOrder_MakeANDModel()
	{
		var item = new FileIndexItem();
		// the wrong order > 1,0,2
		item.SetMakeModel("iPhone", 1);
		item.SetMakeModel("iPad", 1);

		item.SetMakeModel("Apple", 0);

		item.SetMakeModel("Lens", 2);

		Assert.AreEqual("Apple|iPad|Lens", item.MakeModel);

		Assert.AreEqual("Apple", item.Make);
		Assert.AreEqual("iPad", item.Model);
	}

	[TestMethod]
	public void FileIndexItemTest_SetMakeModel_WrongPipeLength()
	{
		var item = new FileIndexItem();

		// Assert that an AggregateException is thrown when SetMakeModel is called with an invalid index
		Assert.ThrowsExactly<AggregateException>(() =>
			item.SetMakeModel("Apple", 95)); // this index (95) never exists
	}

	[TestMethod]
	public void FileIndexItemTest_SetMakeModel_RightOrder_MakeANDModel()
	{
		var item = new FileIndexItem();

		item.SetMakeModel("Apple", 0);
		item.SetMakeModel("iPhone", 1);

		Assert.AreEqual("Apple|iPhone|", item.MakeModel);

		Assert.AreEqual("Apple", item.Make);
		Assert.AreEqual("iPhone", item.Model);
	}

	[TestMethod]
	public void FileIndexItemTest_IsRelativeOrientation()
	{
		var item = FileIndexItem.IsRelativeOrientation(-1);
		Assert.IsTrue(item);

		var item2 = FileIndexItem.IsRelativeOrientation(1);
		Assert.IsTrue(item2);

		var item999 = FileIndexItem.IsRelativeOrientation(999);
		Assert.IsFalse(item999);
	}


	[TestMethod]
	public void FileIndexItemTest_Ctor_SpaceName()
	{
		var item = new FileIndexItem("/test/image with space.jpg");
		Assert.AreEqual("image with space.jpg", item.FileName);
		Assert.AreEqual("image with space", item.FileCollectionName);
		Assert.AreEqual("/test", item.ParentDirectory);
	}

	[TestMethod]
	public void SidecarExtensions_read()
	{
		var item = new FileIndexItem { SidecarExtensions = "xmp|test" };
		Assert.AreEqual("xmp", item.SidecarExtensionsList.FirstOrDefault());
	}

	[TestMethod]
	public void SidecarExtensions_read_null()
	{
		var item = new FileIndexItem { SidecarExtensions = null };
		Assert.AreEqual(0, item.SidecarExtensionsList.Count);
	}

	[TestMethod]
	public void SidecarExtensions_Add()
	{
		var item = new FileIndexItem { SidecarExtensions = "xmp" };
		item.AddSidecarExtension("xmp");

		Assert.AreEqual("xmp", item.SidecarExtensionsList.FirstOrDefault());
		// no duplicates please
		Assert.AreEqual(1, item.SidecarExtensionsList.Count);
	}

	[TestMethod]
	public void SidecarExtensions_Remove()
	{
		var item = new FileIndexItem { SidecarExtensions = "xmp" };
		item.RemoveSidecarExtension("xmp");

		Assert.AreEqual(0, item.SidecarExtensionsList.Count);
	}

	[TestMethod]
	public void SetFilePath_Home()
	{
		var item = new FileIndexItem();
		item.SetFilePath("/");

		Assert.AreEqual("/", item.FileName);
		Assert.AreEqual(string.Empty, item.ParentDirectory);
	}

	[TestMethod]
	public void SetFilePath_testFile()
	{
		var item = new FileIndexItem();
		item.SetFilePath("/test.jpg");

		Assert.AreEqual("test.jpg", item.FileName);
		Assert.AreEqual("/", item.ParentDirectory);
	}

	[TestMethod]
	public void SetFilePath_slashSlashTestFile()
	{
		var item = new FileIndexItem();
		item.SetFilePath("//test.jpg");

		Assert.AreEqual("test.jpg", item.FileName);
		Assert.AreEqual("/", item.ParentDirectory);
		Assert.AreEqual("/test.jpg", item.FilePath);
	}

	[TestMethod]
	public void SetFilePath_subFolderTestFile()
	{
		var item = new FileIndexItem();
		item.SetFilePath("/test/test.jpg");

		Assert.AreEqual("test.jpg", item.FileName);
		Assert.AreEqual("/test", item.ParentDirectory);
	}


	[TestMethod]
	public void Size_Lt_0()
	{
		var value = -1;
		var item = new FileIndexItem { Size = value };

		Assert.AreEqual(0, item.Size);
	}

	[TestMethod]
	public void Size_MinValue()
	{
		var item = new FileIndexItem { Size = 99999999999999999 };
		// overwrite here, should not be 0
		item.Size = int.MinValue;

		// should write to large values to min value
		Assert.AreEqual(0, item.Size);
	}

	[TestMethod]
	public void Size_ShouldAdd()
	{
		var value = 2;
		var item = new FileIndexItem { Size = value };

		Assert.AreEqual(2, item.Size);
	}

	[TestMethod]
	public void FixedListToString_Null()
	{
		Assert.AreEqual(string.Empty, FileIndexItem.FixedListToString(null));
	}

	[TestMethod]
	public void FixedListToString_One()
	{
		Assert.AreEqual("test", FileIndexItem.FixedListToString(new List<string> { "test" }));
	}

	[TestMethod]
	public void FixedListToString_Two()
	{
		Assert.AreEqual("test|test2",
			FileIndexItem.FixedListToString(new List<string> { "test", "test2" }));
	}
}
