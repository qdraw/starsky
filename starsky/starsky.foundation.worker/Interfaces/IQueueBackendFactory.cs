namespace starsky.foundation.worker.Interfaces;

public interface IQueueBackendFactory
{
	IBaseBackgroundTaskQueue Create(string queueName);
}

