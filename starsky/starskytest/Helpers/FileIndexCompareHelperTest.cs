using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;

namespace starskytest.Helpers
{
	[TestClass]
	public sealed class FileIndexCompareHelperTest
	{
		[TestMethod]
		public void FileIndexCompareHelperTest_UpdateNull()
		{
			var source = new FileIndexItem { Tags = "hi" };
			var changes = FileIndexCompareHelper.Compare(source);
			Assert.AreEqual(0, changes.Count);
		}

		[TestMethod]
		public void FileIndexCompareHelperTest_String_Compare()
		{
			var source = new FileIndexItem { Tags = "hi" };
			var update = new FileIndexItem { Tags = "update" };
			FileIndexCompareHelper.Compare(source, update);
			Assert.AreEqual("update", source.Tags);
		}

		[TestMethod]
		public void FileIndexCompareHelperTest_StringList_Compare()
		{
			var source = new FileIndexItem { CollectionPaths = new List<string> { "source" } };
			var update = new FileIndexItem { CollectionPaths = new List<string> { "update" } };
			FileIndexCompareHelper.Compare(source, update);
			Assert.AreEqual("update", source.CollectionPaths[0]);
		}

		[TestMethod]
		public void FileIndexCompareHelperTest_String_Tags_AppendCompare()
		{
			var source = new FileIndexItem { Tags = "hi" };
			var update = new FileIndexItem { Tags = "update" };
			FileIndexCompareHelper.Compare(source, update, true);
			Assert.AreEqual("hi, update", source.Tags);
		}

		[TestMethod]
		public void FileIndexCompareHelperTest_String_Description_AppendCompare()
		{
			var source = new FileIndexItem { Description = "hi" };
			var update = new FileIndexItem { Description = "update" };
			FileIndexCompareHelper.Compare(source, update, true);
			Assert.AreEqual("hi update", source.Description);
		}

		[TestMethod]
		public void FileIndexCompareHelperTest_colorClass_Compare()
		{
			var source = new FileIndexItem { ColorClass = ColorClassParser.Color.None };
			var update = new FileIndexItem { ColorClass = ColorClassParser.Color.Winner };
			FileIndexCompareHelper.Compare(source, update);
			Assert.AreEqual(ColorClassParser.Color.Winner, source.ColorClass);
		}

		[TestMethod]
		public void FileIndexCompareHelperTest_bool_Compare()
		{
			var source = new FileIndexItem { IsDirectory = false };
			var update = new FileIndexItem { IsDirectory = true };
			FileIndexCompareHelper.Compare(source, update);
			Assert.IsTrue(source.IsDirectory);
		}

		[TestMethod]
		public void FileIndexCompareHelperTest_DateTimeNoOverwrite_Compare()
		{
			// so no overwrite
			var source = new FileIndexItem { DateTime = DateTime.Now };
			var update = new FileIndexItem { DateTime = new DateTime() };
			FileIndexCompareHelper.Compare(source, update);
			Assert.AreNotEqual(update.DateTime, source.DateTime);
		}

		[TestMethod]
		public void FileIndexCompareHelperTest_DateTime_Compare()
		{
			// source= null> update is new overwrite
			var source = new FileIndexItem { DateTime = new DateTime() };
			var update = new FileIndexItem { DateTime = DateTime.Now };
			FileIndexCompareHelper.Compare(source, update);
			Assert.AreEqual(update.DateTime, source.DateTime);
		}

		[TestMethod]
		public void FileIndexCompareHelperTest_ushort_Compare()
		{
			var source = new FileIndexItem { IsoSpeed = 0 };
			var update = new FileIndexItem { IsoSpeed = 1 };
			FileIndexCompareHelper.Compare(source, update);
			Assert.AreEqual(1, source.IsoSpeed);
		}

		[TestMethod]
		public void FileIndexCompareHelperTest_double_Compare()
		{
			var source = new FileIndexItem { Aperture = 0 };
			var update = new FileIndexItem { Aperture = 1 };
			FileIndexCompareHelper.Compare(source, update);
			Assert.AreEqual(1, source.Aperture);
		}

		[TestMethod]
		public void FileIndexCompareHelperTest_Rotation_Compare()
		{
			var source = new FileIndexItem { Orientation = FileIndexItem.Rotation.Horizontal };
			var update = new FileIndexItem { Orientation = FileIndexItem.Rotation.Rotate90Cw };
			FileIndexCompareHelper.Compare(source, update);
			Assert.AreEqual(FileIndexItem.Rotation.Rotate90Cw, source.Orientation);
		}

		[TestMethod]
		public void FileIndexCompareHelperTest_SetCompare()
		{
			var source = new FileIndexItem { DateTime = new DateTime() };
			var update = new FileIndexItem { DateTime = DateTime.Now };
			var result =
				FileIndexCompareHelper.SetCompare(source, update, new List<string> { "DateTime" });
			Assert.AreEqual(update.DateTime, result.DateTime);
		}

		[TestMethod]
		public void FileIndexCompareHelperTest_FilePath_Compare()
		{
			// this one is ignored
			var source = new FileIndexItem { FilePath = "/test" };
			var update = new FileIndexItem { FilePath = "/ignore" };
			FileIndexCompareHelper.Compare(source, update);
			Assert.AreEqual("/test", source.FilePath);
		}


		[TestMethod]
		public void FileIndexCompareHelperTest_ImageStabilisationType_Compare_NotUpdate()
		{
			var source = new FileIndexItem { ImageStabilisation = ImageStabilisationType.Off };
			var update = new FileIndexItem { ImageStabilisation = ImageStabilisationType.Unknown };
			FileIndexCompareHelper.Compare(source, update);
			Assert.AreEqual(ImageStabilisationType.Off, source.ImageStabilisation);
		}

		[TestMethod]
		public void FileIndexCompareHelperTest_LastChanged_Compare_Update()
		{
			var source = new FileIndexItem { LastChanged = new List<string>() };
			var update = new FileIndexItem { LastChanged = new List<string> { "test" } };
			FileIndexCompareHelper.Compare(source,
				update); // it element is updated, but the list not
			Assert.AreEqual(1, source.LastChanged.Count);
		}

		[TestMethod]
		public void FileIndexCompareHelperTest_LastChanged_Compare_RemoveFromList()
		{
			var source = new FileIndexItem { LastChanged = new List<string>() };
			var update = new FileIndexItem { LastChanged = new List<string> { "test" } };
			var result = FileIndexCompareHelper.Compare(source, update);
			Assert.AreEqual(0, result.Count);
		}

		[TestMethod]
		public void FileIndexCompareHelperTest_ImageStabilisationType_Compare_Equal_NotUpdate()
		{
			var source = new FileIndexItem { ImageStabilisation = ImageStabilisationType.Off };
			var update = new FileIndexItem { ImageStabilisation = ImageStabilisationType.Off };
			FileIndexCompareHelper.Compare(source, update);
			Assert.AreEqual(ImageStabilisationType.Off, source.ImageStabilisation);
		}

		[TestMethod]
		public void FileIndexCompareHelperTest_ImageStabilisationType_Compare_ShouldUpdate()
		{
			var source = new FileIndexItem { ImageStabilisation = ImageStabilisationType.Unknown };
			var update = new FileIndexItem { ImageStabilisation = ImageStabilisationType.Off };
			FileIndexCompareHelper.Compare(source, update);
			Assert.AreEqual(ImageStabilisationType.Off, source.ImageStabilisation);
		}

		[TestMethod]
		public void FileIndexCompareHelperTest__CheckIfPropertyExist_Tags_True()
		{
			Assert.IsTrue(FileIndexCompareHelper.CheckIfPropertyExist(nameof(FileIndexItem.Tags)));
		}

		[TestMethod]
		public void FileIndexCompareHelperTest__CheckIfPropertyExist_False()
		{
			Assert.IsFalse(FileIndexCompareHelper.CheckIfPropertyExist("45678987654"));
		}

		[TestMethod]
		public void FileIndexCompareHelperTest__SetValue_Tags()
		{
			Assert.AreEqual("value",
				FileIndexCompareHelper.Set(null, nameof(FileIndexItem.Tags), "value").Tags);
		}

		[TestMethod]
		public void FileIndexCompareHelperTest__SetValue_Tags_LowerCase()
		{
			Assert.AreEqual("value",
				FileIndexCompareHelper
					.Set(null, nameof(FileIndexItem.Tags).ToLowerInvariant(), "value").Tags);
		}

		[TestMethod]
		public void FileIndexCompareHelperTest__SetValue_UnknownValue()
		{
			// try database type that does not exist
			Assert.AreEqual(string.Empty,
				FileIndexCompareHelper.Set(null, "ThisTagDoesNotExist", "value").Tags);
		}

		[TestMethod]
		public void FileIndexCompareHelperTest__SetValue_WrongTypeCast()
		{
			// wrong types are ignored by default
			Assert.AreEqual(string.Empty,
				FileIndexCompareHelper.Set(null, nameof(FileIndexItem.Tags), 1).Tags);
		}

		[TestMethod]
		public void FileIndexCompareHelperTest_GetValue()
		{
			var t = new FileIndexItem { Tags = "test" };
			var result = FileIndexCompareHelper.Get(t, nameof(FileIndexItem.Tags));
			Assert.AreEqual(t.Tags, result);
		}

		[TestMethod]
		public void FileIndexCompareHelperTest_GetValue_LowerCase()
		{
			var t = new FileIndexItem { Tags = "test" };
			var result =
				FileIndexCompareHelper.Get(t, nameof(FileIndexItem.Tags).ToLowerInvariant());
			Assert.AreEqual(t.Tags, result);
		}

		[TestMethod]
		public void FileIndexCompareHelperTest_GetValue_NullFieldName()
		{
			var t = new FileIndexItem { Tags = "test" };
			var result = FileIndexCompareHelper.Get(t, "ThisTagDoesNotExist");
			Assert.AreEqual(null, result);
		}

		[TestMethod]
		public void FileIndexCompareHelperTest_GetValue_NullFileIndexItem()
		{
			var result = FileIndexCompareHelper.Get(null, nameof(FileIndexItem.Tags));
			Assert.AreEqual(null, result);
		}

		[TestMethod]
		public void CompareRotation_NotFound()
		{
			var list = new List<string>();
			FileIndexCompareHelper.CompareRotation("t",
				new FileIndexItem(),
				FileIndexItem.Rotation.Horizontal,
				FileIndexItem.Rotation.Horizontal, list);
			Assert.IsNotNull(list);
		}

		[TestMethod]
		public void CompareDouble_NotFound()
		{
			var list = new List<string>();
			FileIndexCompareHelper.CompareDouble("t",
				new FileIndexItem(),
				0d,
				0d, list);
			Assert.IsNotNull(list);
		}


		[TestMethod]
		public void CompareUShort_NotFound()
		{
			var list = new List<string>();
			FileIndexCompareHelper.CompareUshort("t",
				new FileIndexItem(),
				0,
				0, list);
			Assert.IsNotNull(list);
		}

		[TestMethod]
		public void CompareImageFormat_NotFound()
		{
			var list = new List<string>();
			FileIndexCompareHelper.CompareImageFormat("t",
				new FileIndexItem(),
				ExtensionRolesHelper.ImageFormat.bmp,
				ExtensionRolesHelper.ImageFormat.bmp, list);
			Assert.IsNotNull(list);
		}

		[TestMethod]
		public void CompareDateTime_NotFound()
		{
			var list = new List<string>();
			FileIndexCompareHelper.CompareDateTime("t",
				new FileIndexItem(),
				DateTime.Now,
				DateTime.Now, list);
			Assert.IsNotNull(list);
		}

		[TestMethod]
		public void CompareColor_NotFound()
		{
			var list = new List<string>();
			FileIndexCompareHelper.CompareColor("t",
				new FileIndexItem(),
				ColorClassParser.Color.Winner,
				ColorClassParser.Color.Winner, list);
			Assert.IsNotNull(list);
		}


		[TestMethod]
		public void CompareListString_NotFound()
		{
			var list = new List<string>();
			FileIndexCompareHelper.CompareListString("t",
				new FileIndexItem(),
				new List<string> { "1" },
				new List<string> { "1" }, list);
			Assert.IsNotNull(list);
		}

		[TestMethod]
		[SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
		public void CompareNullableBool_NotFound()
		{
			var list = new List<string>();
			bool? boolValue = true;
			FileIndexCompareHelper.CompareNullableBool("t",
				new FileIndexItem(),
				boolValue,
				boolValue, list);
			Assert.IsNotNull(list);
		}

		private static void SetNull(object obj, string propertyName)
		{
			var propertyInfo = obj.GetType().GetProperty(propertyName);
			propertyInfo?.SetValue(obj, null);
		}

		[TestMethod]
		public void CompareString_NotFound()
		{
			var list = new List<string>();
			FileIndexCompareHelper.CompareString("t",
				new FileIndexItem(),
				"Test",
				"test", list, true);
			Assert.IsNotNull(list);
		}

		[TestMethod]
		public void TestCompare_NullColor()
		{
			// Arrange
			var sourceIndexItem = new FileIndexItem();
			var updateObject = new FileIndexItem();

			SetNull(sourceIndexItem, "ColorClass");

			// Act
			var result = FileIndexCompareHelper.Compare(sourceIndexItem, updateObject);

			// Assert
			CollectionAssert.DoesNotContain(result, "ColorClass");
		}

		[TestMethod]
		public void TestCompare_NullDateTime()
		{
			// Arrange
			var sourceIndexItem = new FileIndexItem();
			var updateObject = new FileIndexItem();
			SetNull(sourceIndexItem, "DateTime");

			// Act
			var result = FileIndexCompareHelper.Compare(sourceIndexItem, updateObject);

			// Assert
			CollectionAssert.DoesNotContain(result, "DateTime");
		}

		[TestMethod]
		public void TestCompare_NullRotation()
		{
			// Arrange
			var sourceIndexItem = new FileIndexItem();
			var updateObject = new FileIndexItem();
			sourceIndexItem.Orientation = FileIndexItem.Rotation.DoNotChange;

			// Act
			var result = FileIndexCompareHelper.Compare(sourceIndexItem, updateObject);

			// Assert
			CollectionAssert.DoesNotContain(result, "Orientation");
		}

		[TestMethod]
		public void TestCompare_NullImageStabilisationType()
		{
			// Arrange
			var sourceIndexItem = new FileIndexItem();
			var updateObject = new FileIndexItem();
			sourceIndexItem.ImageStabilisation = ImageStabilisationType.Unknown;

			// Act
			var result = FileIndexCompareHelper.Compare(sourceIndexItem, updateObject);

			// Assert
			CollectionAssert.DoesNotContain(result, "ImageStabilisation");
		}

		[TestMethod]
		public void TestCompare_NullDouble()
		{
			// Arrange
			var sourceIndexItem = new FileIndexItem();
			var updateObject = new FileIndexItem();
			sourceIndexItem.Aperture = 0;

			// Act
			var result = FileIndexCompareHelper.Compare(sourceIndexItem, updateObject);

			// Assert
			CollectionAssert.DoesNotContain(result, "Aperture");
		}

		[TestMethod]
		public void TestCompare_NullUshort()
		{
			// Arrange
			var sourceIndexItem = new FileIndexItem();
			var updateObject = new FileIndexItem();
			SetNull(sourceIndexItem, "IsoSpeed");

			// Act
			var result = FileIndexCompareHelper.Compare(sourceIndexItem, updateObject);

			// Assert
			CollectionAssert.DoesNotContain(result, "IsoSpeed");
		}

		[TestMethod]
		public void TestCompare_NullImageFormat()
		{
			// Arrange
			var sourceIndexItem = new FileIndexItem();
			var updateObject = new FileIndexItem();
			sourceIndexItem.ImageFormat = ExtensionRolesHelper.ImageFormat.unknown;

			// Act
			var result = FileIndexCompareHelper.Compare(sourceIndexItem, updateObject);

			// Assert
			CollectionAssert.DoesNotContain(result, "ImageFormat");
		}
	}
}
