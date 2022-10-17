using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.storage.Storage;

namespace starskytest.starsky.foundation.storage.Storage;

[TestClass]
public class SelectorStorageTest
{
	private readonly IServiceProvider _serviceProvider;

	public SelectorStorageTest()
	{
		var serviceCollection = new ServiceCollection();
		var sp = serviceCollection.BuildServiceProvider();
		_serviceProvider = sp.GetRequiredService<IServiceProvider>();
	}
	
	/// <summary>
	/// Used to overwrite with reflection
	/// </summary>
	private class MyClass
	{
		public SelectorStorage.StorageServices Type { get; set; }
	}
	
	[TestMethod]
	[ExpectedException(typeof(ArgumentOutOfRangeException))]
	public void Get_ArgumentOutOfRangeException()
	{
		var myClass = new MyClass();
		var propertyObject = myClass.GetType().GetProperty("Type");
		propertyObject?.SetValue(myClass, 44, null); // <-- this could not happen
		
		new SelectorStorage(_serviceProvider).Get(myClass.Type);
		// expect exception
	}
}
