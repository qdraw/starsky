using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.injection;

namespace starskytest.starsky.foundation.injection;

public interface ITestInjectionClass
{
	public bool Enabled { get; set; }
}

[TestClass]
public class ServiceCollectionExtensionsTest
{
	public class TestInjectionClass : ITestInjectionClass
	{
		public bool Enabled { get; set; } = true;
	}
	
	private class OverwriteInjectionLifetime
	{
		public InjectionLifetime Type { get; set; }
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
	public void Add_LifeTime_Scope()
	{
		var serviceCollection = new ServiceCollection() as IServiceCollection;
		serviceCollection.Add(InjectionLifetime.Scoped, typeof(TestInjectionClass));

		var serviceDescriptor = serviceCollection.FirstOrDefault(p => p.ServiceType == typeof(TestInjectionClass));
		Assert.AreEqual(ServiceLifetime.Scoped, serviceDescriptor?.Lifetime);
		
		var serviceProvider = serviceCollection.BuildServiceProvider();
		
		var result = serviceProvider.GetService<TestInjectionClass>();
		Assert.AreEqual(true, result?.Enabled);
	}
	
	
	[TestMethod]
	public void Add_LifeTime_Singleton()
	{
		var serviceCollection = new ServiceCollection() as IServiceCollection;
		serviceCollection.Add(InjectionLifetime.Singleton, typeof(TestInjectionClass));
		
		var serviceDescriptor = serviceCollection.FirstOrDefault(p => p.ServiceType == typeof(TestInjectionClass));
		Assert.AreEqual(ServiceLifetime.Singleton, serviceDescriptor?.Lifetime);
		
		var serviceProvider = serviceCollection.BuildServiceProvider();
		
		var result = serviceProvider.GetService<TestInjectionClass>();
		Assert.AreEqual(true, result?.Enabled);
	}

	[TestMethod]
	public void Add_LifeTime_Transient()
	{
		var serviceCollection = new ServiceCollection() as IServiceCollection;
		serviceCollection.Add(InjectionLifetime.Transient, typeof(TestInjectionClass));

		var serviceDescriptor = serviceCollection.FirstOrDefault(p => p.ServiceType == typeof(TestInjectionClass));
		Assert.AreEqual(ServiceLifetime.Transient, serviceDescriptor?.Lifetime);
		
		var serviceProvider = serviceCollection.BuildServiceProvider();
		
		var result = serviceProvider.GetService<TestInjectionClass>();
		Assert.AreEqual(true, result?.Enabled);
	}

	[TestMethod]
	[ExpectedException(typeof(ArgumentOutOfRangeException))]
	public void Add_LifeTime_InvalidType()
	{
		var overwriteInjectionLifetime = new OverwriteInjectionLifetime();
		var propertyObject = overwriteInjectionLifetime.GetType().GetProperty("Type");
		propertyObject?.SetValue(overwriteInjectionLifetime, 44, null); // <-- this could not happen
		
		var serviceCollection = new ServiceCollection() as IServiceCollection;
		serviceCollection.Add(overwriteInjectionLifetime.Type, typeof(TestInjectionClass));
		// expect exception
	}
	
	
	[TestMethod]
	public void Add_implementationType_LifeTime_Scope()
	{
		var serviceCollection = new ServiceCollection() as IServiceCollection;
		serviceCollection.Add(typeof(ITestInjectionClass), 
			typeof(TestInjectionClass),InjectionLifetime.Scoped);

		var serviceDescriptor = serviceCollection.FirstOrDefault(p => p.ServiceType == typeof(ITestInjectionClass));
		Assert.AreEqual(ServiceLifetime.Scoped, serviceDescriptor?.Lifetime);
		
		var serviceProvider = serviceCollection.BuildServiceProvider();
		
		var result = serviceProvider.GetService<ITestInjectionClass>();
		Assert.AreEqual(true, result?.Enabled);
	}
	
	
	[TestMethod]
	public void Add_implementationType_LifeTime_Singleton()
	{
		var serviceCollection = new ServiceCollection() as IServiceCollection;
		serviceCollection.Add(typeof(ITestInjectionClass), 
			typeof(TestInjectionClass),InjectionLifetime.Singleton);

		var serviceDescriptor = serviceCollection.FirstOrDefault(p => p.ServiceType == typeof(ITestInjectionClass));
		Assert.AreEqual(ServiceLifetime.Singleton, serviceDescriptor?.Lifetime);
		
		var serviceProvider = serviceCollection.BuildServiceProvider();
		
		var result = serviceProvider.GetService<ITestInjectionClass>();
		Assert.AreEqual(true, result?.Enabled);
	}
	
	[TestMethod]
	public void Add_implementationType_LifeTime_Transient()
	{
		var serviceCollection = new ServiceCollection() as IServiceCollection;
		serviceCollection.Add(typeof(ITestInjectionClass), 
			typeof(TestInjectionClass),InjectionLifetime.Transient);

		var serviceDescriptor = serviceCollection.FirstOrDefault(p => p.ServiceType == typeof(ITestInjectionClass));
		Assert.AreEqual(ServiceLifetime.Transient, serviceDescriptor?.Lifetime);
		
		var serviceProvider = serviceCollection.BuildServiceProvider();
		
		var result = serviceProvider.GetService<ITestInjectionClass>();
		Assert.AreEqual(true, result?.Enabled);
	}
	
	[TestMethod]
	[ExpectedException(typeof(ArgumentOutOfRangeException))]
	public void Add_implementationType_LifeTime_InvalidType()
	{
		var overwriteInjectionLifetime = new OverwriteInjectionLifetime();
		var propertyObject = overwriteInjectionLifetime.GetType().GetProperty("Type");
		propertyObject?.SetValue(overwriteInjectionLifetime, 44, null); // <-- this could not happen
		
		var serviceCollection = new ServiceCollection() as IServiceCollection;
		serviceCollection.Add(typeof(ITestInjectionClass), 
			typeof(TestInjectionClass),overwriteInjectionLifetime.Type);
		// expect exception
	}
	

}
