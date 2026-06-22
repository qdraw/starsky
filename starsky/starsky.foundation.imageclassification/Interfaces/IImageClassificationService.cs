using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;

namespace starsky.foundation.imageclassification.Interfaces;

public interface IImageClassificationService
{
	Task<OllamaCommandResult> ClassifyAndUpdateAsync(FileIndexItem fileIndexItem,
		CancellationToken cancellationToken = default);
}

