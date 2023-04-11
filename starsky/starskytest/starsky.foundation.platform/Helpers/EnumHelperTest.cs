using System.ComponentModel.DataAnnotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Helpers;

namespace starskytest.starsky.foundation.platform.Helpers;

[TestClass]
public class EnumHelperTest
{
    public enum TestEnum
    {
        [Display(Name = "Test One")]
        Value1,

        [Display(Name = "Test Two")]
        Value2,

        Value3,

        [Display(Name = null)]
        Value4
    }

    [TestMethod]
    public void Test_GetDisplayName_ReturnsDisplayName_ForEnumWithDisplayName()
    {
        // Arrange
        var enumValue = TestEnum.Value1;

        // Act
        var result = EnumHelper.GetDisplayName(enumValue);

        // Assert
        Assert.AreEqual("Test One", result);
    }

    [TestMethod]
    public void Test_GetDisplayName_ReturnsEnumValue_ForEnumWithoutDisplayName()
    {
        // Arrange
        var enumValue = TestEnum.Value3;

        // Act
        var result = EnumHelper.GetDisplayName(enumValue);

        // Assert
        Assert.AreEqual(null, result);
    }

    [TestMethod]
    public void Test_GetDisplayName_ReturnsEmptyString_ForEnumWithNullDisplayName()
    {
        // Arrange
        var enumValue = TestEnum.Value4;

        // Act
        var result = EnumHelper.GetDisplayName(enumValue);

        // Assert
        Assert.AreEqual(null, result);
    }

    [TestMethod]
    public void Test_GetDisplayName_ReturnsEmptyString_ForNullEnum()
    {
        // Arrange
        TestEnum? enumValue = null;

        // Act
        var result = EnumHelper.GetDisplayName(enumValue);

        // Assert
        Assert.AreEqual(null, result);
    }

    [TestMethod]
    public void Test_GetDisplayName_ReturnsDisplayName_ForNullableEnumWithDisplayName()
    {
        // Arrange
        TestEnum? enumValue = TestEnum.Value1;

        // Act
        var result = EnumHelper.GetDisplayName(enumValue);

        // Assert
        Assert.AreEqual("Test One", result);
    }

    [TestMethod]
    public void Test_GetDisplayName_ReturnsEnumValue_ForNullableEnumWithoutDisplayName()
    {
        // Arrange
        TestEnum? enumValue = TestEnum.Value3;

        // Act
        var result = EnumHelper.GetDisplayName(enumValue);

        // Assert
        Assert.AreEqual(null, result);
    }

    [TestMethod]
    public void Test_GetDisplayName_ReturnsEmptyString_ForNullableEnumWithNullDisplayName()
    {
        // Arrange
        TestEnum? enumValue = TestEnum.Value4;

        // Act
        var result = EnumHelper.GetDisplayName(enumValue);

        // Assert
        Assert.AreEqual(null, result);
    }

    [TestMethod]
    public void Test_GetDisplayName_ReturnsEmptyString_ForNullableEnumNull()
    {
        // Arrange
        TestEnum? enumValue = null;

        // Act
        var result = EnumHelper.GetDisplayName(enumValue);

        // Assert
        Assert.AreEqual(null, result);
    }
}
