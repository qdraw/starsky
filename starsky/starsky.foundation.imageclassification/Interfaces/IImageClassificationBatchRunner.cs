namespace starsky.foundation.imageclassification.Interfaces;

public interface IImageClassificationBatchRunner
{
	Task<int> EnqueueBatchAsync(CancellationToken cancellationToken = default);
}

