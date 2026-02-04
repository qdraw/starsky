using System.Threading.Tasks;

namespace starsky.feature.thumbnail.Interfaces;

public interface ISmallThumbnailBackgroundJobService
{
	Task<bool> CreateJob(bool? isAuthenticated, string? filePath);
}
