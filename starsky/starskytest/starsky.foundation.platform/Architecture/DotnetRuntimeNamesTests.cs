using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Architecture;

namespace starskytest.starsky.foundation.platform.Architecture;

[TestClass]
public class DotnetRuntimeNamesTests
{
	[TestMethod]
	public void GetArchitecturesNoGenericAndFallback_ShouldAddCurrentRuntime_WhenEmpty()
	{
		// Arrange
		var architectures = new List<string>();

		// Act
		var result = DotnetRuntimeNames.GetArchitecturesNoGenericAndFallback(architectures)
			.ToList();

		// Assert
		Assert.HasCount(1, result);
		Assert.AreEqual(CurrentArchitecture.GetCurrentRuntimeIdentifier(), result[0]);
	}

	[TestMethod]
	public void GetArchitecturesNoGenericAndFallback_ShouldSkipGenericRuntimeName()
	{
		// Arrange
		var architectures = new List<string> { DotnetRuntimeNames.GenericRuntimeName };

		// Act
		var result = DotnetRuntimeNames.GetArchitecturesNoGenericAndFallback(architectures);

		// Assert
		CollectionAssert.AreEqual(new List<string>(), result.ToList());
	}

	[TestMethod]
	public void GetArchitecturesNoGenericAndFallback_ShouldFilterOutGenericRuntime()
	{
		// Arrange
		var architectures = new List<string>
		{
			DotnetRuntimeNames.GenericRuntimeName, "win-x64", "linux-arm64"
		};

		// Act
		var result = DotnetRuntimeNames.GetArchitecturesNoGenericAndFallback(architectures);

		// Assert
		CollectionAssert.AreEqual(new List<string> { "win-x64", "linux-arm64" }, result.ToList());
	}

	[TestMethod]
	public void GetArchitecturesNoGenericAndFallback_ShouldReturnOriginalList_WhenNoGenericRuntime()
	{
		// Arrange
		var architectures = new List<string> { "win-x64", "linux-arm64" };

		// Act
		var result = DotnetRuntimeNames.GetArchitecturesNoGenericAndFallback(architectures);

		// Assert
		CollectionAssert.AreEqual(architectures, result.ToList());
	}
}
