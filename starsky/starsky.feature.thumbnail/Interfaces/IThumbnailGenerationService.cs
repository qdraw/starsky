using System.Threading.Tasks;

namespace starsky.feature.thumbnail.Interfaces;

public interface IThumbnailGenerationService
{
	Task BgQueue(string subPath);
}
