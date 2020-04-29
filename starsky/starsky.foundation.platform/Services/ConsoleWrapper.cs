using System;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;

namespace starsky.foundation.platform.Services
{
	[Service(typeof(IConsole), InjectionLifetime = InjectionLifetime.Scoped)]
	public class ConsoleWrapper : IConsole
	{
		public void Write(string message)
		{
			Console.Write(message);
		}

		public void WriteLine(string message)
		{
			Console.WriteLine(message);
		}

		public string ReadLine()
		{
			return Console.ReadLine();
		}
	}
}
