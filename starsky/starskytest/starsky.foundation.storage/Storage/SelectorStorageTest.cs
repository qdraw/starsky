using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.storage.Storage;

namespace starskytest.starsky.foundation.storage.Storage;

[TestClass]
public class SelectorStorageTest
{
	/// <summary>
	///     Service Provider
	/// </summary>
	private readonly IServiceProvider _serviceProvider;

	public SelectorStorageTest()
	{
		var serviceCollection = new ServiceCollection();
		var sp = serviceCollection.BuildServiceProvider();
		_serviceProvider = sp.GetRequiredService<IServiceProvider>();
	}

	[TestMethod]
	public void Get_ArgumentOutOfRangeException()
	{
		// Arrange
		var myClass = new MyClass();
		var propertyObject = myClass.GetType().GetProperty("Type");

		// Set an invalid value that should trigger an exception
		propertyObject?.SetValue(myClass, 44, null);

		// Act & Assert
		Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
		{
			new SelectorStorage(_serviceProvider).Get(myClass.Type);
		});
	}

	/// <summary>
	///     Used to overwrite with reflection
	/// </summary>
	[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
	[SuppressMessage("Usage",
		"S1144:Unused private types or members should be removed")]
	[SuppressMessage("Usage", "S3459:Unassigned members should be removed")]
	private class MyClass
	{
		public SelectorStorage.StorageServices Type { get; set; }
	}
}
