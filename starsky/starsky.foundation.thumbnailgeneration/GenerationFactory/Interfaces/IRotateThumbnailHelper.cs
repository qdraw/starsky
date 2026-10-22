using System.Threading.Tasks;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.Interfaces;

/// <summary>
///     Rotate a thumbnail, get an instance with IImageProviderFactory
/// </summary>
public interface IRotateThumbnailHelper
{
	/// <param name="fileHash">name of the thumbnail</param>
	/// <param name="orientation">-1 > Rotate -90degrees, anything else 90 degrees</param>
	/// <param name="width">to resize, default 1000</param>
	/// <param name="height">to resize, default keep ratio (0)</param>
	/// <returns>Is successful?</returns>
	Task<bool> RotateThumbnail(string fileHash, int orientation, int width = 1000,
		int height = 0);
}
