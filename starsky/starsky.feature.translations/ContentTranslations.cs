using System.Collections;
using System.Resources.NetStandard;

namespace starsky.feature.translations;

public class ContentTranslations
{
	private const string Home = "Home";
	private const string PhotosOfTheWeek = "PhotosOfTheWeek";
	private const string Trash = "Trash";
	private const string Import = "Import";
	private const string Preferences = "Preferences";
	private const string Logout = "Logout";
	
	public ContentTranslations()
	{
		using (ResXResourceWriter resx = new ResXResourceWriter(@".\ContentTranslations.resx"))
		{
			resx.AddResource(Home, "Classic American Cars");
			resx.AddResource(PhotosOfTheWeek, "Make");
			resx.AddResource(Trash, "Model");
			resx.AddResource(Import, "Year");
			resx.AddResource(Preferences, "Doors");
			resx.AddResource(Logout, "Cylinders");
		}
	}
	
	public Dictionary<string, string?> GetDictionary()
	{
		using (ResXResourceReader resx = new ResXResourceReader(@".\ContentTranslations.resx", CultureInfo.InvariantCulture))
		{
			return resx.Cast<DictionaryEntry>().ToDictionary(x => x.Key.ToString(), x => x.Value.ToString());
		}
	}
	
}


