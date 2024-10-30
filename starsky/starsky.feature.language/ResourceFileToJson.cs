using System.Globalization;
using System.Reflection;
using System.Text.Json;

namespace starsky.feature.language;

public class Class1
{
	public Class1()
	{
		var properties =
			typeof(LanguageContent).GetProperties(BindingFlags.Static | BindingFlags.NonPublic);
		var cultures = GetAllCultures.CulturesOfResource<LanguageContent>()
			.OrderBy(p => p.Name);
		var translations = new Dictionary<string, Dictionary<string, string>>();

		foreach ( var property in properties )
		{
			if ( property.PropertyType != typeof(string) )
			{
				continue;
			}

			var propertyTranslations = new Dictionary<string, string>();

			foreach ( var culture in cultures )
			{
				LanguageContent.Culture = new CultureInfo(culture.Name);
				var value = property.GetValue(null) as string;
				propertyTranslations[culture.Name] = value;

				// Console.WriteLine($"{property.Name} ({culture}): {value}");
			}

			translations[property.Name] = propertyTranslations;
		}

		var json = JsonSerializer.Serialize(translations,
			new JsonSerializerOptions { WriteIndented = true });
		Console.WriteLine(json);
	}
}
