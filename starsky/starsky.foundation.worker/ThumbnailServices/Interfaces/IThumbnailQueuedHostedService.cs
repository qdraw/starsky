using starsky.foundation.worker.Interfaces;

namespace starsky.foundation.worker.ThumbnailServices.Interfaces;

public interface IThumbnailQueuedHostedService : IBaseBackgroundTaskQueue
{
	bool ThrowExceptionIfCpuUsageIsToHigh(string metaData);
}
