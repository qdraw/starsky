namespace starsky.feature.language;

public class CustomCultureComparer : IComparer<string>
{
	private readonly List<string> _customOrder;

	public CustomCultureComparer(List<string> customOrder)
	{
		_customOrder = customOrder;
	}

	public int Compare(string? x, string? y)
	{
		var indexX = _customOrder.IndexOf(x);
		var indexY = _customOrder.IndexOf(y);

		if ( indexX == -1 )
		{
			indexX = int.MaxValue;
		}

		if ( indexY == -1 )
		{
			indexY = int.MaxValue;
		}

		return indexX.CompareTo(indexY);
	}
}
