using starsky.foundation.metaupdate.Models;

namespace starsky.foundation.metaupdate.Interfaces;

public interface IExifTimezoneDisplayListService
{
	List<ExifTimezoneDisplay> GetMovedToDifferentPlaceTimezonesList(DateTime dateTime,
		string query);
}
