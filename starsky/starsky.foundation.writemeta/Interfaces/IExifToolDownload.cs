using System.Threading.Tasks;

namespace starsky.foundation.writemeta.Interfaces
{
	public interface IExifToolDownload
	{
		Task<bool> DownloadExifTool(bool isWindows);
	}
}
