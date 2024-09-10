using System;
using System.IO;
using Serilog;
using static SimpleExec.Command;
using static build.Build;

namespace helpers
{

	public static class ClientHelper
	{
		public static string GetClientAppFolder()
		{
			var baseDirectory = AppDomain.CurrentDomain
				.BaseDirectory;
			if ( baseDirectory == null )
			{
				throw new DirectoryNotFoundException("base directory is null, this is wrong");
			}

			var rootDirectory = Directory.GetParent(baseDirectory)?.Parent?.Parent?.Parent?.FullName;
			if ( rootDirectory == null )
			{
				throw new DirectoryNotFoundException("rootDirectory is null, this is wrong");
			}

			return Path.Combine(rootDirectory, ClientAppFolder);
		}

		public static void NpmPreflight()
		{
			Log.Information("Checking if Npm (and implicit: Node) is installed, will fail if not on this step");
			Run(NpmBaseCommand, "-v");

			Log.Information("Checking if Node is installed, will fail if not on this step");
			Run(NodeBaseCommand, "-v");
		}

		public static void ClientCiCommand()
		{
			Run(NpmBaseCommand, "ci --legacy-peer-deps --prefer-offline --no-audit --no-fund", GetClientAppFolder());
		}

		public static void ClientBuildCommand()
		{
			Run(NpmBaseCommand, "run build", GetClientAppFolder());
		}

		public static void ClientTestCommand()
		{
			Run(NpmBaseCommand, "run test:ci", GetClientAppFolder());
		}
	}
}
