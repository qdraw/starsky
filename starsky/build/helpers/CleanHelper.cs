using System;
using System.IO;

namespace helpers;

public static class Clean
{
	public static string GetRootFolder()
	{
		var baseDirectory = AppDomain.CurrentDomain?
			.BaseDirectory;
		if ( baseDirectory == null )
			throw new Exception("base directory is null, this is wrong");
		var rootDirectory = Directory.GetParent(baseDirectory)?.Parent?.Parent?.Parent?.FullName;
		if ( rootDirectory == null )
			throw new Exception("rootDirectory is null, this is wrong");
		return rootDirectory;
	}
	
	public static void Test()
	{
		// foreach(var runtime in GetRuntimesWithoutGeneric())
		// {
		// 	if (File.Exists($"starsky-{runtime}.zip"))
		// 	{
		// 		File.Delete($"starsky-{runtime}.zip");
		// 	}
		// 			
		// 	if (Directory.Exists($"starsky-{runtime}.zip"))
		// 	{
		// 		Directory.Delete($"starsky-{runtime}.zip");
		// 	}
		// 			
		// 	var distDirectory = Directory($"./{runtime}");
		// 	CleanDirectory(distDirectory);
		//
		// 	CleanDirectory($"obj/Release/netcoreapp3.1/{runtime}");
		//
		// }
	}
}
