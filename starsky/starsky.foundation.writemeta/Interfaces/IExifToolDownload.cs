using System.Collections.Generic;
using System.Threading.Tasks;

namespace starsky.foundation.writemeta.Interfaces
{
	public interface IExifToolDownload
	{
		Task<List<bool>> DownloadExifTool(List<string> architectures);
		Task<bool> DownloadExifTool(bool isWindows, int minimumSize = 30);
	}
}
