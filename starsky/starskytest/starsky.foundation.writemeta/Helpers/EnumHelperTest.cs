using System.ComponentModel.DataAnnotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.writemeta.Helpers;

namespace starskytest.starsky.foundation.writemeta.Helpers;

[TestClass]
public class EnumHelperTest
{
	public enum TestValue
	{
		[Display(Name = "Test One")] Value1,

		[Display(Name = "Test Two")] Value2,

		Value3,

		[Display(Name = null)] Value4
	}

	[TestMethod]
	public void Test_GetDisplayName_ReturnsDisplayName_ForEnumWithDisplayName()
	{
		// Arrange
		var enumValue = TestValue.Value1;

		// Act
		var result = EnumHelper.GetDisplayName(enumValue);

		// Assert
		Assert.AreEqual("Test One", result);
	}

	[TestMethod]
	public void Test_GetDisplayName_ReturnsEnumValue_ForEnumWithoutDisplayName()
	{
		// Arrange
		var enumValue = TestValue.Value3;

		// Act
		var result = EnumHelper.GetDisplayName(enumValue);

		// Assert
		Assert.AreEqual(null, result);
	}

	[TestMethod]
	public void Test_GetDisplayName_ReturnsEmptyString_ForEnumWithNullDisplayName()
	{
		// Arrange
		var enumValue = TestValue.Value4;

		// Act
		var result = EnumHelper.GetDisplayName(enumValue);

		// Assert
		Assert.AreEqual(null, result);
	}

	[TestMethod]
	public void Test_GetDisplayName_ReturnsEmptyString_ForNullEnum()
	{
		// Arrange
		TestValue? enumValue = null;

		// Act
		var result = EnumHelper.GetDisplayName(enumValue!);

		// Assert
		Assert.AreEqual(null, result);
	}

	[TestMethod]
	public void Test_GetDisplayName_ReturnsDisplayName_ForNullableEnumWithDisplayName()
	{
		// Arrange
		TestValue? enumValue = TestValue.Value1;

		// Act
		var result = EnumHelper.GetDisplayName(enumValue);

		// Assert
		Assert.AreEqual("Test One", result);
	}

	[TestMethod]
	public void Test_GetDisplayName_ReturnsEnumValue_ForNullableEnumWithoutDisplayName()
	{
		// Arrange
		TestValue? enumValue = TestValue.Value3;

		// Act
		var result = EnumHelper.GetDisplayName(enumValue);

		// Assert
		Assert.AreEqual(null, result);
	}

	[TestMethod]
	public void Test_GetDisplayName_ReturnsEmptyString_ForNullableEnumWithNullDisplayName()
	{
		// Arrange
		TestValue? enumValue = TestValue.Value4;

		// Act
		var result = EnumHelper.GetDisplayName(enumValue);

		// Assert
		Assert.AreEqual(null, result);
	}
}
