using System.Collections.Generic;
using System.Threading.Tasks;

namespace starsky.foundation.metathumbnail.Interfaces;

public interface IMetaUpdateStatusThumbnailService
{
	Task UpdateStatusThumbnail(List<(bool, string)> statusList);
}
