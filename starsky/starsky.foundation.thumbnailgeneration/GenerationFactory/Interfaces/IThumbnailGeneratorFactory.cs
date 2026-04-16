using starsky.foundation.thumbnailgeneration.GenerationFactory.Generators.Interfaces;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.Interfaces;

/// <summary>
/// Please use IThumbnailService instead of this factory directly,
/// it will be used internally to get the right generator for the file type.
/// </summary>
public interface IThumbnailGeneratorFactory
{
	/// <summary>
	/// Please use ThumbnailService instead of this factory directly
	/// </summary>
	/// <param name="filePath"></param>
	/// <returns></returns>
	IThumbnailGenerator GetGenerator(string filePath);
}
