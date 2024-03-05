using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
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
	private class TestInjectionClass : ITestInjectionClass
	{
		public bool Enabled { get; set; } = true;
	}
	
	private class OverwriteInjectionLifetime
	{
		[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")] 
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
		Assert.IsTrue(result?.Enabled);
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
		Assert.IsTrue(result?.Enabled);
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
		Assert.IsTrue(result?.Enabled);
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
		Assert.IsTrue(result?.Enabled);
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
		Assert.IsTrue(result?.Enabled);
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
		Assert.IsTrue(result?.Enabled);
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

	[TestMethod]
	public void GetExportedTypes_Default()
	{
		var result = ServiceCollectionExtensions.GetExportedTypes(typeof(TestInjectionClass).Assembly);
		var count = result.Count();
		Assert.IsTrue(count >= 100);
	}
	
	public class AssemblyTestClass : Assembly
	{
		private readonly Exception _exception;

		public AssemblyTestClass(Exception exception)
		{
			_exception = exception;
		}

		public override string FullName { get; } = "test";
		
		public override Type[] GetExportedTypes()
		{
			throw _exception;
		}
	}
	
	[TestMethod]
	public void GetExportedTypes_NotSupportedException()
	{
		var result = ServiceCollectionExtensions.GetExportedTypes(new AssemblyTestClass(new NotSupportedException()));
		Assert.AreEqual(Type.EmptyTypes,result);
	}
	
	[TestMethod]
	public void GetExportedTypes_FileLoadException()
	{
		var result = ServiceCollectionExtensions.GetExportedTypes(new AssemblyTestClass(new FileLoadException()));
		Assert.AreEqual(Type.EmptyTypes,result);
	}
	
	[TestMethod]
	public void GetExportedTypes_ReflectionTypeLoadException()
	{
		var exception = new ReflectionTypeLoadException([typeof(bool), typeof(byte), null!
			],
			[null!, new Exception()]);
		var result = ServiceCollectionExtensions.GetExportedTypes(new AssemblyTestClass(exception));
		var count = result.Count();

		Assert.AreEqual(2,count);
	}
	
		
	[TestMethod]
	[ExpectedException(typeof(InvalidOperationException))]
	public void GetExportedTypes_NullRefException()
	{
		var exception = new NullReferenceException();
		ServiceCollectionExtensions.GetExportedTypes(new AssemblyTestClass(exception));
	}
}
