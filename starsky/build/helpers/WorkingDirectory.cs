using System.IO;

namespace helpers;

public static class WorkingDirectory
{
	public static string  GetSolutionParentFolder()
	{
		var strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
		return Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(strExeFilePath))));
	}
}
