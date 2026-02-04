using System.Threading.Tasks;

namespace starsky.feature.thumbnail.Interfaces;

public interface IManualThumbnailGenerationService
{
	Task ManualBackgroundQueue(string subPath);
}
