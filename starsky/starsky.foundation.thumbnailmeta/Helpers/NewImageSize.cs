using System;

namespace starsky.foundation.thumbnailmeta.Helpers;

public static class NewImageSize
{
	/// <summary>
	///     @see: https://stackoverflow.com/a/2001462
	/// </summary>
	/// <param name="smallWidth"></param>
	/// <param name="smallHeight"></param>
	/// <param name="sourceWidth"></param>
	/// <param name="sourceHeight"></param>
	/// <returns></returns>
	public static ImageSizeModel NewImageSizeCalc(int smallWidth, int smallHeight, int sourceWidth,
		int sourceHeight)
	{
		float nPercent;
		var destX = 0;
		var destY = 0;

		var nPercentW = smallWidth / ( float ) sourceWidth;
		var nPercentH = smallHeight / ( float ) sourceHeight;
		if ( nPercentH < nPercentW )
		{
			nPercent = nPercentH;
			destX = Convert.ToInt16(( smallWidth -
			                          sourceWidth * nPercent ) / 2);
		}
		else
		{
			nPercent = nPercentW;
			destY = Convert.ToInt16(( smallHeight -
			                          sourceHeight * nPercent ) / 2);
		}

		var destWidth = ( int ) ( sourceWidth * nPercent );
		var destHeight = ( int ) ( sourceHeight * nPercent );

		return new ImageSizeModel(destWidth, destHeight, destX, destY);
	}

	public class ImageSizeModel
	{
		public ImageSizeModel(int destWidth, int destHeight,
			int destX, int destY)
		{
			DestWidth = destWidth;
			DestHeight = destHeight;
			DestX = destX;
			DestY = destY;
		}

		public int DestWidth { get; set; }
		public int DestHeight { get; set; }
		public int DestX { get; set; }
		public int DestY { get; set; }
	}
}
