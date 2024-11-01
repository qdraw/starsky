using System.Collections.Concurrent;
using System.Globalization;
using System.Resources;

namespace starsky.feature.language;

public static class GetAllCultures
{
	private static readonly ConcurrentDictionary<Type, List<CultureInfo>>
		ResourceCultures = new();

	/// <summary>
	///     Return the list of cultures that is supported by a Resource Assembly (usually collection of
	///     resx files).
	/// </summary>
	public static List<CultureInfo> CulturesOfResource<T>()
	{
		return ResourceCultures.GetOrAdd(typeof(T), t =>
		{
			var manager = new ResourceManager(t);
			return CultureInfo.GetCultures(CultureTypes.AllCultures)
				.Where(c => !c.Equals(CultureInfo.InvariantCulture) &&
				            manager.GetResourceSet(c, true, false) != null)
				.ToList();
		});
	}
}
