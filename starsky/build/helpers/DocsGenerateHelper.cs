using System;
using System.Collections.Generic;
using System.IO;
using static SimpleExec.Command;
using static build.Build;

namespace helpers;

public static class DocsGenerateHelper
{
	public static void Docs(List<string> runtimes)
	{
		if (!Directory.Exists(DocsPath()))
		{
			Console.WriteLine($"Docs generation disabled (folder does not exist)");
			return;
		}

		Console.WriteLine("next: call docs build script");
		// in build npm command ci is called
		Run(NpmBaseCommand, "run build", DocsPath());

		// copy to build directory
		foreach(var runtime in runtimes)
		{
			if(!Directory.Exists($"./{runtime}")) {
				Console.WriteLine($"runtime {runtime} does not exists");
				continue;
			}

			var docsDistDirectory = System.IO.Path.Combine(Environment.CurrentDirectory, runtime, "docs");
			Console.WriteLine("copy to: " + docsDistDirectory);
			
			Run(NpmBaseCommand, $"run copy {docsDistDirectory}", DocsPath());
		}
		
	}

	static string  DocsPath()
	{
		return "../starsky-tools/docs/";
	}
}
