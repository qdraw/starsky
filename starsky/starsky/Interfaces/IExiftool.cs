using System.Collections.Generic;
using starsky.Models;

namespace starsky.Interfaces
{
    public interface IExiftool
    {
        void Update(ExifToolModel updateModel, List<string> inputFullFilePaths);
        void Update(ExifToolModel updateModel, string inputFullFilePath);
    }
}