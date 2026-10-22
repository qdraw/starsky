namespace starsky.foundation.thumbnailmeta.Interfaces;

/// <summary>
///     The one place that decides which image library is used for the meta thumbnail
/// </summary>
public interface IMetaThumbnailImageProcessorFactory
{
	IMetaThumbnailImageProcessor Create();
}
