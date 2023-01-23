using System;
using System.Threading.Tasks;

namespace starsky.feature.thumbnail.Interfaces;

public interface IDatabaseThumbnailGenerationService
{
	Task StartBackgroundQueue(DateTime startTime, TimeSpan timeout);
}
