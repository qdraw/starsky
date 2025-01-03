using System.IO;
using starsky.foundation.platform.Helpers;

namespace starsky.foundation.thumbnailgeneration.Helpers;

public static class ErrorLogItemFullPath
{
	internal static string GetErrorLogItemFullPath(string subPath)
	{
		return FilenamesHelper.GetParentPath(subPath)
		       + "/"
		       + "_"
		       + Path.GetFileNameWithoutExtension(PathHelper.GetFileName(subPath))
		       + ".log";
	}
}
