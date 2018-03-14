using System;
using System.IO;
using System.Linq;
using starsky.Models;
using SkiaSharp;

namespace starsky.Services
{
    public static class Thumbnail
    {
        private static SKBitmap RotateAndFlip(SKBitmap original, SKCodecOrigin origin)
        {
            // these are the origins that represent a 90 degree turn in some fashion
            var differentOrientations = new SKCodecOrigin[]
            {
        SKCodecOrigin.LeftBottom,
        SKCodecOrigin.LeftTop,
        SKCodecOrigin.RightBottom,
        SKCodecOrigin.RightTop
            };

            // check if we need to turn the image
            bool isDifferentOrientation = differentOrientations.Any(o => o == origin);

            // define new width/height
            var width = isDifferentOrientation ? original.Height : original.Width;
            var height = isDifferentOrientation ? original.Width : original.Height;

            var bitmap = new SKBitmap(width, height, original.AlphaType == SKAlphaType.Opaque);

            // todo: the stuff in this switch statement should be rewritten to use pointers
            switch (origin)
            {
                case SKCodecOrigin.LeftBottom:

                    for (var x = 0; x < original.Width; x++)
                        for (var y = 0; y < original.Height; y++)
                            bitmap.SetPixel(y, original.Width - 1 - x, original.GetPixel(x, y));
                    break;

                case SKCodecOrigin.RightTop:

                    for (var x = 0; x < original.Width; x++)
                        for (var y = 0; y < original.Height; y++)
                            bitmap.SetPixel(original.Height - 1 - y, x, original.GetPixel(x, y));
                    break;

                case SKCodecOrigin.RightBottom:

                    for (var x = 0; x < original.Width; x++)
                        for (var y = 0; y < original.Height; y++)
                            bitmap.SetPixel(original.Height - 1 - y, original.Width - 1 - x, original.GetPixel(x, y));

                    break;

                case SKCodecOrigin.LeftTop:

                    for (var x = 0; x < original.Width; x++)
                        for (var y = 0; y < original.Height; y++)
                            bitmap.SetPixel(y, x, original.GetPixel(x, y));
                    break;

                case SKCodecOrigin.BottomLeft:

                    for (var x = 0; x < original.Width; x++)
                        for (var y = 0; y < original.Height; y++)
                            bitmap.SetPixel(x, original.Height - 1 - y, original.GetPixel(x, y));
                    break;

                case SKCodecOrigin.BottomRight:

                    for (var x = 0; x < original.Width; x++)
                        for (var y = 0; y < original.Height; y++)
                            bitmap.SetPixel(original.Width - 1 - x, original.Height - 1 - y, original.GetPixel(x, y));
                    break;

                case SKCodecOrigin.TopRight:

                    for (var x = 0; x < original.Width; x++)
                        for (var y = 0; y < original.Height; y++)
                            bitmap.SetPixel(original.Width - 1 - x, y, original.GetPixel(x, y));
                    break;

            }

            original.Dispose();

            return bitmap;
        }

        private static SKBitmap LoadBitmap(Stream stream, out SKCodecOrigin origin)
        {
            using (var s = new SKManagedStream(stream))
            {
                using (var codec = SKCodec.Create(s))
                {
                    origin = codec.Origin;
                    var info = codec.Info;
                    var bitmap = new SKBitmap(info.Width, info.Height, SKImageInfo.PlatformColorType, info.IsOpaque ? SKAlphaType.Opaque : SKAlphaType.Premul);

                    IntPtr length;
                    var result = codec.GetPixels(bitmap.Info, bitmap.GetPixels(out length));
                    if (result == SKCodecResult.Success || result == SKCodecResult.IncompleteInput)
                    {
                        return bitmap;
                    }
                    else
                    {
                        throw new ArgumentException("Unable to load bitmap from provided data");
                    }
                }
            }
        }
        public static FileIndexItem CreateThumb(FileStream fs, FileIndexItem item)
        {
            // input: Files.PathToFull(item.FilePath))
            var imagePath = Files.PathToFull(item.FilePath);

            SKCodecOrigin origin; // this represents the EXIF orientation
            var bitmap = LoadBitmap(File.OpenRead(imagePath), out origin); // always load as 32bit (to overcome issues with indexed color)

            // if autorotate = true, and origin isn't correct for the rotation, rotate it
            if (origin != SKCodecOrigin.TopLeft) bitmap = RotateAndFlip(bitmap, origin);

            // resize
            var height = (int)Math.Round(bitmap.Height * (float)1000 / bitmap.Width); ;
            var resizedImageInfo = new SKImageInfo(1000, height, SKImageInfo.PlatformColorType, bitmap.AlphaType);
            var resizedBitmap = bitmap.Resize(resizedImageInfo, SKBitmapResizeMethod.Lanczos3);

            var resizedImage = SKImage.FromBitmap(resizedBitmap);
            SKData imageData;
            imageData = resizedImage.Encode(SKEncodedImageFormat.Jpeg, 75);

            // cleanup
            resizedImage.Dispose();
            bitmap.Dispose();
            resizedBitmap.Dispose();

            imageData.SaveTo(fs);

            Console.Write("%");

            return item;
        }

        public static FileIndexItem CreateThumb(FileIndexItem item)
        {
            if (!System.IO.Directory.Exists(AppSettingsProvider.ThumbnailTempFolder))
            {
                throw new FileNotFoundException("ThumbnailTempFolder not found "+ AppSettingsProvider.ThumbnailTempFolder);
            }

            var thumbPath = AppSettingsProvider.ThumbnailTempFolder + item.FileHash + ".jpg";

            if (!System.IO.File.Exists(Files.PathToFull(item.FilePath)))
            {
                Console.WriteLine("File Not found: " + item.FilePath);
                return null;
            }

                


            if (System.IO.File.Exists(thumbPath))
            {
                return null;
            }


            FileStream stream = new FileStream(
                thumbPath,
                System.IO.FileMode.Create);

            try
            {
                return CreateThumb(stream, item ); ;
            }
            finally
            {
                stream.Close();
            }
        }

    }
}
