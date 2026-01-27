using starsky.foundation.metaupdate.Models;

namespace starsky.foundation.metaupdate.Interfaces;

public interface IExifTimezoneDisplayListService
{
	List<ExifTimezoneDisplay> GetIncorrectCameraTimezonesList();
	List<ExifTimezoneDisplay> GetMovedToDifferentPlaceTimezonesList(DateTime? dateTime);
}
