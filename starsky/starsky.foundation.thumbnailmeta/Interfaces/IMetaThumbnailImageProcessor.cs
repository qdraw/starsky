using System.IO;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Enums;

namespace starsky.foundation.thumbnailmeta.Interfaces;

/// <summary>
///     Image work of the thumbnail that is embedded in the meta data of an image
/// </summary>
public interface IMetaThumbnailImageProcessor
{
	/// <summary>
	///     Crop away the (letterbox) borders, resize, rotate and encode
	/// </summary>
	/// <param name="thumbnail">the embedded thumbnail</param>
	/// <param name="sourceWidth">width of the original image</param>
	/// <param name="sourceHeight">height of the original image</param>
	/// <param name="rotation">rotate the pixels</param>
	/// <param name="imageFormat">output format</param>
	/// <param name="output">stream to write to</param>
	void CropResizeRotate(Stream thumbnail, int sourceWidth, int sourceHeight,
		ImageRotation.Rotation rotation, ThumbnailImageFormat imageFormat, Stream output);
}
