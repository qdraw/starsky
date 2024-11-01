using System.Resources.NetStandard;
using System.Text.Json;

public static class JsonToResxConverter
{
	public static void ConvertJsonToResx(string jsonFilePath, string resxFilePath)
	{
		// Read and parse the JSON file
		var jsonString = File.ReadAllText(jsonFilePath);
		var translations =
			JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(jsonString);

		var languages = new List<string> {string.Empty, "nl", "en", "de" };

		foreach ( var language in languages )
		{
			// Create a .resx file and write the translations
			var path = resxFilePath;
			if ( language != string.Empty )
			{
				path = resxFilePath.Replace(".resx", $".{language}.resx");
			}
			
			Console.WriteLine(path);
			using ( var resxWriter = new ResXResourceWriter(path) )
			{
				foreach ( var entry in translations )
				{
					var value = entry.Value["en"];
					if ( language != string.Empty )
					{
						value = entry.Value[language];
					}

					resxWriter.AddResource(entry.Key, value);
				}
			}
		}
	}
}
