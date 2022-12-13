using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace starsky.foundation.database.Helpers;

public class DoubleBinderProvider : IModelBinderProvider
{
	public IModelBinder? GetBinder(ModelBinderProviderContext context)
	{
		if (context == null)
		{
			throw new ArgumentNullException(nameof(context));
		}

		return context.Metadata.ModelType == typeof(double) ? new DoubleModelBinder() : null;
	}
}

public class DoubleModelBinder : IModelBinder
{
	public Task BindModelAsync(ModelBindingContext bindingContext)
	{
		var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

		var value = valueProviderResult.FirstValue;

		if (string.IsNullOrEmpty(value))
		{
			return Task.CompletedTask;
		}

		// Remove unnecessary commas and spaces
		value = value.Replace(",", string.Empty).Trim();

		try
		{
			var myValue = Convert.ToDouble(value, CultureInfo.InvariantCulture);
			bindingContext.Result = ModelBindingResult.Success(myValue);
			return Task.CompletedTask;
		}
		catch (Exception)
		{
			return Task.CompletedTask;                
		}
           
	}
}
