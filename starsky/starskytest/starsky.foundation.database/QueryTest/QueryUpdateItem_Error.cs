using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;

namespace starskytest.starsky.foundation.database.QueryTest
{
	[TestClass]
	public class QueryUpdateItem_Error
	{
		public static bool IsCalled { get; set; }
		// private class MyClass2 : EntityEntry
		// {
		// 	public MyClass2(InternalEntityEntry internalEntry) : base(internalEntry)
		// 	{
		// 	}
		// }
		private class MyClass : IUpdateEntry
		{
			public void SetOriginalValue(IProperty property, object value)
			{
				throw new System.NotImplementedException();
			}
		
			public void SetPropertyModified(IProperty property)
			{
				throw new System.NotImplementedException();
			}
		
			public bool IsModified(IProperty property)
			{
				throw new System.NotImplementedException();
			}
		
			public bool HasTemporaryValue(IProperty property)
			{
				throw new System.NotImplementedException();
			}
		
			public bool IsStoreGenerated(IProperty property)
			{
				throw new System.NotImplementedException();
			}
		
			public object GetCurrentValue(IPropertyBase propertyBase)
			{
				throw new System.NotImplementedException();
			}
		
			public object GetOriginalValue(IPropertyBase propertyBase)
			{
				throw new System.NotImplementedException();
			}
		
			public TProperty GetCurrentValue<TProperty>(IPropertyBase propertyBase)
			{
				throw new System.NotImplementedException();
			}
		
			public TProperty GetOriginalValue<TProperty>(IProperty property)
			{
				throw new System.NotImplementedException();
			}
		
			public void SetStoreGeneratedValue(IProperty property, object value)
			{
				throw new System.NotImplementedException();
			}
		
			public EntityEntry ToEntityEntry()
			{
				IsCalled = true;
				throw new DbUpdateConcurrencyException();
				// System.NullReferenceException: Object reference not set to an instance of an object.
			}
		
			public IEntityType EntityType { get; }
			public EntityState EntityState { get; set; }
			public IUpdateEntry SharedIdentityEntry { get; }
		}
        
		private class TestClass : ApplicationDbContext
		{
			public TestClass(DbContextOptions options) : base(options)
			{
			}

			public override int SaveChanges()
			{
				throw new DbUpdateConcurrencyException("t",
					new List<IUpdateEntry>{new MyClass()});
		}
		}


		[TestMethod]
		public void Query_UpdateItem_DbUpdateConcurrencyException()
		{
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(databaseName: "MovieListDatabase")
				.Options;
			
			var fakeQuery = new Query(new TestClass(options));
			fakeQuery.UpdateItem(new FileIndexItem());
			
			Assert.IsTrue(IsCalled);
		}
		
		private class MyClass3 : PropertyValues
		{
			public MyClass3(InternalEntityEntry internalEntry) : base(internalEntry)
			{
			}

			public override object ToObject()
			{
				throw new NotImplementedException();
			}

			public override void SetValues(object obj)
			{
				throw new NotImplementedException();
			}

			public override PropertyValues Clone()
			{
				throw new NotImplementedException();
			}

			public override void SetValues(PropertyValues propertyValues)
			{
				throw new NotImplementedException();
			}

			public override TValue GetValue<TValue>(string propertyName)
			{
				throw new NotImplementedException();
			}

			public override TValue GetValue<TValue>(IProperty property)
			{
				throw new NotImplementedException();
			}

			public override IReadOnlyList<IProperty> Properties { get; } =
				new List<IProperty>();

			public override object this[string propertyName]
			{
				get => throw new NotImplementedException();
				set => throw new NotImplementedException();
			}

			public override object this[IProperty property]
			{
				get => throw new NotImplementedException();
				set => throw new NotImplementedException();
			}
		}

		public bool IsWritten2 { get; set; }

		[TestMethod]
		public void SolveConcurrencyException_should_callDelegate()
		{
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(databaseName: "MovieListDatabase")
				.Options;
			
			var fakeQuery = new Query(new TestClass(options));

			fakeQuery.SolveConcurrencyException(new FileIndexItem(),
				new MyClass3(null), new MyClass3(null),
				"", values => IsWritten2 = true);
			
			Assert.IsTrue(IsCalled);
		}
		
		[TestMethod]
		[ExpectedException(typeof(NotSupportedException))]
		public void Query_UpdateItem_DbUpdateConcurrencyException22()
		{
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(databaseName: "MovieListDatabase")
				.Options;
			
			var fakeQuery = new Query(new TestClass(options));

			fakeQuery.SolveConcurrencyException(null,
				new MyClass3(null), new MyClass3(null),
				"", values => IsWritten2 = true);
			// expect error
		}
	}
}
