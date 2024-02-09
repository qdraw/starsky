using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;

namespace starskytest.starsky.foundation.database.Data;

[TestClass]
public class DbContextFactoryTests
{
	[TestMethod]
	public void TestCreateDbContext()
	{
		// Arrange
		var factory = new DbContextFactory();
		var args = Array.Empty<string>();

		// Act
		var context = factory.CreateDbContext(args);

		// Assert
		Assert.IsNotNull(context);
		Assert.IsInstanceOfType(context, typeof(ApplicationDbContext));
	}
}
