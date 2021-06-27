namespace starsky.foundation.metathumbnail.Helpers
{
	public static class NewImageSize
	{
		
		public class ImageSizeModel
		{
			public ImageSizeModel(int destWidth, int destHeight, 
				int destX,  int destY)
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
		
		/// <summary>
		/// @see: https://stackoverflow.com/a/2001462
		/// </summary>
		/// <param name="smallWidth"></param>
		/// <param name="smallHeight"></param>
		/// <param name="sourceWidth"></param>
		/// <param name="sourceHeight"></param>
		/// <returns></returns>
		public static ImageSizeModel NewImageSizeCalc(int smallWidth, int smallHeight, int sourceWidth, int sourceHeight)
		{
			float nPercent = 0;
			float nPercentW = 0;
			float nPercentH = 0;
			int destX = 0;
			int destY = 0;
			
			nPercentW = ((float)smallWidth / (float)sourceWidth);
			nPercentH = ((float)smallHeight / (float)sourceHeight);
			if (nPercentH < nPercentW)
			{
				nPercent = nPercentH;
				destX = System.Convert.ToInt16((smallWidth -
				                                (sourceWidth * nPercent)) / 2);
			}
			else
			{
				nPercent = nPercentW;
				destY = System.Convert.ToInt16((smallHeight -
				                                (sourceHeight * nPercent)) / 2);
			}

			int destWidth = (int)(sourceWidth * nPercent);
			int destHeight = (int)(sourceHeight * nPercent);

			return new ImageSizeModel( destWidth, destHeight, destX,  destY);
		}
	}
}
