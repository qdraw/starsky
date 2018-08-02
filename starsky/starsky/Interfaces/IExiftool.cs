using System.Collections.Generic;
using starsky.Models;

namespace starsky.Interfaces
{
    public interface IExiftool
    {
        ExifToolModel Update(ExifToolModel updateModel, List<string> inputFullFilePaths);
        ExifToolModel Update(ExifToolModel updateModel, string inputFullFilePath);
        ExifToolModel Info(string fullFilePath);
    }
}