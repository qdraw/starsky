using starskycore.Models;

namespace starskycore.Interfaces
{
    public interface IExiftool
    {
        string BaseCommmand(string options, string fullFilePathSpaceSeperated);
        ExifToolModel Info(string fullFilePath);
    }
}