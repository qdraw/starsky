namespace starsky.foundation.database.Models;

public sealed class PageViewType
{
	public enum PageType
	{
		Unknown = -1,
		Archive = 1, // index
		DetailView = 2,
		Search = 3,
		Trash = 4
	}
}
