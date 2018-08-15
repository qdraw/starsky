using starsky.Interfaces;
using starsky.Models;

namespace starsky.Services
{
    public partial class ReadMeta : IReadMeta
    {
        public FileIndexItem ReadExifAndXmpFromFile(string singleFilePath)
        {
            var databaseItem = ReadExifFromFile(singleFilePath);
            databaseItem = XmpGetSidecarFile(databaseItem, singleFilePath);
            return databaseItem;
        }
    }
}