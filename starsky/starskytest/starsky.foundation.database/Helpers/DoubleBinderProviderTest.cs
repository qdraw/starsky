using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Helpers;
using starsky.foundation.platform.Exceptions;

namespace starskytest.starsky.foundation.database.Helpers;

[TestClass]
public class DoubleBinderProviderTest
{
	public class MyClass :ModelBinderProviderContext
	{
		public override IModelBinder CreateBinder(ModelMetadata metadata)
		{
			return null;
		}

		public override BindingInfo BindingInfo { get; }

		public override ModelMetadata Metadata { get; }
		public override IModelMetadataProvider MetadataProvider { get; }
	}
	
	[TestMethod]
	public void DoubleBinderProviderTest_1()
	{
		var doubleBinderProvider = new DoubleBinderProvider();
		
		var result = doubleBinderProvider.GetBinder(new MyClass());
		Assert.IsNull(result);
	}
	
	[TestMethod]
	[ExpectedException(typeof(ArgumentNullException))]
	public void DoubleBinderProviderTest_2_Null()
	{
		var doubleBinderProvider = new DoubleBinderProvider();
		
		var result = doubleBinderProvider.GetBinder(null!);
		Assert.IsNull(result);
	}

	[TestMethod]
	public void DoubleModelBinder_DefaultFlow()
	{
		var binder = new DefaultModelBindingContext();
		binder.ModelName = "test";
		binder.ValueProvider = new QueryStringValueProvider(new BindingSource("query", "query", false, true), 
			new QueryCollection(new Dictionary<string, StringValues> {{"test", "1.1"}}), System.Globalization.CultureInfo.InvariantCulture);
		new DoubleModelBinder().BindModelAsync(binder);
		Assert.AreEqual(1.1, binder.Result.Model);
	}
	
	[TestMethod]
	public void DoubleModelBinder_EmptyString()
	{
		var binder = new DefaultModelBindingContext();
		binder.ModelName = "test";
		binder.ValueProvider = new QueryStringValueProvider(new BindingSource("query", "query", false, true), 
			new QueryCollection(new Dictionary<string, StringValues> {{"test", string.Empty}}), System.Globalization.CultureInfo.InvariantCulture);
		new DoubleModelBinder().BindModelAsync(binder);
		Assert.AreEqual(null, binder.Result.Model);
	}
	
	[TestMethod]
	public void DoubleModelBinder_DefaultFlowComma()
	{
		var binder = new DefaultModelBindingContext();
		binder.ModelName = "test";
		binder.ValueProvider = new QueryStringValueProvider(new BindingSource("query", "query", false, true), 
			new QueryCollection(new Dictionary<string, StringValues> {{"test", "1,1"}}), System.Globalization.CultureInfo.InvariantCulture);
		new DoubleModelBinder().BindModelAsync(binder);
		Assert.AreEqual(11d, binder.Result.Model);
	}
	
		
	[TestMethod]
	public void DoubleModelBinder_NonValidNumber()
	{
		var binder = new DefaultModelBindingContext();
		binder.ModelName = "test";
		binder.ValueProvider = new QueryStringValueProvider(new BindingSource("query", "query", false, true), 
			new QueryCollection(new Dictionary<string, StringValues> {{"test", "___NaN__"}}), System.Globalization.CultureInfo.InvariantCulture);
		new DoubleModelBinder().BindModelAsync(binder);
		Assert.AreEqual(null, binder.Result.Model);
	}
}
