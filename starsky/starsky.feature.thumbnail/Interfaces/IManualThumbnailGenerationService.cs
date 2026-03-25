using System.Threading.Tasks;

namespace starsky.feature.thumbnail.Interfaces;

public interface IManualThumbnailGenerationService
{
	Task CreateJob(string subPath);
	Task WorkThumbnailGeneration(string subPath);
}
