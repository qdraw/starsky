using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.injection;

namespace starskytest.starsky.foundation.injection;

[TestClass]
public class ServiceCollectionExtensionsTest
{
	public class MyClass
	{
		public bool Enabled { get; set; } = true;
	}
	
	[TestMethod]
	[ExpectedException(typeof(ArgumentNullException))]
	public void Add_LifeTimeNull()
	{
		var serviceCollection = new ServiceCollection() as IServiceCollection;
		serviceCollection.Add(InjectionLifetime.Scoped, null!);
	}
	
	[TestMethod]
	[ExpectedException(typeof(ArgumentNullException))]
	public void Add_LifeTimeNull2()
	{
		var serviceCollection = new ServiceCollection() as IServiceCollection;
		serviceCollection.Add(InjectionLifetime.Scoped, null!, null!);
	}

	[TestMethod]
	public void Add_LifeTime()
	{
		var serviceCollection = new ServiceCollection() as IServiceCollection;
		serviceCollection.Add(InjectionLifetime.Scoped, typeof(MyClass));

		serviceCollection.Where(p => p.ServiceType == typeof(MyClass)).ToList()
			.ForEach(p => Assert.AreEqual(ServiceLifetime.Scoped, p.Lifetime));
		
		var serviceProvider = serviceCollection.BuildServiceProvider();
		
		var result = serviceProvider.GetService<MyClass>();
		Assert.AreEqual(true, result?.Enabled);
	}
	
	
	[TestMethod]
	public void Add_LifeTime_Singleton()
	{
		var serviceCollection = new ServiceCollection() as IServiceCollection;
		serviceCollection.Add(InjectionLifetime.Singleton, typeof(MyClass));

		serviceCollection.Where(p => p.ServiceType == typeof(MyClass)).ToList()
			.ForEach(p => Assert.AreEqual(ServiceLifetime.Singleton, p.Lifetime));
		
		var serviceProvider = serviceCollection.BuildServiceProvider();
		
		var result = serviceProvider.GetService<MyClass>();
		Assert.AreEqual(true, result?.Enabled);
	}

	[TestMethod]
	public void Add_LifeTime_Transient()
	{
		var serviceCollection = new ServiceCollection() as IServiceCollection;
		serviceCollection.Add(InjectionLifetime.Transient, typeof(MyClass));

		serviceCollection.Where(p => p.ServiceType == typeof(MyClass)).ToList()
			.ForEach(p => Assert.AreEqual(ServiceLifetime.Transient, p.Lifetime));
		
		var serviceProvider = serviceCollection.BuildServiceProvider();
		
		var result = serviceProvider.GetService<MyClass>();
		Assert.AreEqual(true, result?.Enabled);
	}
	
	private class MyClass2
	{
		public InjectionLifetime Type { get; set; }
	}
	
	[TestMethod]
	[ExpectedException(typeof(ArgumentOutOfRangeException))]
	public void Add_LifeTime_InvalidType()
	{
		var myClass = new MyClass2();
		var propertyObject = myClass.GetType().GetProperty("Type");
		propertyObject?.SetValue(myClass, 44, null); // <-- this could not happen
		
		var serviceCollection = new ServiceCollection() as IServiceCollection;
		serviceCollection.Add(myClass.Type, typeof(MyClass));
		// expect exception
	}
	

}
