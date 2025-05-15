using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.thumbnailgeneration.Models;

namespace starsky.feature.thumbnail.Interfaces;

public interface IThumbnailSocketService
{
	Task NotificationSocketUpdate(string subPath,
		List<GenerationResultModel> generateThumbnailResults);
}
