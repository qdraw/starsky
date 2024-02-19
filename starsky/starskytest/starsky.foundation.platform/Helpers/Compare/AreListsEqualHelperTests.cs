using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Helpers.Compare;

namespace starskytest.starsky.foundation.platform.Helpers.Compare;

[TestClass]
public class AreListsEqualHelperTests
{
	[TestMethod]
	public void AreListsEqual_SameLists_ReturnsTrue()
	{
		// Arrange
		var list1 = new List<int> { 1, 2, 3 };
		var list2 = new List<int> { 1, 2, 3 };

		// Act
		var result = AreListsEqualHelper.AreListsEqual(list1, list2);

		// Assert
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void AreListsEqual_DifferentLists_ReturnsFalse()
	{
		// Arrange
		var list1 = new List<int> { 1, 2, 3 };
		var list2 = new List<int> { 1, 2, 4 };

		// Act
		var result = AreListsEqualHelper.AreListsEqual(list1, list2);

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void AreListsEqual_DifferentCountLists_ReturnsFalse()
	{
		// Arrange
		var list1 = new List<int> { 1, 2, 3, 4 };
		var list2 = new List<int> { 1, 4 };

		// Act
		var result = AreListsEqualHelper.AreListsEqual(list1, list2);

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	[ExpectedException(typeof(ArgumentNullException))]
	public void AreListsEqual_NullLists_ArgumentNullException()
	{
		// Arrange
		List<int> list1 = null!;
		List<int> list2 = null!;

		// Act
		var result = AreListsEqualHelper.AreListsEqual(list1, list2);

		// Assert
		Assert.IsTrue(result);
	}

	[TestMethod]
	[ExpectedException(typeof(ArgumentNullException))]
	public void AreListsEqual_OneListNull_ArgumentNullException()
	{
		// Arrange
		var list1 = new List<int> { 1, 2, 3 };
		List<int>? list2 = null;

		// Act
		var result = AreListsEqualHelper.AreListsEqual(list1, list2!);

		// Assert
		Assert.IsFalse(result);
	}
}
