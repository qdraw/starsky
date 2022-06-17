using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using build;

namespace helpers;

public class ZipperHelper
{

	public const string ZipPrefix = "starsky-";
	
	public static void ZipGeneric()
	{
		if ( !Directory.Exists(Build.GenericRuntimeName) )
		{
			throw new Exception($"dir {Build.GenericRuntimeName} not found");
		}

		if ( File.Exists(ZipPrefix + Build.GenericRuntimeName + ".zip") )
		{
			File.Delete(ZipPrefix + Build.GenericRuntimeName + ".zip");
		}
		
		Console.WriteLine($"next: {Build.GenericRuntimeName} zip");
		ZipFile.CreateFromDirectory(Build.GenericRuntimeName, 
			ZipPrefix + Build.GenericRuntimeName + ".zip");
	}
	
	public static void ZipRuntimes(List<string> getRuntimesWithoutGeneric)
	{
		if ( !getRuntimesWithoutGeneric.Any() )
		{
			Console.WriteLine("There are no runtime specific items selected");
			return;
		}

		foreach ( var runtime in getRuntimesWithoutGeneric )
		{
			if ( !Directory.Exists(Build.GenericRuntimeName) )
			{
				throw new Exception($"dir {Build.GenericRuntimeName} not found");
			}

			if ( File.Exists(ZipPrefix + Build.GenericRuntimeName + ".zip") )
			{
				File.Delete(ZipPrefix + Build.GenericRuntimeName + ".zip");
			}

			Console.WriteLine($"next: {runtime} zip");
			ZipFile.CreateFromDirectory(runtime, 
				ZipPrefix + Build.GenericRuntimeName + ".zip");
		}
	}
}
