using starsky.Models;

namespace starsky.Interfaces
{
    public interface IExiftool
    {
        ExifToolModel Update(ExifToolModel updateModel, string fullFilePath);
        ExifToolModel Info(string fullFilePath);
    }
}